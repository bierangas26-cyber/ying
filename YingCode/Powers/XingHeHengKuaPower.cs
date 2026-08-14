using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class XingHeHengKuaPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public int EnergyGain { get; set; } = 1;
    public int RellyGain { get; set; } = 10;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只对拥有者打出的牌生效
        if (cardPlay.Card.Owner?.Creature != base.Owner) return;

        // 检查卡牌是否带有“消耗”词条（原先是“静滞”）
        if (!cardPlay.Card.Keywords.Contains(CardKeyword.Exhaust)) return;

        // 获得能量
        if (EnergyGain > 0)
            await PlayerCmd.GainEnergy(EnergyGain, base.Owner.Player);

        // 获得心脏充能（次要资源系统）
        if (RellyGain > 0)
            await SecondaryResourceCmd.Gain(base.Owner.Player, MainFile.RellyId, RellyGain);
    }
}