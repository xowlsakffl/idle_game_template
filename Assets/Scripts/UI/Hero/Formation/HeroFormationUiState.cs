using System.Collections.Generic;
using System;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.UI.Hero;

namespace IdleGame.UI.Hero.Formation
{
    public sealed class HeroFormationUiState
    {
        private readonly List<string> editingHeroIds = new List<string>();

        public int SelectedPreset { get; private set; } = 1;
        public string SelectedHeroForPlacement { get; private set; } = string.Empty;
        public bool IsDirty { get; private set; }
        public bool SavePromptOpen { get; private set; }
        public IList<string> EditingHeroIds => editingHeroIds;

        public void ResetRuntime()
        {
            SelectedPreset = 1;
            SelectedHeroForPlacement = string.Empty;
            IsDirty = false;
            SavePromptOpen = false;
            editingHeroIds.Clear();
        }

        public void LoadDraft(BattleManager battleManager, int preset)
        {
            SelectedPreset = ClampPreset(preset);
            SelectedHeroForPlacement = string.Empty;
            editingHeroIds.Clear();

            IReadOnlyList<string> savedIds = battleManager != null
                ? battleManager.GetHeroFormationHeroIds(SelectedPreset)
                : null;
            editingHeroIds.AddRange(HeroFormationDraftRules.CreateDraft(savedIds, GameData.MaxPartyHeroes));
            IsDirty = false;
        }

        public void EnsureDraft(BattleManager battleManager)
        {
            if (editingHeroIds.Count == GameData.MaxPartyHeroes)
            {
                return;
            }

            LoadDraft(battleManager, battleManager != null ? battleManager.ActiveHeroPreset : 1);
        }

        public bool HasPendingChanges(BattleManager battleManager)
        {
            EnsureDraft(battleManager);

            IReadOnlyList<string> savedIds = battleManager != null
                ? battleManager.GetHeroFormationHeroIds(SelectedPreset)
                : null;
            int activePreset = battleManager != null ? battleManager.ActiveHeroPreset : SelectedPreset;
            return HeroFormationDraftRules.HasPendingChanges(
                editingHeroIds,
                savedIds,
                IsDirty,
                SelectedPreset,
                activePreset,
                GameData.MaxPartyHeroes);
        }

        public void OpenSavePrompt()
        {
            SavePromptOpen = true;
        }

        public void CloseSavePrompt()
        {
            SavePromptOpen = false;
        }

        public bool TryApply(BattleManager battleManager)
        {
            return battleManager != null && battleManager.ApplyHeroFormation(SelectedPreset, editingHeroIds);
        }

        public void MarkApplied()
        {
            IsDirty = false;
            SelectedHeroForPlacement = string.Empty;
            SavePromptOpen = false;
        }

        public bool IsSelectedPreset(int preset)
        {
            return SelectedPreset == ClampPreset(preset);
        }

        public bool ContainsHero(string heroId)
        {
            EnsureDraft(null);
            return HeroFormationDraftRules.Contains(editingHeroIds, heroId);
        }

        public int IndexOfHero(string heroId)
        {
            EnsureDraft(null);
            return HeroFormationDraftRules.IndexOf(editingHeroIds, heroId);
        }

        public int CountFilled()
        {
            EnsureDraft(null);
            return HeroFormationDraftRules.CountFilled(editingHeroIds);
        }

        public void AutoArrange(BattleManager battleManager, Func<HeroState, double> getCombatPower)
        {
            EnsureDraft(battleManager);
            if (battleManager == null || battleManager.Heroes.Count <= 0)
            {
                return;
            }

            editingHeroIds.Clear();
            editingHeroIds.AddRange(HeroFormationDraftRules.BuildAutoDraft(
                battleManager.Heroes,
                getCombatPower,
                GameData.MaxPartyHeroes));

            SelectedHeroForPlacement = string.Empty;
            IsDirty = HasPendingChanges(battleManager);
        }

        public void ApplyActionResult(HeroActionResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.ClearSelectedHeroForPlacement)
            {
                SelectedHeroForPlacement = string.Empty;
            }

            if (result.HasSelectedHeroForPlacement)
            {
                SelectedHeroForPlacement = result.SelectedHeroForPlacement ?? string.Empty;
            }

            if (result.FormationDirty)
            {
                IsDirty = true;
            }
        }

        private static int ClampPreset(int preset)
        {
            if (preset < 1)
            {
                return 1;
            }

            return preset > GameData.MaxHeroPresets ? GameData.MaxHeroPresets : preset;
        }
    }
}
