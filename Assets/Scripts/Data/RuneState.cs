using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{
    public sealed class RuneState
    {
        public const int GradeCount = (int)RuneGrade.Mythic + 1;

        private readonly int[] counts = new int[GradeCount];
        private bool unlocked;

        public RuneState(RuneDefinition definition, RuneGrade grade, IReadOnlyList<int> gradeCounts, bool unlocked)
        {
            Definition = definition;
            Grade = (RuneGrade)Mathf.Clamp((int)grade, 0, (int)RuneDefinition.MaxGrade);

            if (gradeCounts != null)
            {
                for (int i = 0; i < Mathf.Min(gradeCounts.Count, counts.Length); i++)
                {
                    counts[i] = Mathf.Max(0, gradeCounts[i]);
                }
            }

            this.unlocked = unlocked || definition.StartUnlocked || HasAnyCount();
            EnsureMinimumOwnedRune();
            RefreshHighestGrade();
        }

        public RuneDefinition Definition { get; }
        public RuneGrade Grade { get; set; }
        public bool Unlocked
        {
            get => unlocked;
            set
            {
                unlocked = value || Definition.StartUnlocked;
                EnsureMinimumOwnedRune();
                RefreshHighestGrade();
            }
        }

        public int Level => 1 + (int)Grade * 20;
        public bool IsMaxed => IsMaxGrade;
        public int LevelUpCost => PromoteCost;
        public bool IsMaxGrade => Grade >= RuneDefinition.MaxGrade;
        public bool CanPromote => Unlocked && !IsMaxGrade && TryFindSynthesizableGrade(out _);
        public int PromoteCost => Definition.GetPromoteRequirement(Grade);
        public int CurrentGradeCount => GetCount(Grade);
        public string GradeLabel => RuneDefinition.GetGradeLabel(Grade);

        public int GetCount(RuneGrade grade)
        {
            int index = Mathf.Clamp((int)grade, 0, counts.Length - 1);
            return counts[index];
        }

        public void SetCount(RuneGrade grade, int count)
        {
            int index = Mathf.Clamp((int)grade, 0, counts.Length - 1);
            counts[index] = Mathf.Max(0, count);
            unlocked = unlocked || HasAnyCount();
            EnsureMinimumOwnedRune();
            RefreshHighestGrade();
        }

        public void AddCount(RuneGrade grade, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int index = Mathf.Clamp((int)grade, 0, counts.Length - 1);
            counts[index] = Mathf.Clamp(counts[index] + amount, 0, GameData.MaxIntBalanceValue);
            unlocked = true;
            RefreshHighestGrade();
        }

        public bool TryFindSynthesizableGrade(out RuneGrade grade)
        {
            for (int i = (int)RuneDefinition.MaxGrade - 1; i >= 0; i--)
            {
                RuneGrade candidate = (RuneGrade)i;
                int requirement = Definition.GetPromoteRequirement(candidate);
                if (requirement > 0 && counts[i] >= requirement)
                {
                    grade = candidate;
                    return true;
                }
            }

            grade = RuneGrade.Common;
            return false;
        }

        public bool TrySynthesizeOnce(out RuneGrade fromGrade, out RuneGrade toGrade, out bool highestGradeChanged)
        {
            highestGradeChanged = false;
            fromGrade = RuneGrade.Common;
            toGrade = RuneGrade.Common;

            if (!Unlocked || IsMaxGrade || !TryFindSynthesizableGrade(out fromGrade))
            {
                return false;
            }

            int fromIndex = (int)fromGrade;
            int cost = Definition.GetPromoteRequirement(fromGrade);
            if (cost <= 0 || counts[fromIndex] < cost)
            {
                return false;
            }

            RuneGrade oldGrade = Grade;
            toGrade = (RuneGrade)Mathf.Clamp(fromIndex + 1, 0, (int)RuneDefinition.MaxGrade);
            counts[fromIndex] = Mathf.Max(0, counts[fromIndex] - cost);
            AddCount(toGrade, 1);
            RefreshHighestGrade();
            highestGradeChanged = oldGrade != Grade;
            return true;
        }

        public string FormatCurrentSynthesisProgress()
        {
            if (IsMaxGrade)
            {
                return "MAX";
            }

            RuneGrade grade = TryFindSynthesizableGrade(out RuneGrade synthGrade) ? synthGrade : Grade;
            int cost = Definition.GetPromoteRequirement(grade);
            return RuneDefinition.GetGradeLabel(grade) + " " + GetCount(grade) + "/" + cost;
        }

        public string FormatOwnedCounts()
        {
            var parts = new List<string>();
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] <= 0)
                {
                    continue;
                }

                RuneGrade grade = (RuneGrade)i;
                parts.Add(RuneDefinition.GetGradeLabel(grade) + " " + counts[i]);
            }

            return parts.Count > 0 ? string.Join(" / ", parts) : "없음";
        }

        private bool HasAnyCount()
        {
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureMinimumOwnedRune()
        {
            if (!unlocked || HasAnyCount())
            {
                return;
            }

            counts[0] = 1;
        }

        private void RefreshHighestGrade()
        {
            for (int i = counts.Length - 1; i >= 0; i--)
            {
                if (counts[i] > 0)
                {
                    Grade = (RuneGrade)i;
                    return;
                }
            }

            Grade = RuneGrade.Common;
        }
    }
}
