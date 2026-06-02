using System.Collections.Generic;
using UnityEngine;

namespace IdleGame.Data
{
    public sealed class HeroState
    {
        private readonly List<HeroTranscendOptionState> transcendOptions = new List<HeroTranscendOptionState>(HeroDefinition.MaxTranscendSlots);

        public HeroState(HeroDefinition definition, int level, int shards, int stars)
        {
            Definition = definition;
            Shards = Mathf.Max(0, shards);
            Stars = Mathf.Clamp(stars, 0, HeroDefinition.MaxStars);
            Level = Mathf.Clamp(level, 1, Definition.GetMaxLevel(Stars));

            for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
            {
                transcendOptions.Add(new HeroTranscendOptionState(string.Empty));
            }
        }

        public HeroDefinition Definition { get; }
        public int Level { get; set; }
        public int Shards { get; set; }
        public int Stars { get; set; }
        public float AttackCooldown { get; set; }
        public IReadOnlyList<HeroTranscendOptionState> TranscendOptions => transcendOptions;

        public int AttackPower => Definition.GetAttackPower(Level, Stars);
        public int MaxHp => Definition.GetMaxHp(Level, Stars);
        public float AttackSpeed => Definition.GetAttackSpeed(Stars);
        public float MoveSpeed => Definition.GetMoveSpeed(Stars);
        public float AttackInterval => Mathf.Max(0.1f, 1f / AttackSpeed);
        public int LevelUpCost => Definition.GetLevelUpCost(Level);
        public int MaxLevel => Definition.GetMaxLevel(Stars);
        public bool IsMaxStars => Stars >= HeroDefinition.MaxStars;
        public int StarUpCost => Definition.GetStarUpCost(Stars);
        public bool CanStarUp => !IsMaxStars && Shards >= StarUpCost;
        public bool IsOwned => Definition.StartUnlocked || Shards > 0 || Stars > 0 || Level > 1;

        public bool IsTranscendSlotUnlocked(int slotIndex)
        {
            return IsOwned
                && slotIndex >= 0
                && slotIndex < HeroDefinition.MaxTranscendSlots
                && Stars >= HeroDefinition.GetTranscendRequiredStars(slotIndex);
        }

        public string GetTranscendOptionId(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= transcendOptions.Count)
            {
                return string.Empty;
            }

            return transcendOptions[slotIndex].OptionId;
        }

        public void SetTranscendOptionId(int slotIndex, string optionId)
        {
            if (slotIndex < 0 || slotIndex >= transcendOptions.Count)
            {
                return;
            }

            transcendOptions[slotIndex].OptionId = optionId ?? string.Empty;
        }
    }
}
