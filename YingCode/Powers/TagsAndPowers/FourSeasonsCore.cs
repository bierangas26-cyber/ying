using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Tags;

// ==========================================
// 1. 注册专属标签 (RitsuLib 自动扫描)
// ==========================================
[RegisterOwnedCardTag("FOUR_SEASONS")]
[RegisterOwnedCardTag("ZHONG_MO")]
public class YingCardTags
{
    public static CardTag FourSeasons => ModContentRegistry.GetQualifiedCardTagId(MainFile.ModId, "FOUR_SEASONS").GetModCardTag();

    public static CardTag ZhongMo => ModContentRegistry.GetQualifiedCardTagId(MainFile.ModId, "ZHONG_MO").GetModCardTag();
}


// ==========================================
// 2. 四时·秋所需的临时力量
// ==========================================
public class TempAutumnStrPower : YingPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == base.Owner.Side)
        {
            // 🌟 严格对齐官方 Apply 签名：第2参数传入单体 Creature，第5参数传入 null 作为 CardModel 占位
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, -base.Amount, base.Owner, null);

            // 🌟 严格对齐官方 Remove 签名：只需传入 1 个参数（能力实例本身）
            await PowerCmd.Remove(this);
        }
    }
}

// ==========================================
// 3. 四时·秋所需的临时敏捷
// ==========================================
public class TempAutumnDexPower : YingPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == base.Owner.Side)
        {
            // 🌟 严格对齐官方 Apply 签名
            await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner, -base.Amount, base.Owner, null);

            // 🌟 严格对齐官方 Remove 签名
            await PowerCmd.Remove(this);
        }
    }
}