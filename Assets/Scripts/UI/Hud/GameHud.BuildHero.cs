using System.Collections.Generic;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Hero;
using IdleGame.UI.Hero.Detail;
using IdleGame.UI.Hero.Formation;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void CreateHeroFormationSavePrompt(Transform parent)
        {
            heroHud.FormationSavePrompt = HeroFormationSavePromptView.Build(
                parent,
                ConfirmHeroFormationSavePrompt,
                CancelHeroFormationSavePrompt);
        }

        private void CreateHeroDetailPanel(Transform parent)
        {
            heroHud.DetailViewRefs = HeroDetailView.Build(new HeroDetailViewBuildArgs
            {
                Parent = parent,
                OnToggleFormation = ToggleSelectedHeroDetailFormation,
                OnLevelUpHero = LevelUpSelectedHeroDetail,
                CanLevelUpHero = CanLevelUpSelectedHeroDetail,
                OnStarUpHero = StarUpSelectedHeroDetail,
                OnPlaceEquipmentSlot = TryPlaceSelectedHeroDetailEquipment,
                OnRemoveEquipmentSlot = RemoveHeroDetailEquipment,
                OnToggleEquipmentFilter = ToggleHeroDetailEquipmentFilter,
                OnOpenEquipmentDismantle = OpenEquipmentDismantlePopup,
                OnUnequipAllEquipment = UnequipAllHeroDetailEquipment,
                OnAutoEquipEquipment = AutoEquipHeroDetailEquipment,
                OnSelectTranscendSlot = slot =>
                {
                    heroTranscendState.SelectSlot(slot);
                    UpdateView();
                },
                OnToggleTranscendSlotLock = ToggleHeroTranscendSlotLock,
                OnToggleTranscendStopMode = ToggleHeroTranscendStopMode,
                OnRollTranscendManual = RollSelectedHeroTranscendManual,
                OnAutoRollTranscend = AutoRollSelectedHeroTranscend,
                OnSelectTab = SelectHeroDetailTab,
                OnConfirmTranscendRollPrompt = ConfirmHeroTranscendRollPrompt,
                OnCancelTranscendRollPrompt = CancelHeroTranscendRollPrompt,
                OnToggleSelectedEquipmentDetailEquip = ToggleSelectedEquipmentDetailEquip,
                OnLevelUpSelectedEquipmentDetail = LevelUpSelectedEquipmentDetail,
                CanLevelUpSelectedEquipmentDetail = CanLevelUpSelectedEquipmentDetail,
                OnStarUpSelectedEquipmentDetail = StarUpSelectedEquipmentDetail,
                OnCloseEquipmentDetailPopup = CloseEquipmentDetailPopup,
                OnDismantleSelectedEquipment = DismantleSelectedEquipment,
                OnOpenEquipmentBulkDismantlePrompt = OpenEquipmentBulkDismantlePrompt,
                OnCloseEquipmentDismantlePopup = CloseEquipmentDismantlePopup,
                OnChangeBulkDismantleRarity = ChangeBulkDismantleRarity,
                OnConfirmBulkDismantleEquipment = ConfirmBulkDismantleEquipment,
                OnCloseEquipmentBulkDismantlePrompt = CloseEquipmentBulkDismantlePrompt,
                TabButtons = heroHud.DetailTabButtons,
                EquipmentSlotButtons = heroHud.DetailEquipmentSlotButtons,
                EquipmentSlotTexts = heroHud.DetailEquipmentSlotTexts,
                EquipmentSlotRemoveButtons = heroHud.DetailEquipmentSlotRemoveButtons,
                EquipmentFilterButtons = heroHud.DetailEquipmentFilterButtons,
                DismantleFilterButtons = heroHud.EquipmentDismantleFilterButtons,
                SelectedEquipmentSlots = heroDetailEquipmentState.SelectedSlots,
                TranscendSlotButtons = heroHud.DetailTranscendSlotButtons,
                TranscendSlotTexts = heroHud.DetailTranscendSlotTexts,
                TranscendLockButtons = heroHud.DetailTranscendLockButtons
            });

            HeroDetailViewRefs refs = heroHud.DetailViewRefs;
            heroHud.DetailPanel = refs.Panel;
            heroHud.DetailStatsPanel = refs.StatsPanel;
            heroHud.DetailActionRow = refs.ActionRow;
            heroHud.DetailEquipmentContent = refs.EquipmentContent;
            heroHud.DetailTranscendContent = refs.TranscendContent;
            heroHud.TranscendConfirmPrompt = refs.TranscendConfirmPrompt;
            heroHud.DetailTranscendText = refs.TranscendText;
            heroHud.DetailTranscendNoticeText = refs.TranscendNoticeText;
            heroHud.TranscendConfirmMessageText = refs.TranscendConfirmMessageText;
            heroHud.DetailExcludeButton = refs.ExcludeButton;
            heroHud.DetailLevelUpButton = refs.LevelUpButton;
            heroHud.DetailStarUpButton = refs.StarUpButton;
            heroHud.DetailTranscendChangeButton = refs.TranscendChangeButton;
            heroHud.DetailTranscendAutoButton = refs.TranscendAutoButton;
            heroHud.DetailTranscendStopButton = refs.TranscendStopButton;
        }

        private void CreateHeroPanel(Transform parent)
        {
            HeroPanelView.BuildHeader(parent);

            HeroFormationViewRefs formationRefs = HeroFormationView.Build(new HeroFormationViewBuildArgs
            {
                Parent = parent,
                RosterHeroes = GetSortedHeroRosterDefinitions(),
                GetRarityColor = HeroUiText.GetRarityColor,
                OnFormationSlotClick = TryPlaceSelectedHeroInSlot,
                OnFormationSlotRemove = RemoveHeroFromEditingFormationSlot,
                OnPresetClick = RequestHeroPresetChange,
                OnRuneSlotClick = HandleFormationRuneSlotClick,
                OnRuneSlotRemove = RemoveRuneFromFormationSlot,
                OnHeroCardClick = OpenHeroDetailPanel,
                OnHeroRosterActionClick = SelectOrRemoveRosterHero,
                OnAutoArrange = AutoArrangeEditingFormation,
                OnBulkStarUp = BulkStarUpHeroesFromHud,
                PresetButtons = heroHud.PresetButtons,
                FormationSlotButtons = heroHud.FormationSlotButtons,
                FormationSlotRemoveButtons = heroHud.FormationSlotRemoveButtons,
                RuneSlotButtons = heroHud.FormationRuneSlotButtons,
                RuneSlotTexts = heroHud.FormationRuneSlotTexts,
                RuneSlotRemoveButtons = heroHud.FormationRuneSlotRemoveButtons,
                HeroRosterButtons = heroHud.RosterButtons,
                HeroButtonTexts = heroHud.HeroButtonTexts,
                HeroRosterActionButtons = heroHud.RosterActionButtons,
                HeroRosterDeployedOverlays = heroHud.RosterDeployedOverlays,
                HeroNotificationDots = heroHud.NotificationDots,
                FormationSlotTexts = heroHud.FormationSlotTexts
            });

            heroHud.FormationContent = formationRefs.Content;
            heroHud.FormationSummaryText = formationRefs.SummaryText;
            heroHud.OwnedEffectText = formationRefs.OwnedEffectText;
            heroHud.RosterGridRect = formationRefs.RosterGridRect;
            CreateHeroSubContent(parent);

            HeroPanelViewRefs heroPanelRefs = HeroPanelView.BuildFooter(new HeroPanelViewBuildFooterArgs
            {
                Parent = parent,
                OnTabClick = RequestHeroPageTabChange,
                TabButtons = heroHud.PageTabButtons
            });
            heroHud.PlaceholderText = heroPanelRefs.PlaceholderText;
        }

        private void CreateHeroSubContent(Transform parent)
        {
            HeroSubContentViewRefs refs = HeroSubContentView.Build(new HeroSubContentViewBuildArgs
            {
                Parent = parent,
                Totems = GameData.Totems,
                Runes = GameData.Runes,
                OnTalentSelected = talentId =>
                {
                    selectedHeroTraitId = talentId;
                    UpdateView();
                },
                OnTraitLevelUp = LevelUpSelectedHeroTrait,
                CanTraitLevelUp = CanLevelUpSelectedHeroTrait,
                OnSelectTotem = SelectTotem,
                OnLevelUpTotem = LevelUpSelectedTotem,
                CanLevelUpTotem = CanLevelUpSelectedTotem,
                OnSelectRune = SelectRune,
                OnRuneAction = StartPendingRuneEquip,
                OnEquipSelectedRune = EquipSelectedRune,
                OnLevelUpRune = LevelUpSelectedRune,
                CanLevelUpRune = CanLevelUpSelectedRune,
                TalentButtons = heroHud.TraitButtons,
                TalentButtonTexts = heroHud.TraitButtonTexts,
                TotemButtons = heroHud.TotemButtons,
                TotemButtonTexts = heroHud.TotemButtonTexts,
                RuneButtons = heroHud.RuneButtons,
                RuneButtonTexts = heroHud.RuneButtonTexts,
                RuneActionButtons = heroHud.RuneActionButtons
            });

            heroHud.TraitContent = refs.TraitContent;
            heroHud.TraitSummaryText = refs.TraitSummaryText;
            heroHud.TraitDetailText = refs.TraitDetailText;
            heroHud.TraitLevelUpButton = refs.TraitLevelUpButton;
            heroHud.TotemContent = refs.TotemContent;
            heroHud.TotemSummaryText = refs.TotemSummaryText;
            heroHud.TotemDetailText = refs.TotemDetailText;
            heroHud.TotemLevelUpButton = refs.TotemLevelUpButton;
            heroHud.RuneContent = refs.RuneContent;
            heroHud.RuneSummaryText = refs.RuneSummaryText;
            heroHud.RuneDetailText = refs.RuneDetailText;
            heroHud.RuneEquipButton = refs.RuneEquipButton;
            heroHud.RuneLevelUpButton = refs.RuneLevelUpButton;
        }

        private List<HeroDefinition> GetSortedHeroRosterDefinitions()
        {
            return HeroFormationDraftRules.SortRosterDefinitions(GameData.Heroes);
        }
    }
}
