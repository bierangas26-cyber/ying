using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using System.Threading.Tasks;
using Yingmod.Ying;
using Yingmod.Ying.Keywords;
using static STS2RitsuLib.Models.HookedSingletonModel;

namespace Yingmod.Ying.Singletons;

[RegisterSingleton]
public class LunHuiKeywordSingleton : HookedSingletonModel
{
    public LunHuiKeywordSingleton() : base(HookType.Combat) { }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card.Keywords.Contains(YingKeywords.LunHui))
        {
            var clone = card.CreateClone();
            // ★ 移除复制品上的“轮回”词条，防止无限连锁
            CardCmd.RemoveKeyword(clone, YingKeywords.LunHui);
            await CardPileCmd.Add(clone, MainFile.RebirthPile);
        }
    }
}