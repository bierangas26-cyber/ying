using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Yingmod.Ying.Keywords;

[RegisterOwnedCardKeyword("ZHONG_MO", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class ZhongMoKeywordClass { }

[RegisterOwnedCardKeyword("JING_ZHI", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class JingZhiKeywordClass { }

// ★ 新增：轮回关键词注册
[RegisterOwnedCardKeyword("LUN_HUI", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class LunHuiKeywordClass { }

public class YingKeywords
{
    public static readonly string ZhongMoId = ModContentRegistry.GetQualifiedKeywordId(MainFile.ModId, "ZHONG_MO");
    public static readonly string JingZhiId = ModContentRegistry.GetQualifiedKeywordId(MainFile.ModId, "JING_ZHI");
    // ★ 新增：轮回关键词 ID
    public static readonly string LunHuiId = ModContentRegistry.GetQualifiedKeywordId(MainFile.ModId, "LUN_HUI");

    public static CardKeyword ZhongMo => ZhongMoId.GetModKeywordCardKeyword();
    public static CardKeyword JingZhi => JingZhiId.GetModKeywordCardKeyword();
    // ★ 新增：轮回关键词
    public static CardKeyword LunHui => LunHuiId.GetModKeywordCardKeyword();
}