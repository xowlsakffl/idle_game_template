using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;

namespace IdleGame.UI.Hero.TotemRune
{
    public static class TotemRuneUiText
    {
        public static string BuildTotemSummary(long essence, Func<long, string> formatCount)
        {
            return "토템 6종 효과 전체 적용  정수 " + FormatCount(essence, formatCount)
                + "\n같은 등급 토템 6개를 모두 Lv." + TotemDefinition.MaxLevel + "까지 강화하면 전체 진화";
        }

        public static string BuildTotemCardText(TotemDefinition totem, TotemState state, bool unlocked)
        {
            if (totem == null)
            {
                return string.Empty;
            }

            return totem.Icon
                + "\n" + GetTotemCategoryLabel(totem.Archetype)
                + "\n" + (state != null ? state.GradeLabel : TotemDefinition.GetGradeLabel(TotemGrade.Common))
                + " Lv." + (state != null ? state.Level : 1)
                + "\n전체 적용"
                + (unlocked ? string.Empty : "\n미보유");
        }

        public static string BuildTotemDetailText(TotemState state, IReadOnlyList<HeroState> deployedHeroes, bool isBoss)
        {
            if (state == null)
            {
                return string.Empty;
            }

            return state.Definition.Icon + " " + state.DisplayName
                + "  " + state.GradeLabel + " Lv." + state.Level + "/" + TotemDefinition.MaxLevel
                + "\n효과: " + state.Definition.Role
                + "\n" + state.Definition.GetEffectSummary(state.Level, state.Grade, deployedHeroes, isBoss)
                + "\n\n장착 없이 항상 적용";
        }

        public static string BuildTotemLevelUpButtonText(TotemState state, bool tierReady, Func<long, string> formatCount)
        {
            if (state == null)
            {
                return "강화";
            }

            if (state.CanPromote)
            {
                return tierReady ? "전체 진화\n" + FormatCount(state.PromoteCost, formatCount) : "전체 MAX 필요";
            }

            return state.IsMaxed ? "MAX" : "강화\n" + FormatCount(state.LevelUpCost, formatCount);
        }

        public static string BuildRuneSummary(int preset, RuneState selectedState)
        {
            string synthesisText = selectedState != null && !selectedState.IsMaxed
                ? "  합성 " + selectedState.FormatCurrentSynthesisProgress()
                : string.Empty;
            return "프리셋 " + preset + synthesisText;
        }

        public static string BuildRuneCardText(RuneDefinition rune, RuneState state, bool equipped, bool unlocked)
        {
            if (rune == null)
            {
                return string.Empty;
            }

            return (equipped ? "장착중\n" : string.Empty)
                + rune.Icon
                + "\n" + rune.DisplayName
                + "\n" + (state != null ? state.GradeLabel : RuneDefinition.GetGradeLabel(RuneGrade.Common))
                + (state != null && !state.IsMaxed ? "\n합성 " + state.FormatCurrentSynthesisProgress() : string.Empty)
                + (unlocked ? string.Empty : "\n미보유");
        }

        public static string BuildRuneDetailText(RuneState state)
        {
            if (state == null)
            {
                return string.Empty;
            }

            return state.Definition.Icon + " " + state.Definition.DisplayName
                + "  " + state.GradeLabel
                + (state.IsMaxed ? " MAX" : "  합성 " + state.FormatCurrentSynthesisProgress())
                + "\n" + state.Definition.Role
                + "\n보유: " + state.FormatOwnedCounts()
                + "\n" + state.Definition.GetEffectSummary(state.Grade);
        }

        public static string BuildRuneEquipButtonText(bool equipped)
        {
            return equipped ? "장착 해제" : "장착";
        }

        public static string BuildRuneLevelUpButtonText(RuneState state)
        {
            if (state == null)
            {
                return "합성";
            }

            return state.IsMaxed ? "MAX" : "합성\n" + state.FormatCurrentSynthesisProgress();
        }

        public static Color GetTotemColor(TotemDefinition totem)
        {
            if (totem == null)
            {
                return new Color(0.20f, 0.27f, 0.38f, 1f);
            }

            switch (totem.Archetype)
            {
                case TotemArchetype.Combat:
                    return new Color(0.55f, 0.20f, 0.20f, 1f);
                case TotemArchetype.Support:
                    return new Color(0.42f, 0.36f, 0.14f, 1f);
                case TotemArchetype.Guardian:
                    return new Color(0.22f, 0.42f, 0.54f, 1f);
                case TotemArchetype.Storm:
                    return new Color(0.18f, 0.48f, 0.44f, 1f);
                case TotemArchetype.Arcane:
                    return new Color(0.42f, 0.22f, 0.58f, 1f);
                case TotemArchetype.Command:
                    return new Color(0.38f, 0.32f, 0.52f, 1f);
                default:
                    return new Color(0.20f, 0.27f, 0.38f, 1f);
            }
        }

        public static Color GetRuneColor(RuneDefinition rune)
        {
            if (rune == null)
            {
                return new Color(0.20f, 0.27f, 0.38f, 1f);
            }

            switch (rune.EffectKind)
            {
                case RuneEffectKind.Strike:
                    return new Color(0.46f, 0.24f, 0.22f, 1f);
                case RuneEffectKind.Execute:
                    return new Color(0.42f, 0.18f, 0.30f, 1f);
                case RuneEffectKind.Barrier:
                    return new Color(0.20f, 0.34f, 0.48f, 1f);
                case RuneEffectKind.Harvest:
                    return new Color(0.42f, 0.36f, 0.16f, 1f);
                case RuneEffectKind.Arcane:
                    return new Color(0.36f, 0.22f, 0.56f, 1f);
                case RuneEffectKind.Storm:
                    return new Color(0.18f, 0.45f, 0.43f, 1f);
                case RuneEffectKind.Focus:
                    return new Color(0.30f, 0.40f, 0.60f, 1f);
                case RuneEffectKind.Vitality:
                    return new Color(0.24f, 0.42f, 0.30f, 1f);
                case RuneEffectKind.Command:
                    return new Color(0.38f, 0.32f, 0.52f, 1f);
                case RuneEffectKind.Regeneration:
                    return new Color(0.25f, 0.38f, 0.34f, 1f);
                default:
                    return new Color(0.20f, 0.27f, 0.38f, 1f);
            }
        }

        public static string GetTotemCategoryLabel(TotemArchetype archetype)
        {
            switch (archetype)
            {
                case TotemArchetype.Combat:
                    return "전투\n토템";
                case TotemArchetype.Guardian:
                    return "수호\n토템";
                case TotemArchetype.Support:
                    return "지원\n토템";
                case TotemArchetype.Arcane:
                    return "비전\n토템";
                case TotemArchetype.Storm:
                    return "폭풍\n토템";
                case TotemArchetype.Command:
                    return "지휘\n토템";
                default:
                    return "토템";
            }
        }

        private static string FormatCount(long amount, Func<long, string> formatCount)
        {
            return formatCount != null ? formatCount(amount) : amount.ToString();
        }
    }
}
