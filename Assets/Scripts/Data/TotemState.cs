using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{
    public sealed class TotemState
    {
        public TotemState(TotemDefinition definition, int level, TotemGrade grade, bool unlocked)
        {
            Definition = definition;
            Level = Mathf.Clamp(level, 1, TotemDefinition.MaxLevel);
            Grade = grade;
            Unlocked = unlocked || definition.StartUnlocked;
        }

        public TotemDefinition Definition { get; }
        public int Level { get; set; }
        public TotemGrade Grade { get; set; }
        public bool Unlocked { get; set; }
        public bool IsMaxed => Level >= TotemDefinition.MaxLevel;
        public bool CanPromote => IsMaxed && Grade < TotemGrade.Mythic;
        public string DisplayName => Definition.GetDisplayName(Grade);
        public string GradeLabel => TotemDefinition.GetGradeLabel(Grade);
        public int LevelUpCost => Definition.GetLevelUpCost(Level, Grade);
        public int PromoteCost => Definition.GetPromoteCost(Grade);
    }

}
