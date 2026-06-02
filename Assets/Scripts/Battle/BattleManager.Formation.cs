using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;
using IdleGame.Save;
using IdleGame.Speed;

namespace IdleGame.Battle
{
    public sealed partial class BattleManager
    {
        public void SetActiveHeroPreset(int preset)
        {
            activeHeroPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
            PlayerPrefs.SetInt(SaveKeys.HeroFormationPreset, activeHeroPreset);
            saveManager.Flush();
            RefreshDeployedHeroes();
            NotifyChanged(BattleChangeFlags.Formation);
        }

        public IReadOnlyList<string> GetHeroFormationHeroIds(int preset)
        {
            return NormalizeFormationHeroIds(LoadFormationHeroIds(Mathf.Clamp(preset, 1, GameData.MaxHeroPresets)));
        }

        public bool ApplyHeroFormation(int preset, IReadOnlyList<string> heroIds)
        {
            if (!IsReady())
            {
                return false;
            }

            List<string> normalizedIds = NormalizeFormationHeroIds(heroIds);
            if (GetFilledFormationCount(normalizedIds) <= 0)
            {
                LastBattleLog = "편성 실패: 최소 1명이 필요";
                NotifyChanged(BattleChangeFlags.BattleLog);
                return false;
            }

            SaveHeroFormationAndRestart(preset, normalizedIds, null);
            return true;
        }

        public bool ApplyHeroFormationLoadout(
            int preset,
            IReadOnlyList<string> heroIds,
            IReadOnlyList<string> runeIds)
        {
            if (!IsReady())
            {
                return false;
            }

            List<string> normalizedHeroIds = NormalizeFormationHeroIds(heroIds);
            if (GetFilledFormationCount(normalizedHeroIds) <= 0)
            {
                LastBattleLog = "편성 실패: 최소 1명이 필요";
                NotifyChanged(BattleChangeFlags.BattleLog);
                return false;
            }

            SaveHeroFormationAndRestart(
                preset,
                normalizedHeroIds,
                () =>
                {
                    FormationLoadoutService.SaveRuneSlots(
                        saveManager,
                        runesById,
                        GetCurrentAccountLevel(),
                        activeHeroPreset,
                        runeIds);
                });
            return true;
        }

        public bool ToggleHeroInActiveFormation(string heroId)
        {
            bool changed = HeroFormationService.TryToggleHeroInFormation(
                heroes,
                LoadFormationHeroIds(activeHeroPreset),
                heroId,
                out List<string> ids,
                out string battleLog);

            return ApplyActiveFormationChange(changed, ids, battleLog, "프리셋 " + activeHeroPreset + " 편성 갱신");
        }

        public bool SetHeroInActiveFormationSlot(string heroId, int slotIndex)
        {
            if (!HeroFormationService.TrySetHeroInFormationSlot(
                    heroes,
                    LoadFormationHeroIds(activeHeroPreset),
                    heroId,
                    slotIndex,
                    out List<string> ids,
                    out HeroState hero))
            {
                return false;
            }

            return ApplyActiveFormationChange(
                true,
                ids,
                string.Empty,
                hero.Definition.DisplayName + " 슬롯 " + (slotIndex + 1) + " 배치");
        }

        public bool RemoveHeroFromActiveFormationSlot(int slotIndex)
        {
            bool changed = HeroFormationService.TryRemoveHeroFromFormationSlot(
                heroes,
                LoadFormationHeroIds(activeHeroPreset),
                slotIndex,
                out List<string> ids,
                out string battleLog);

            return ApplyActiveFormationChange(changed, ids, battleLog, "슬롯 " + (slotIndex + 1) + " 편성 해제");
        }

        public bool RemoveHeroFromActiveFormation(string heroId)
        {
            List<string> ids = NormalizeFormationHeroIds(LoadFormationHeroIds(activeHeroPreset));
            int index = ids.IndexOf(heroId);
            return index >= 0 && RemoveHeroFromActiveFormationSlot(index);
        }

        private bool ApplyFormationLoadoutChange(bool changed, FormationLoadoutChangeResult result)
        {
            return ApplyLoggedChange(
                changed,
                result.BattleLog,
                () =>
                {
                    if (result.Preset == activeHeroPreset)
                    {
                        StartStage(false);
                        NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.Formation | BattleChangeFlags.TotemRune);
                    }
                    else
                    {
                        NotifyChanged(BattleChangeFlags.Formation | BattleChangeFlags.TotemRune);
                    }
                });
        }

        private bool ApplyActiveFormationChange(
            bool changed,
            List<string> ids,
            string failureBattleLog,
            string successBattleLog)
        {
            return ApplyLoggedChange(
                changed,
                failureBattleLog,
                () =>
                {
                    SaveFormationHeroIds(activeHeroPreset, ids);
                    RefreshDeployedHeroes();
                    ApplyBattleLog(successBattleLog);
                    NotifyChanged(BattleChangeFlags.Formation);
                });
        }

        private void SaveHeroFormationAndRestart(int preset, List<string> normalizedIds, Action saveLoadout)
        {
            activeHeroPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
            PlayerPrefs.SetInt(SaveKeys.HeroFormationPreset, activeHeroPreset);
            SaveFormationHeroIds(activeHeroPreset, normalizedIds);
            saveLoadout?.Invoke();
            saveManager.Flush();
            RefreshDeployedHeroes();
            LastBattleLog = "프리셋 " + activeHeroPreset + " 편성 저장";
            StartStage(false);
            NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.Formation | BattleChangeFlags.TotemRune);
        }

        private void RefreshDeployedHeroes()
        {
            HeroFormationService.RefreshDeployedHeroes(heroes, activeHeroPreset, deployedHeroes, activeFormationHeroIds);
        }

        private List<string> LoadFormationHeroIds(int preset)
        {
            return HeroFormationService.LoadFormationHeroIds(preset, heroes);
        }

        private void SaveFormationHeroIds(int preset, List<string> ids)
        {
            HeroFormationService.SaveFormationHeroIds(preset, ids, saveManager);
        }

        private List<string> NormalizeFormationHeroIds(IReadOnlyList<string> sourceIds)
        {
            return HeroFormationService.NormalizeFormationHeroIds(sourceIds, heroes);
        }

        private static int GetFilledFormationCount(List<string> ids)
        {
            return HeroFormationService.GetFilledFormationCount(ids);
        }
    }
}
