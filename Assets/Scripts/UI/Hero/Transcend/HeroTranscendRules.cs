using System.Collections.Generic;
using IdleGame.Data;

namespace IdleGame.UI.Hero.Transcend
{
    public static class HeroTranscendRules
    {
        public const int BaseRollCost = 10;
        public const int LockedSlotExtraCost = 10;

        public static int GetRollCost(HeroState hero, bool[] lockedSlots)
        {
            return BaseRollCost + CountLockedSlots(hero, lockedSlots) * LockedSlotExtraCost;
        }

        public static List<int> GetChangeableSlots(HeroState hero, bool[] lockedSlots)
        {
            var slots = new List<int>();
            if (hero == null)
            {
                return slots;
            }

            for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
            {
                if (hero.IsTranscendSlotUnlocked(i) && !IsLocked(lockedSlots, i))
                {
                    slots.Add(i);
                }
            }

            return slots;
        }

        public static int CountUnlockedSlots(HeroState hero)
        {
            if (hero == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
            {
                if (hero.IsTranscendSlotUnlocked(i))
                {
                    count += 1;
                }
            }

            return count;
        }

        public static int CountLockedSlots(HeroState hero, bool[] lockedSlots)
        {
            if (hero == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
            {
                if (hero.IsTranscendSlotUnlocked(i) && IsLocked(lockedSlots, i))
                {
                    count += 1;
                }
            }

            return count;
        }

        public static int CountChangeableSlots(HeroState hero, bool[] lockedSlots)
        {
            if (hero == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
            {
                if (hero.IsTranscendSlotUnlocked(i) && !IsLocked(lockedSlots, i))
                {
                    count += 1;
                }
            }

            return count;
        }

        public static bool HasSsInChangeableSlots(HeroState hero, bool[] lockedSlots)
        {
            List<int> targetSlots = GetChangeableSlots(hero, lockedSlots);
            foreach (int slotIndex in targetSlots)
            {
                HeroTranscendOptionDefinition option = GameData.GetHeroTranscendOption(hero.GetTranscendOptionId(slotIndex));
                if (option != null && option.Grade >= HeroTranscendGrade.SS)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsBetterOption(HeroTranscendOptionDefinition candidate, HeroTranscendOptionDefinition current)
        {
            if (candidate == null)
            {
                return false;
            }

            return current == null || candidate.Grade > current.Grade;
        }

        public static bool ShouldStopAuto(HeroTranscendOptionDefinition option, bool stopOnlySs)
        {
            if (option == null)
            {
                return false;
            }

            return stopOnlySs
                ? option.Grade >= HeroTranscendGrade.SS
                : option.Grade >= HeroTranscendGrade.S;
        }

        public static bool IsLocked(bool[] lockedSlots, int slotIndex)
        {
            return lockedSlots != null
                && slotIndex >= 0
                && slotIndex < lockedSlots.Length
                && lockedSlots[slotIndex];
        }
    }
}
