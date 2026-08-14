using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Capabilities;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class MingZhou : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9m, ValueProp.Move),
        new DynamicVar("Durability", 2m),
        new DynamicVar("BlockGain", 2m)
    ];

    public MingZhou() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 1. 造成伤害
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx(VfxCmd.slashPath)
            .Execute(choiceContext);

        // 2. 生成明昼幻境
        int durability = (int)DynamicVars["Durability"].BaseValue;
        int blockGain = (int)DynamicVars["BlockGain"].BaseValue;

        var manager = Owner.Character.GetOrCreateCapability<IllusionManagerCapability>();
        manager.SetOwnerPlayer(Owner);
        var illusionNode = manager.CreateAndAddIllusionUI("res://Ying/Images/MingZhou.png", durability);

        await PowerCmd.Apply<MingZhouIllusionPower>(choiceContext, Owner.Creature, durability, Owner.Creature, this);

        var powers = Owner.Creature.GetPowerInstances<MingZhouIllusionPower>();
        var power = powers.LastOrDefault();
        if (power != null)
        {
            power.IllusionNode = illusionNode;
            power.Manager = manager;
            power.BlockGain = blockGain;
            manager.AddIllusionPower(power);
        }

        // 3. 打出时选牌复制（自动打出时跳过）
        if (!cardPlay.IsAutoPlay)
        {
            await CopyFromRebirthPile(choiceContext);
        }
    }

    private async Task CopyFromRebirthPile(PlayerChoiceContext choiceContext)
    {
        var rebirthPile = MainFile.RebirthPile.GetPile(Owner);
        if (rebirthPile?.Cards == null) return;

        var eligible = rebirthPile.Cards.Where(c => c.Keywords.Contains(CardKeyword.Exhaust)).ToList();
        if (eligible.Count == 0) return;

        // 必须选择1张牌（无跳过按钮）
        var prefs = new CardSelectorPrefs(
            new LocString("cards", "YING_CARD_MING_ZHOU.select"), 1, 1);
        var selected = await CardSelectCmd.FromCombatPile(
            choiceContext, rebirthPile, Owner, prefs,
            c => c.Keywords.Contains(CardKeyword.Exhaust));

        foreach (var card in selected)
        {
            for (int i = 0; i < 2; i++)
            {
                var clone = card.CreateClone();
                await CardPileCmd.Add(clone, rebirthPile);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["BlockGain"].UpgradeValueBy(1m); // 2 → 3
    }
}