using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class FenSuiXianShi : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<ShiJianZhiHuanPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(30m, ValueProp.Move),
        new DynamicVar("DebuffAmount", 40m)
    ];

    public FenSuiXianShi()
        : base(0, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 播放自定义动画 "attack2"
        var creatureNode = base.Owner.Creature.GetCreatureNode();
        var animState = creatureNode?.Visuals?.SpineBody?.GetAnimationState();
        if (animState != null)
        {
            animState.SetAnimation("attack2", false, 0);
        }
        await Cmd.CustomScaledWait(1.0f, 1.0f); // 等待动画完成，可根据实际动画长度调整

        // 播放攻击音效（可选）
        SfxCmd.Play(base.Owner.Character.AttackSfx);

        // 造成伤害 + 超能光束特效，禁止默认攻击动画
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithAttackerAnim("", 0)   // 阻止任何自动动画
            .BeforeDamage(async delegate
            {
                // 创建光束扫射特效
                NHyperbeamVfx nHyperbeamVfx = NHyperbeamVfx.Create(base.Owner.Creature, cardPlay.Target);
                if (nHyperbeamVfx != null)
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nHyperbeamVfx);

                    // 微调光束起始位置，向上偏移30像素（可根据实际效果调整此数值）
                    var container = NCombatRoom.Instance?.CombatVfxContainer;
                    if (container != null && container.GetChildCount() > 0)
                    {
                        var beamNode = container.GetChild(container.GetChildCount() - 1) as Node2D;
                        if (beamNode != null)
                        {
                            beamNode.Position += new Vector2(0, -60);
                        }
                    }

                    await Cmd.Wait(0.3f);
                }

                // 创建命中爆炸特效
                NHyperbeamImpactVfx nHyperbeamImpactVfx = NHyperbeamImpactVfx.Create(base.Owner.Creature, cardPlay.Target);
                if (nHyperbeamImpactVfx != null)
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nHyperbeamImpactVfx);
                }
            })
            .Execute(choiceContext);

        // 施加时间滞缓
        await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, cardPlay.Target,
            base.DynamicVars["DebuffAmount"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(6m);
        base.DynamicVars["DebuffAmount"].UpgradeValueBy(1m);
    }
}