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

    public static class HeroDetailEquipmentPresenter
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

        private static void RefreshInventoryCard(HeroDetailEquipmentPresenterArgs args, HeroDetailEquipmentInventoryListEntry entry)
        {
            if (entry == null || args.Refs == null)
            {
                return;
            }

            Button cardButton = HeroDetailEquipmentCardView.GetOrCreateInventoryCard(new HeroDetailEquipmentInventoryCardBuildArgs
            {
                Equipment = entry.Equipment,
                CardKey = entry.CardKey,
                Parent = args.Refs.EquipmentGridTransform,
                CardButtons = args.EquipmentCardButtons,
                CardTexts = args.EquipmentCardTexts,
                ActionButtons = args.EquipmentActionButtons,
                CreateButton = args.CreateButton,
                CreateCornerActionButton = args.CreateCornerActionButton,
                OpenDetail = args.OpenEquipmentDetail,
                ToggleEquipment = args.ToggleEquipment
            });

            if (cardButton == null)
            {
                return;
            }

            bool equipped = !string.IsNullOrEmpty(entry.EquippedHeroId);
            bool equippedToCurrentHero = equipped && entry.EquippedHeroId == args.SelectedHeroId;
            bool selected = entry.Equipment != null
                && args.SelectedEquipmentId == entry.Equipment.Id
                && !equipped;
            HeroState equippedHero = args.FindHeroState != null ? args.FindHeroState(entry.EquippedHeroId) : null;
            string equippedHeroLabel = equippedHero != null && args.GetShortHeroLabel != null
                ? args.GetShortHeroLabel(equippedHero.Definition)
                : string.Empty;

            Text cardText = null;
            args.EquipmentCardTexts?.TryGetValue(entry.CardKey, out cardText);
            Button actionButton = null;
            args.EquipmentActionButtons?.TryGetValue(entry.CardKey, out actionButton);

            HeroDetailEquipmentCardView.ApplyInventoryCard(cardButton, cardText, actionButton, new HeroDetailEquipmentInventoryCardViewState
            {
                Equipment = entry.Equipment,
                State = entry.State,
                EquippedHeroLabel = equippedHeroLabel,
                CopyNumber = entry.CopyNumber,
                Equipped = equipped,
                EquippedToCurrentHero = equippedToCurrentHero,
                Selected = selected
            });
        }

        private static void RefreshEquipmentDetailPopup(
            HeroDetailEquipmentPresenterArgs args,
            HeroDetailEquipmentPresenterResult result)
        {
            if (args.Refs == null || args.Refs.EquipmentDetailPopup == null)
            {
                return;
            }

            if (!args.EquipmentDetailPopupOpen)
            {
                args.Refs.EquipmentDetailPopup.SetActive(false);
                return;
            }

            EquipmentState state = args.EquipmentInventory != null
                ? args.EquipmentInventory.GetState(args.SelectedEquipmentDetailId)
                : null;
            if (args.SelectedHero == null || state == null || !state.IsOwned)
            {
                result.EquipmentDetailPopupInvalid = true;
                args.Refs.EquipmentDetailPopup.SetActive(false);
                return;
            }

            bool equippedToHero = args.EquipmentInventory.IsEquipmentEquippedToHero(args.SelectedHero.Definition.Id, state.Definition.Id);
            int availableCount = args.EquipmentInventory.GetAvailableCount(state.Definition.Id);
            string equippedOwners = HeroDetailEquipmentListBuilder.BuildEquippedOwnerText(
                state.Definition.Id,
                args.EquipmentInventory,
                args.BattleManager != null ? args.BattleManager.Heroes : null,
                args.GetShortHeroLabel);
            int starMaterialCount = args.EquipmentInventory.GetStarUpMaterialCount(state.Definition.Id);
            HeroDetailView.ApplyEquipmentDetailPopup(args.Refs, HeroDetailStateBuilder.BuildEquipmentDetail(new HeroDetailEquipmentDetailStateBuildArgs
            {
                State = state,
                Wallet = args.Wallet,
                EquippedToHero = equippedToHero,
                AvailableCount = availableCount,
                EquippedOwners = equippedOwners,
                StarMaterialCount = starMaterialCount,
                NoticeText = args.NoticeText ?? string.Empty
            }));

            args.Refs.EquipmentDetailPopup.SetActive(true);
        }

        private static void RefreshEquipmentDismantlePopup(HeroDetailEquipmentPresenterArgs args)
        {
            if (args.Refs == null || args.Refs.EquipmentDismantlePopup == null)
            {
                return;
            }

            if (!args.EquipmentDismantlePopupOpen)
            {
                args.Refs.EquipmentDismantlePopup.SetActive(false);
                return;
            }

            PruneInvalidDismantleSelections(
                args.SelectedDismantleEquipmentIds,
                args.EquipmentInventory,
                args.SelectedSlots);

            HeroDetailEquipmentCardView.HideCards(args.DismantleCardButtons);
            HeroDetailEquipmentDismantleListState listState = HeroDetailEquipmentListBuilder.BuildDismantleList(
                GameData.Equipments,
                args.EquipmentInventory,
                args.SelectedSlots,
                args.SelectedDismantleEquipmentIds);

            foreach (HeroDetailEquipmentDismantleListEntry entry in listState.Entries)
            {
                RefreshDismantleCard(args, entry);
            }

            HeroDetailView.ApplyEquipmentDismantlePopup(args.Refs, HeroDetailStateBuilder.BuildEquipmentDismantle(new HeroDetailEquipmentDismantleStateBuildArgs
            {
                SelectedSlots = args.SelectedSlots,
                VisibleCount = listState.VisibleCount,
                SelectedCount = listState.SelectedCount,
                SelectedReward = listState.SelectedReward,
                NoticeText = args.NoticeText ?? string.Empty
            }));

            args.Refs.EquipmentDismantlePopup.SetActive(true);
        }

        private static void RefreshDismantleCard(HeroDetailEquipmentPresenterArgs args, HeroDetailEquipmentDismantleListEntry entry)
        {
            if (entry == null || args.Refs == null)
            {
                return;
            }

            Button cardButton = HeroDetailEquipmentCardView.GetOrCreateDismantleCard(new HeroDetailEquipmentDismantleCardBuildArgs
            {
                Equipment = entry.Equipment,
                CardKey = entry.CardKey,
                Parent = args.Refs.DismantleGridTransform,
                CardButtons = args.DismantleCardButtons,
                CardTexts = args.DismantleCardTexts,
                CreateButton = args.CreateButton,
                SelectCard = args.SelectDismantleEquipment
            });

            if (cardButton == null)
            {
                return;
            }

            Text cardText = null;
            args.DismantleCardTexts?.TryGetValue(entry.CardKey, out cardText);
            HeroDetailEquipmentCardView.ApplyDismantleCard(cardButton, cardText, new HeroDetailEquipmentDismantleCardViewState
            {
                Equipment = entry.Equipment,
                State = entry.State,
                CopyNumber = entry.CopyNumber,
                Reward = entry.Reward,
                Selected = entry.Selected
            });
        }

        private static void RefreshEquipmentBulkDismantlePrompt(HeroDetailEquipmentPresenterArgs args)
        {
            if (args.Refs == null || args.Refs.EquipmentBulkDismantlePrompt == null)
            {
                return;
            }

            if (!args.EquipmentBulkDismantlePromptOpen || !args.EquipmentDismantlePopupOpen)
            {
                args.Refs.EquipmentBulkDismantlePrompt.SetActive(false);
                return;
            }

            HeroDetailEquipmentBulkDismantleCandidateState candidates =
                HeroDetailEquipmentListBuilder.CountBulkDismantleCandidates(
                    args.EquipmentInventory,
                    args.SelectedBulkDismantleRarity);
            HeroDetailView.ApplyEquipmentBulkDismantlePrompt(args.Refs, HeroDetailStateBuilder.BuildEquipmentBulkDismantle(new HeroDetailEquipmentBulkDismantleStateBuildArgs
            {
                SelectedRarity = args.SelectedBulkDismantleRarity,
                Count = candidates.Count,
                Reward = candidates.Reward,
                NoticeText = args.NoticeText ?? string.Empty
            }));

            args.Refs.EquipmentBulkDismantlePrompt.SetActive(true);
        }

        private static void ApplyEquipmentContentState(HeroDetailViewRefs refs, HeroDetailEquipmentContentViewState state)
        {
            if (refs == null || state == null)
            {
                return;
            }

            if (refs.EquipmentSummaryText != null)
            {
                refs.EquipmentSummaryText.text = state.SummaryText;
            }

            if (refs.EquipmentEmptyText != null)
            {
                refs.EquipmentEmptyText.text = state.EmptyText;
                refs.EquipmentEmptyText.gameObject.SetActive(state.EmptyVisible);
            }
        }
    }
}
