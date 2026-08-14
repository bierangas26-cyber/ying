using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class ReJi : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("StrengthPerCard", 2m),
        new EnergyVar(1),
        new DynamicVar("RellyPerCard", 8m),
        new DynamicVar("GroupCost", 15m)
    ];

    public ReJi() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int totalCleared = 0;
        foreach (var enemy in base.CombatState.HittableEnemies)
        {
            var power = enemy.GetPower<ShiJianZhiHuanPower>();
            if (power != null && power.Amount > 0)
            {
                totalCleared += power.Amount;
                power.SetAmount(0, silent: true);
            }
        }

        int groupCost = (int)base.DynamicVars["GroupCost"].BaseValue;
        int groups = totalCleared / groupCost;
        if (groups == 0) return;

        int strPerGroup = (int)base.DynamicVars["StrengthPerCard"].BaseValue;
        int enPerGroup = (int)base.DynamicVars.Energy.BaseValue;
        int rellyPerGroup = (int)base.DynamicVars["RellyPerCard"].BaseValue;

        int totalStr = groups * strPerGroup;
        int totalEn = groups * enPerGroup;
        int totalRelly = groups * rellyPerGroup;

        if (totalStr > 0) await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, totalStr, base.Owner.Creature, this);
        if (totalEn > 0) await PlayerCmd.GainEnergy(totalEn, base.Owner);
        if (totalRelly > 0) await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId, totalRelly);

        if (groups >= 5)
        {
            await CreatureCmd.LoseBlock(base.Owner.Creature, base.Owner.Creature.Block);
            foreach (var enemy in base.CombatState.HittableEnemies)
                await CreatureCmd.LoseBlock(enemy, enemy.Block);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["StrengthPerCard"].UpgradeValueBy(1m);
        base.DynamicVars.Energy.UpgradeValueBy(1);
        base.DynamicVars["RellyPerCard"].UpgradeValueBy(6m);
    }
}