using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Hero.Detail;

namespace IdleGame.UI.Hero.Detail.Equipment
{
    public sealed class HeroDetailEquipmentPresenterArgs
    {
        public HeroDetailViewRefs Refs;
        public EquipmentInventory EquipmentInventory;
        public CurrencyWallet Wallet;
        public BattleManager BattleManager;
        public HeroState SelectedHero;
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
        public string NoticeText;
        public IDictionary<EquipmentSlot, Button> EquipmentFilterButtons;
        public IDictionary<EquipmentSlot, Button> DismantleFilterButtons;
        public IDictionary<string, Button> EquipmentCardButtons;
        public IDictionary<string, Text> EquipmentCardTexts;
        public IDictionary<string, Button> EquipmentActionButtons;
        public IDictionary<string, Button> DismantleCardButtons;
        public IDictionary<string, Text> DismantleCardTexts;
        public Func<string, Transform, int, Color, Button> CreateButton;
        public Func<string, Transform, Color, Button> CreateCornerActionButton;
        public Action<string> OpenEquipmentDetail;
        public Action<string> ToggleEquipment;
        public Action<string> SelectDismantleEquipment;
        public Func<string, HeroState> FindHeroState;
        public Func<HeroDefinition, string> GetShortHeroLabel;
    }

    public sealed class HeroDetailEquipmentPresenterResult
    {
        public bool EquipmentDetailPopupInvalid;
    }

    public static partial class HeroDetailEquipmentPresenter
    {
        public static HeroDetailEquipmentPresenterResult Refresh(HeroDetailEquipmentPresenterArgs args)
        {
            var result = new HeroDetailEquipmentPresenterResult();
            if (args == null)
            {
                return result;
            }

            RefreshEquipmentContent(args);
            RefreshEquipmentDetailPopup(args, result);
            RefreshEquipmentDismantlePopup(args);
            RefreshEquipmentBulkDismantlePrompt(args);
            return result;
        }

        public static void PruneInvalidDismantleSelections(
            ISet<string> selectedDismantleEquipmentIds,
            EquipmentInventory equipmentInventory,
            ICollection<EquipmentSlot> selectedSlots)
        {
            HeroDetailEquipmentListBuilder.PruneInvalidDismantleSelections(
                selectedDismantleEquipmentIds,
                equipmentInventory,
                selectedSlots);
        }

        private static void RefreshEquipmentContent(HeroDetailEquipmentPresenterArgs args)
        {
            HeroDetailView.ApplyEquipmentFilterButtons(args.EquipmentFilterButtons, args.SelectedSlots);
            HeroDetailView.ApplyEquipmentFilterButtons(args.DismantleFilterButtons, args.SelectedSlots);

            if (args.Refs == null || args.Refs.EquipmentContent == null || args.EquipmentInventory == null)
            {
                return;
            }

            if (args.SelectedHero == null || !args.SelectedHero.IsOwned)
            {
                HeroDetailEquipmentCardView.HideCards(args.EquipmentCardButtons);
                ApplyEquipmentContentState(args.Refs, HeroDetailStateBuilder.BuildEquipmentContentLocked());
                return;
            }

            EquipmentState selectedState = args.EquipmentInventory.GetState(args.SelectedEquipmentId);
            HeroDetailEquipmentCardView.HideCards(args.EquipmentCardButtons);
            HeroDetailEquipmentInventoryListState listState = HeroDetailEquipmentListBuilder.BuildInventoryList(
                GameData.Equipments,
                args.EquipmentInventory,
                args.BattleManager != null ? args.BattleManager.Heroes : null,
                args.SelectedSlots);

            foreach (HeroDetailEquipmentInventoryListEntry entry in listState.Entries)
            {
                RefreshInventoryCard(args, entry);
            }

            ApplyEquipmentContentState(args.Refs, HeroDetailStateBuilder.BuildEquipmentContent(new HeroDetailEquipmentContentStateBuildArgs
            {
                SelectedSlots = args.SelectedSlots,
                SlotSelectionActive = args.SlotSelectionActive,
                SelectedSlot = args.SelectedSlot,
                SelectedState = selectedState,
                OwnedCount = listState.OwnedCount,
                VisibleCount = listState.VisibleCount
            }));
        }

    }
}
