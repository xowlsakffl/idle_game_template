using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;

namespace IdleGame.UI.Hero.Detail
{
    public sealed class HeroDetailViewRefs
    {
        public GameObject Panel;
        public GameObject StatsPanel;
        public GameObject ActionRow;
        public GameObject EquipmentContent;
        public GameObject TranscendContent;
        public GameObject EquipmentDetailPopup;
        public GameObject EquipmentDismantlePopup;
        public GameObject EquipmentBulkDismantlePrompt;
        public GameObject TranscendConfirmPrompt;
        public Transform EquipmentGridTransform;
        public Transform DismantleGridTransform;
        public Text TitleText;
        public Text TraitText;
        public Text StarsText;
        public Text CharacterText;
        public Text LevelText;
        public Text PowerText;
        public Text ExpBookText;
        public Text SkillText;
        public Text StatsText;
        public Text StarEffectsText;
        public Text OwnedEffectText;
        public Text NoticeText;
        public Text EquipmentSummaryText;
        public Text EquipmentEmptyText;
        public Text TranscendText;
        public Text TranscendNoticeText;
        public Text TranscendConfirmMessageText;
        public Text EquipmentDetailIconText;
        public Text EquipmentDetailTitleText;
        public Text EquipmentDetailMetaText;
        public Text EquipmentDetailStatsText;
        public Text EquipmentDetailSetText;
        public Text EquipmentDetailBookText;
        public Text EquipmentDetailNoticeText;
        public Text EquipmentDismantleSummaryText;
        public Text EquipmentDismantleEmptyText;
        public Text EquipmentDismantleNoticeText;
        public Text EquipmentBulkDismantleRarityText;
        public Text EquipmentBulkDismantleInfoText;
        public Text EquipmentBulkDismantleNoticeText;
        public Button ExcludeButton;
        public Button LevelUpButton;
        public Button StarUpButton;
        public Button TranscendChangeButton;
        public Button TranscendAutoButton;
        public Button TranscendStopButton;
        public Button EquipmentDetailEquipButton;
        public Button EquipmentDetailLevelUpButton;
        public Button EquipmentDetailStarUpButton;
        public Button EquipmentDismantleButton;
        public Button EquipmentBulkDismantleButton;
    }

    public sealed class HeroDetailViewBuildArgs
    {
        public Transform Parent;
        public Action OnToggleFormation;
        public Action OnLevelUpHero;
        public Func<bool> CanLevelUpHero;
        public Action OnStarUpHero;
        public Action<EquipmentSlot> OnPlaceEquipmentSlot;
        public Action<EquipmentSlot> OnRemoveEquipmentSlot;
        public Action<EquipmentSlot> OnToggleEquipmentFilter;
        public Action OnOpenEquipmentDismantle;
        public Action OnUnequipAllEquipment;
        public Action OnAutoEquipEquipment;
        public Action<int> OnSelectTranscendSlot;
        public Action<int> OnToggleTranscendSlotLock;
        public Action OnToggleTranscendStopMode;
        public Action OnRollTranscendManual;
        public Action OnAutoRollTranscend;
        public Action<HeroDetailTab> OnSelectTab;
        public Action OnConfirmTranscendRollPrompt;
        public Action OnCancelTranscendRollPrompt;
        public Action OnToggleSelectedEquipmentDetailEquip;
        public Action OnLevelUpSelectedEquipmentDetail;
        public Func<bool> CanLevelUpSelectedEquipmentDetail;
        public Action OnStarUpSelectedEquipmentDetail;
        public Action OnCloseEquipmentDetailPopup;
        public Action OnDismantleSelectedEquipment;
        public Action OnOpenEquipmentBulkDismantlePrompt;
        public Action OnCloseEquipmentDismantlePopup;
        public Action<int> OnChangeBulkDismantleRarity;
        public Action OnConfirmBulkDismantleEquipment;
        public Action OnCloseEquipmentBulkDismantlePrompt;
        public IDictionary<HeroDetailTab, Button> TabButtons;
        public IDictionary<EquipmentSlot, Button> EquipmentSlotButtons;
        public IDictionary<EquipmentSlot, Text> EquipmentSlotTexts;
        public IDictionary<EquipmentSlot, Button> EquipmentSlotRemoveButtons;
        public IDictionary<EquipmentSlot, Button> EquipmentFilterButtons;
        public IDictionary<EquipmentSlot, Button> DismantleFilterButtons;
        public ICollection<EquipmentSlot> SelectedEquipmentSlots;
        public IList<Button> TranscendSlotButtons;
        public IList<Text> TranscendSlotTexts;
        public IList<Button> TranscendLockButtons;
    }

    public sealed class HeroDetailButtonViewState
    {
        public bool Interactable;
        public string Text;
        public Color Color;
    }

    public sealed class HeroDetailActionViewState
    {
        public HeroDetailButtonViewState Formation;
        public HeroDetailButtonViewState LevelUp;
        public HeroDetailButtonViewState StarUp;
    }

    public sealed class HeroDetailBasicViewState
    {
        public string TitleText;
        public string TraitText;
        public string StarsText;
        public string CharacterText;
        public Color CharacterColor;
        public string LevelText;
        public string PowerText;
        public string ResourceText;
        public string SkillText;
        public string StatsText;
        public string StarEffectsText;
        public string OwnedEffectText;
        public string NoticeText;
    }

    public sealed class HeroDetailEquipmentSlotViewState
    {
        public EquipmentSlot Slot;
        public string Text;
        public Color TextColor;
        public Color ButtonColor;
        public bool RemoveVisible;
    }

    public sealed class HeroDetailEquipmentDetailViewState
    {
        public string IconText;
        public Color IconColor;
        public string MetaText;
        public string TitleText;
        public string StatsText;
        public string SetText;
        public string BookText;
        public string NoticeText;
        public HeroDetailButtonViewState EquipButton;
        public HeroDetailButtonViewState LevelUpButton;
        public HeroDetailButtonViewState StarUpButton;
    }

    public sealed class HeroDetailEquipmentDismantleViewState
    {
        public string SummaryText;
        public bool EmptyVisible;
        public string NoticeText;
        public HeroDetailButtonViewState DismantleButton;
    }

    public sealed class HeroDetailEquipmentBulkDismantleViewState
    {
        public string InfoText;
        public string RarityText;
        public Color RarityColor;
        public string NoticeText;
    }
}
