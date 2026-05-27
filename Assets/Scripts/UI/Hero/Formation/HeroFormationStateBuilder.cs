using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;
using IdleGame.UI.Hero;
using IdleGame.UI.Hero.TotemRune;

namespace IdleGame.UI.Hero.Formation
{
    public static class HeroFormationStateBuilder
    {
        private static readonly Color EmptySlotTextColor = new Color(0.64f, 0.70f, 0.78f, 1f);
        private static readonly Color EmptySlotColor = HudButtonStyle.Slot.Color;
        private static readonly Color DeployedRosterColor = new Color(0.13f, 0.15f, 0.18f, 1f);
        private static readonly Color SelectedRosterColor = new Color(0.55f, 0.49f, 0.20f, 1f);
        private static readonly Color LockedActionColor = HudButtonStyle.Disabled.Color;
        private static readonly Color RemoveActionColor = HudButtonStyle.Danger.Color;
        private static readonly Color AddActionColor = HudButtonStyle.ActionAdd.Color;
        private static readonly Color LockedRuneSlotColor = HudButtonStyle.LockedSlot.Color;
        private static readonly Color EmptyRuneSlotColor = HudButtonStyle.RuneSlot.Color;

        public static string BuildSummary(
            int deployedCount,
            int maxPartyHeroes,
            int selectedPreset,
            bool hasPendingChanges,
            string selectedHeroName)
        {
            return "출전 " + deployedCount + "/" + maxPartyHeroes
                + "    프리셋 " + selectedPreset
                + (hasPendingChanges ? "    저장 필요" : string.Empty)
                + (string.IsNullOrEmpty(selectedHeroName) ? string.Empty : "    배치 선택: " + selectedHeroName);
        }

        public static HeroFormationSlotState BuildSlotState(
            HeroState hero,
            bool hasSelectedHeroForPlacement,
            Func<double, string> formatShortNumber,
            Func<HeroDefinition, string> getShortHeroLabel)
        {
            bool occupied = hero != null;
            return new HeroFormationSlotState
            {
                Interactable = occupied || hasSelectedHeroForPlacement,
                RemoveVisible = occupied,
                Text = occupied
                    ? HeroUiText.GetTraitBadge(hero.Definition.Trait)
                        + " " + HeroUiText.GetRarityBadge(hero.Definition.Rarity)
                        + " Lv." + hero.Level
                        + "\n" + FormatHeroLabel(hero.Definition, getShortHeroLabel) + "  " + StarUiText.FormatStars(hero.Stars)
                        + "\n공 " + FormatNumber(hero.AttackPower, formatShortNumber) + " 체 " + FormatNumber(hero.MaxHp, formatShortNumber)
                    : "빈슬롯",
                TextColor = occupied ? Color.white : EmptySlotTextColor,
                ButtonColor = occupied ? HeroUiText.GetRarityColor(hero.Definition.Rarity) : EmptySlotColor
            };
        }

        public static HeroRosterCardState BuildRosterCardState(
            HeroState hero,
            bool isDeployed,
            bool isSelectedForPlacement,
            bool needsAttention,
            Func<double, string> formatShortNumber,
            Func<HeroDefinition, string> getShortHeroLabel)
        {
            if (hero == null)
            {
                return null;
            }

            bool isOwned = hero.IsOwned;
            string actionText = isSelectedForPlacement ? "선택중" : "배치";
            string starCostText = hero.IsMaxStars ? "S MAX" : "S " + hero.Shards + "/" + hero.StarUpCost;
            string deployActionSuffix = isDeployed || !isOwned ? string.Empty : "  " + actionText;

            Color buttonColor = isDeployed
                ? DeployedRosterColor
                : isSelectedForPlacement ? SelectedRosterColor : HeroUiText.GetRarityColor(hero.Definition.Rarity);

            Color actionColor = !isOwned
                ? LockedActionColor
                : isDeployed ? RemoveActionColor : AddActionColor;

            return new HeroRosterCardState
            {
                IsOwned = isOwned,
                IsDeployed = isDeployed,
                NeedsAttention = needsAttention,
                ActionInteractable = isOwned,
                DisplayText = HeroUiText.GetTraitBadge(hero.Definition.Trait)
                    + " " + HeroUiText.GetRarityBadge(hero.Definition.Rarity)
                    + (isOwned ? " Lv." + hero.Level : " 미보유")
                    + "\n" + FormatHeroLabel(hero.Definition, getShortHeroLabel)
                    + "  " + hero.Definition.RarityLabel + "  " + StarUiText.FormatStars(hero.Stars)
                    + "\n공 " + FormatNumber(hero.AttackPower, formatShortNumber) + " 체 " + FormatNumber(hero.MaxHp, formatShortNumber)
                    + "\n" + starCostText + deployActionSuffix,
                ActionText = !isOwned ? string.Empty : isDeployed ? "-" : "+",
                ButtonColor = buttonColor,
                ActionColor = actionColor
            };
        }

        public static HeroFormationRuneSlotState BuildRuneSlotState(
            int slot,
            bool unlocked,
            int unlockLevel,
            RuneState equippedState,
            RuneState pendingState)
        {
            if (!unlocked)
            {
                return new HeroFormationRuneSlotState
                {
                    Interactable = false,
                    RemoveVisible = false,
                    Text = "잠김\nLv." + unlockLevel,
                    ButtonColor = LockedRuneSlotColor
                };
            }

            if (equippedState == null)
            {
                return new HeroFormationRuneSlotState
                {
                    Interactable = pendingState != null,
                    RemoveVisible = false,
                    Text = pendingState != null
                        ? slot + "\n" + pendingState.Definition.DisplayName + "\n장착"
                        : slot + "\n빈슬롯",
                    ButtonColor = pendingState != null
                        ? Color.Lerp(TotemRuneUiText.GetRuneColor(pendingState.Definition), Color.white, 0.22f)
                        : EmptyRuneSlotColor
                };
            }

            return new HeroFormationRuneSlotState
            {
                Interactable = pendingState != null,
                RemoveVisible = true,
                Text = slot
                    + "\n" + equippedState.Definition.Icon + " " + equippedState.Definition.DisplayName
                    + "\n" + equippedState.GradeLabel
                    + (pendingState != null && equippedState.Definition.Id != pendingState.Definition.Id ? "\n교체 가능" : string.Empty),
                ButtonColor = TotemRuneUiText.GetRuneColor(equippedState.Definition)
            };
        }

        private static string FormatHeroLabel(HeroDefinition definition, Func<HeroDefinition, string> getShortHeroLabel)
        {
            if (definition == null)
            {
                return string.Empty;
            }

            return getShortHeroLabel != null ? getShortHeroLabel(definition) : definition.DisplayName;
        }

        private static string FormatNumber(double value, Func<double, string> formatShortNumber)
        {
            return formatShortNumber != null ? formatShortNumber(value) : value.ToString("0");
        }
    }
}
