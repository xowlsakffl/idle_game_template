using System.Collections.Generic;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.UI.Hero.Transcend
{
    public sealed class HeroTranscendRollResult
    {
        public bool Success;
        public int ChangedSlots;
        public int Cost;
        public HeroTranscendOptionDefinition BestOption;
        public string Message;
    }

    public static class HeroTranscendRollService
    {
        public static bool ShouldAskBeforeRoll(HeroState hero, bool[] lockedSlots)
        {
            return HeroTranscendRules.HasSsInChangeableSlots(hero, lockedSlots);
        }

        public static HeroTranscendRollResult TryRollBatch(
            HeroState hero,
            bool[] lockedSlots,
            CurrencyWallet wallet,
            BattleManager battleManager)
        {
            var result = new HeroTranscendRollResult();
            if (hero == null || !hero.IsOwned || battleManager == null)
            {
                return result;
            }

            List<int> targetSlots = HeroTranscendRules.GetChangeableSlots(hero, lockedSlots);
            if (targetSlots.Count <= 0)
            {
                result.Message = "변경할 초월칸이 없습니다.";
                return result;
            }

            int cost = HeroTranscendRules.GetRollCost(hero, lockedSlots);
            result.Cost = cost;
            if (wallet == null || !wallet.SpendHeroTranscendStone(cost))
            {
                result.Message = "초월석이 부족합니다.";
                return result;
            }

            foreach (int slotIndex in targetSlots)
            {
                if (!battleManager.TryRollHeroTranscendOption(hero.Definition.Id, slotIndex, false, out HeroTranscendOptionDefinition option))
                {
                    continue;
                }

                result.ChangedSlots += 1;
                if (HeroTranscendRules.IsBetterOption(option, result.BestOption))
                {
                    result.BestOption = option;
                }
            }

            if (result.ChangedSlots <= 0)
            {
                wallet.AddHeroTranscendStone(cost);
                result.Message = "초월 변경에 실패했습니다.";
                return result;
            }

            result.Success = true;
            result.Message = "초월 변경 " + result.ChangedSlots + "칸 "
                + (result.BestOption != null ? result.BestOption.Grade + " " + result.BestOption.Description : "완료");
            return result;
        }

        public static string BuildAutoRollFinishedMessage(int rolls, HeroTranscendOptionDefinition lastOption)
        {
            return rolls > 0 && lastOption != null
                ? "자동 변경 " + rolls + "회 " + lastOption.Grade + " " + lastOption.Description
                : "자동 변경할 수 없습니다.";
        }
    }
}
