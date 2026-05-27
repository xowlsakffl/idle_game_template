using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;
using IdleGame.UI.Hero;
using IdleGame.UI.Hero.Detail.Equipment;
using IdleGame.UI.Hero.Transcend;

namespace IdleGame.UI.Hero.Detail
{
    public sealed class HeroDetailPanelPresenterArgs
    {
        public HeroDetailViewRefs Refs;
        public EquipmentInventory EquipmentInventory;
        public CurrencyWallet Wallet;
        public BattleManager BattleManager;
        public HeroState Hero;
        public string SelectedHeroId;
        public string SelectedEquipmentId;
        public string SelectedEquipmentDetailId;
        public bool SlotSelectionActive;
        public EquipmentSlot SelectedSlot;
        public ICollection<EquipmentSlot> SelectedSlots;
        public bool EquipmentDetailPopupOpen;
        public bool EquipmentDismantlePopupOpen;
        public bool EquipmentBulkDismantlePromptOpen;
        public HeroRarity SelectedBulkDismantleRarity;
        public ISet<string> SelectedDismantleEquipmentIds;
        public HeroDetailTab ActiveTab;
        public bool InFormation;
        public double CombatPower;
        public int SelectedTranscendSlotIndex;
        public bool HeroTranscendStopOnlySs;
        public bool HeroTranscendAutoRolling;
        public string NoticeText;
        public IDictionary<HeroDetailTab, Button> TabButtons;
        public IDictionary<EquipmentSlot, Button> EquipmentSlotButtons;
        public IDictionary<EquipmentSlot, Text> EquipmentSlotTexts;
        public IDictionary<EquipmentSlot, Button> EquipmentSlotRemoveButtons;
        public IDictionary<EquipmentSlot, Button> EquipmentFilterButtons;
        public IDictionary<EquipmentSlot, Button> DismantleFilterButtons;
        public IDictionary<string, Button> EquipmentCardButtons;
        public IDictionary<string, Text> EquipmentCardTexts;
        public IDictionary<string, Button> EquipmentActionButtons;
        public IDictionary<string, Button> DismantleCardButtons;
        public IDictionary<string, Text> DismantleCardTexts;
        public IList<Button> TranscendSlotButtons;
        public IList<Text> TranscendSlotTexts;
        public IList<Button> TranscendLockButtons;
        public Func<string, Transform, int, Color, Button> CreateButton;
        public Func<string, Transform, Color, Button> CreateCornerActionButton;
        public Action<string> OpenEquipmentDetail;
        public Action<string> ToggleEquipment;
        public Action<string> SelectDismantleEquipment;
        public Func<string, HeroState> FindHeroState;
        public Func<HeroDefinition, string> GetShortHeroLabel;
    }

    public sealed class HeroDetailPanelPresenterResult
    {
        public int SelectedTranscendSlotIndex;
        public bool EquipmentDetailPopupInvalid;
    }

    public static class HeroDetailPanelPresenter
    {
        public static HeroDetailPanelPresenterResult Refresh(HeroDetailPanelPresenterArgs args)
        {
            var result = new HeroDetailPanelPresenterResult
            {
                SelectedTranscendSlotIndex = args != null ? args.SelectedTranscendSlotIndex : 0
            };

            if (args == null || args.Hero == null)
            {
                return result;
            }

            string noticeText = args.NoticeText ?? string.Empty;
            HeroDetailView.ApplyBasicInfo(args.Refs, HeroDetailStateBuilder.BuildBasicInfo(new HeroDetailBasicStateBuildArgs
            {
                Hero = args.Hero,
                ActiveTab = args.ActiveTab,
                Wallet = args.Wallet,
                CombatPower = args.CombatPower,
                OwnedAttackBonusPercent = args.BattleManager != null
                    ? args.BattleManager.GetHeroOwnedAttackBonusPercent(args.Hero)
                    : 0d,
                NoticeText = noticeText
            }));

            ApplyActionButtons(args);
            ApplyEquipmentSlots(args);
            HeroDetailView.ApplyTabState(args.Refs, args.TabButtons, args.ActiveTab);
            ApplyEquipmentArea(args, result, noticeText);
            ApplyTranscendContent(args, result);
            return result;
        }

        private static void ApplyActionButtons(HeroDetailPanelPresenterArgs args)
        {
            HeroDetailView.ApplyActionButtons(args.Refs, HeroDetailStateBuilder.BuildActionButtons(new HeroDetailActionStateBuildArgs
            {
                Hero = args.Hero,
                InFormation = args.InFormation,
                Wallet = args.Wallet
            }));
        }

        private static void ApplyEquipmentSlots(HeroDetailPanelPresenterArgs args)
        {
            if (args.EquipmentSlotTexts == null)
            {
                return;
            }

            EquipmentState selectedState = args.EquipmentInventory != null
                ? args.EquipmentInventory.GetState(args.SelectedEquipmentId)
                : null;
            foreach (KeyValuePair<EquipmentSlot, Text> pair in args.EquipmentSlotTexts)
            {
                EquipmentSlot slot = pair.Key;
                EquipmentState equippedState = args.EquipmentInventory != null
                    ? args.EquipmentInventory.GetEquippedState(args.Hero.Definition.Id, slot)
                    : null;
                HeroDetailView.ApplyEquipmentSlotState(
                    args.EquipmentSlotButtons,
                    args.EquipmentSlotTexts,
                    args.EquipmentSlotRemoveButtons,
                    BuildEquipmentSlotState(slot, selectedState, equippedState, args.SlotSelectionActive, args.SelectedSlot));
            }
        }

        private static HeroDetailEquipmentSlotViewState BuildEquipmentSlotState(
            EquipmentSlot slot,
            EquipmentState selectedState,
            EquipmentState equippedState,
            bool slotSelectionActive,
            EquipmentSlot selectedSlot)
        {
            bool occupied = equippedState != null && equippedState.IsOwned;
            bool selectedEquipmentSlot = selectedState != null && selectedState.Definition.Slot == slot;
            bool selectedTargetSlot = slotSelectionActive && selectedSlot == slot;
            bool selected = selectedEquipmentSlot || selectedTargetSlot;
            string slotLabel = EquipmentUiText.GetSlotLabel(slot);
            string text;
            Color textColor;

            if (occupied)
            {
                text = "Lv." + equippedState.Level
                    + "\n" + slotLabel
                    + "\n" + equippedState.Definition.DisplayName;
                textColor = Color.white;
            }
            else if (selected)
            {
                text = "+\n" + slotLabel + "\n선택중";
                textColor = new Color(1f, 0.91f, 0.40f, 1f);
            }
            else
            {
                text = "+\n" + slotLabel;
                textColor = new Color(0.72f, 0.76f, 0.88f, 1f);
            }

            Color buttonColor = occupied
                ? selectedTargetSlot
                    ? Color.Lerp(HeroUiText.GetRarityColor(equippedState.Definition.Rarity), new Color(1f, 0.91f, 0.40f, 1f), 0.42f)
                    : HeroUiText.GetRarityColor(equippedState.Definition.Rarity)
                : selected ? new Color(0.54f, 0.45f, 0.16f, 1f) : new Color(0.28f, 0.18f, 0.29f, 0.88f);

            return new HeroDetailEquipmentSlotViewState
            {
                Slot = slot,
                Text = text,
                TextColor = textColor,
                ButtonColor = buttonColor,
                RemoveVisible = occupied
            };
        }

        private static void ApplyEquipmentArea(
            HeroDetailPanelPresenterArgs args,
            HeroDetailPanelPresenterResult result,
            string noticeText)
        {
            HeroDetailEquipmentPresenterResult equipmentResult = HeroDetailEquipmentPresenter.Refresh(new HeroDetailEquipmentPresenterArgs
            {
                Refs = args.Refs,
                EquipmentInventory = args.EquipmentInventory,
                Wallet = args.Wallet,
                BattleManager = args.BattleManager,
                SelectedHero = args.Hero,
                SelectedHeroId = args.SelectedHeroId,
                SelectedEquipmentId = args.SelectedEquipmentId,
                SelectedEquipmentDetailId = args.SelectedEquipmentDetailId,
                SlotSelectionActive = args.SlotSelectionActive,
                SelectedSlot = args.SelectedSlot,
                SelectedSlots = args.SelectedSlots,
                EquipmentDetailPopupOpen = args.EquipmentDetailPopupOpen,
                EquipmentDismantlePopupOpen = args.EquipmentDismantlePopupOpen,
                EquipmentBulkDismantlePromptOpen = args.EquipmentBulkDismantlePromptOpen,
                SelectedBulkDismantleRarity = args.SelectedBulkDismantleRarity,
                SelectedDismantleEquipmentIds = args.SelectedDismantleEquipmentIds,
                NoticeText = noticeText,
                EquipmentFilterButtons = args.EquipmentFilterButtons,
                DismantleFilterButtons = args.DismantleFilterButtons,
                EquipmentCardButtons = args.EquipmentCardButtons,
                EquipmentCardTexts = args.EquipmentCardTexts,
                EquipmentActionButtons = args.EquipmentActionButtons,
                DismantleCardButtons = args.DismantleCardButtons,
                DismantleCardTexts = args.DismantleCardTexts,
                CreateButton = args.CreateButton,
                CreateCornerActionButton = args.CreateCornerActionButton,
                OpenEquipmentDetail = args.OpenEquipmentDetail,
                ToggleEquipment = args.ToggleEquipment,
                SelectDismantleEquipment = args.SelectDismantleEquipment,
                FindHeroState = args.FindHeroState,
                GetShortHeroLabel = args.GetShortHeroLabel
            });

            result.EquipmentDetailPopupInvalid = equipmentResult != null && equipmentResult.EquipmentDetailPopupInvalid;
        }

        private static void ApplyTranscendContent(
            HeroDetailPanelPresenterArgs args,
            HeroDetailPanelPresenterResult result)
        {
            if (args.Refs == null || args.Refs.TranscendContent == null)
            {
                return;
            }

            int selectedSlot = Mathf.Clamp(args.SelectedTranscendSlotIndex, 0, HeroDefinition.MaxTranscendSlots - 1);
            result.SelectedTranscendSlotIndex = selectedSlot;
            HeroDetailView.ApplyTranscendContent(
                args.Refs,
                args.TranscendSlotTexts,
                args.TranscendSlotButtons,
                args.TranscendLockButtons,
                HeroDetailTranscendStateBuilder.Build(new HeroDetailTranscendStateBuildArgs
                {
                    Hero = args.Hero,
                    SelectedSlotIndex = selectedSlot,
                    LockedSlots = HeroTranscendActionService.BuildLockedSlots(args.Hero),
                    Wallet = args.Wallet,
                    StopOnlySs = args.HeroTranscendStopOnlySs,
                    AutoRolling = args.HeroTranscendAutoRolling
                }));
        }
    }
}
