using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying;
using Yingmod.Ying.Keywords;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class LaiShengPower : IllusionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int CopyCount { get; set; }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        var rebirthPile = MainFile.RebirthPile.GetPile(player);
        if (rebirthPile?.Cards == null) return;

        // 使用官方联机安全的随机数生成器，选择两张牌附加轮回词条
        var rng = player.RunState.Rng.CombatCardGeneration;
        var candidates = rebirthPile.Cards
            .Where(c => c.Type == CardType.Attack || c.Type == CardType.Skill)
            .ToList();

        // 随机选取至多两张牌附加轮回词条（不足两张时全选）
        for (int i = 0; i < 2 && candidates.Count > 0; i++)
        {
            int index = rng.NextInt(0, candidates.Count);
            var card = candidates[index];
            if (!card.Keywords.Contains(YingKeywords.LunHui))
                CardCmd.ApplyKeyword(card, YingKeywords.LunHui);
            candidates.RemoveAt(index);
        }

        // 从轮回堆选择 1 张牌，复制指定数量（min=1, max=1）
        if (rebirthPile.Cards.Count > 0)
        {
            var prefs = new CardSelectorPrefs(new LocString("cards", "YING_CARD_LAI_SHENG.select"), 1, 1);
            var selected = await CardSelectCmd.FromCombatPile(choiceContext, rebirthPile, player, prefs);
            if (selected.Any())
            {
                var chosen = selected.First();
                for (int i = 0; i < CopyCount; i++)
                {
                    var clone = chosen.CreateClone();
                    await CardPileCmd.Add(clone, rebirthPile);
                }
            }
        }
    }
}