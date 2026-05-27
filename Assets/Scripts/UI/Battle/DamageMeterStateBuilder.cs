using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Hero;

namespace IdleGame.UI.Battle
{
    public sealed class DamageMeterRowViewState
    {
        public bool Active;
        public string Text;
        public float FillRatio;
        public Color FillColor;
    }

    public static class DamageMeterStateBuilder
    {
        private static readonly Color DamageHighlightColor = new Color(1f, 0.78f, 0.18f, 1f);

        public static List<DamageMeterRowViewState> BuildRows(
            IReadOnlyList<HeroState> deployedHeroes,
            Func<string, GameNumber> getHeroDamage,
            GameNumber maxDamage,
            int rowCount,
            Func<HeroDefinition, string> getShortHeroLabel,
            Func<GameNumber, string> formatShort,
            List<HeroState> heroScratch,
            List<DamageMeterRowViewState> rowStates)
        {
            if (rowStates == null)
            {
                rowStates = new List<DamageMeterRowViewState>(rowCount);
            }

            EnsureRowCount(rowStates, rowCount);
            List<HeroState> heroes = heroScratch ?? new List<HeroState>(rowCount);
            heroes.Clear();

            if (deployedHeroes != null)
            {
                for (int i = 0; i < deployedHeroes.Count; i++)
                {
                    HeroState hero = deployedHeroes[i];
                    if (hero != null && hero.Definition != null)
                    {
                        heroes.Add(hero);
                    }
                }
            }

            heroes.Sort((left, right) => CompareHeroesForDamageMeter(left, right, getHeroDamage));
            GameNumber safeMaxDamage = GameNumber.Max(GameNumber.One, maxDamage);

            for (int i = 0; i < rowStates.Count; i++)
            {
                DamageMeterRowViewState state = rowStates[i];
                bool active = i < heroes.Count;
                state.Active = active;

                if (!active)
                {
                    state.Text = string.Empty;
                    state.FillRatio = 0f;
                    state.FillColor = Color.clear;
                    continue;
                }

                HeroState hero = heroes[i];
                GameNumber damage = GetHeroDamage(hero.Definition.Id, getHeroDamage);
                state.Text = FormatHeroLabel(hero.Definition, getShortHeroLabel)
                    + "  " + FormatDamage(damage, formatShort);
                state.FillRatio = Mathf.Clamp01((float)damage.RatioTo(safeMaxDamage));
                state.FillColor = Color.Lerp(HeroUiText.GetRarityColor(hero.Definition.Rarity), DamageHighlightColor, 0.28f);
            }

            return rowStates;
        }

        private static void EnsureRowCount(List<DamageMeterRowViewState> rowStates, int rowCount)
        {
            int safeRowCount = Mathf.Max(0, rowCount);
            while (rowStates.Count < safeRowCount)
            {
                rowStates.Add(new DamageMeterRowViewState());
            }

            if (rowStates.Count > safeRowCount)
            {
                rowStates.RemoveRange(safeRowCount, rowStates.Count - safeRowCount);
            }
        }

        private static int CompareHeroesForDamageMeter(
            HeroState left,
            HeroState right,
            Func<string, GameNumber> getHeroDamage)
        {
            GameNumber rightDamage = right != null && right.Definition != null
                ? GetHeroDamage(right.Definition.Id, getHeroDamage)
                : GameNumber.Zero;
            GameNumber leftDamage = left != null && left.Definition != null
                ? GetHeroDamage(left.Definition.Id, getHeroDamage)
                : GameNumber.Zero;

            int damageCompare = rightDamage.CompareTo(leftDamage);
            if (damageCompare != 0)
            {
                return damageCompare;
            }

            string leftId = left != null && left.Definition != null ? left.Definition.Id : string.Empty;
            string rightId = right != null && right.Definition != null ? right.Definition.Id : string.Empty;
            return string.CompareOrdinal(leftId, rightId);
        }

        private static GameNumber GetHeroDamage(string heroId, Func<string, GameNumber> getHeroDamage)
        {
            return getHeroDamage != null ? getHeroDamage(heroId) : GameNumber.Zero;
        }

        private static string FormatHeroLabel(HeroDefinition hero, Func<HeroDefinition, string> getShortHeroLabel)
        {
            return getShortHeroLabel != null ? getShortHeroLabel(hero) : hero.DisplayName;
        }

        private static string FormatDamage(GameNumber damage, Func<GameNumber, string> formatShort)
        {
            return formatShort != null ? formatShort(damage) : damage.ToString();
        }
    }
}
