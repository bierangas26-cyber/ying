using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class HuaLuoSiShiSheng : YingCard
{
    // 🌟 严格适配：将 ExtraHoverTips 变更为 AdditionalHoverTips 并合并基类
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromCard<FourSeasonsSpring>(base.IsUpgraded),
        HoverTipFactory.FromCard<FourSeasonsSummer>(base.IsUpgraded),
        HoverTipFactory.FromCard<FourSeasonsAutumn>(base.IsUpgraded),
        HoverTipFactory.FromCard<FourSeasonsWinter>(base.IsUpgraded)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public HuaLuoSiShiSheng()
        : base(3, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        SfxCmd.Play("event:/sfx/ui/gain_energy", 1.2f);

        List<CardModel> cardTemplates = [
            ModelDb.Card<FourSeasonsSpring>(),
            ModelDb.Card<FourSeasonsSummer>(),
            ModelDb.Card<FourSeasonsAutumn>(),
            ModelDb.Card<FourSeasonsWinter>()
        ];

        foreach (var template in cardTemplates)
        {
            CardModel newCard = base.CombatState.CreateCard(template, base.Owner);

            if (base.IsUpgraded)
            {
                newCard.UpgradeInternal();
                newCard.FinalizeUpgradeInternal();
            }

            await CardPileCmd.Add(newCard, PileType.Hand);

            await Cmd.Wait(0.15f);
        }
    }

    protected override void OnUpgrade()
    {
    }
}