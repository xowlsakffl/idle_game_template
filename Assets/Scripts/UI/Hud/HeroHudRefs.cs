using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.UI.Hero;
using IdleGame.UI.Hero.Detail;
using UnityEngine;
using UnityEngine.UI;

namespace IdleGame.UI.Hud
{
    public sealed class HeroHudRefs
    {
        public HeroDetailViewRefs DetailViewRefs;

        public GameObject Panel;
        public GameObject FormationContent;
        public GameObject TraitContent;
        public GameObject TotemContent;
        public GameObject RuneContent;
        public GameObject DetailPanel;
        public GameObject DetailStatsPanel;
        public GameObject DetailActionRow;
        public GameObject DetailEquipmentContent;
        public GameObject DetailTranscendContent;
        public GameObject TranscendConfirmPrompt;
        public GameObject FormationSavePrompt;
        public RectTransform RosterGridRect;

        public Text FormationSummaryText;
        public Text TraitSummaryText;
        public Text TraitDetailText;
        public Text TotemSummaryText;
        public Text TotemDetailText;
        public Text RuneSummaryText;
        public Text RuneDetailText;
        public Text FormationTotemText;
        public Text FormationTotemSecondText;
        public Text OwnedEffectText;
        public Text PlaceholderText;
        public Text DetailTranscendText;
        public Text DetailTranscendNoticeText;
        public Text TranscendConfirmMessageText;

        public readonly Dictionary<string, Text> HeroButtonTexts = new Dictionary<string, Text>();
        public readonly Dictionary<string, Button> TraitButtons = new Dictionary<string, Button>();
        public readonly Dictionary<string, Text> TraitButtonTexts = new Dictionary<string, Text>();
        public readonly Dictionary<string, Button> TotemButtons = new Dictionary<string, Button>();
        public readonly Dictionary<string, Text> TotemButtonTexts = new Dictionary<string, Text>();
        public readonly Dictionary<string, Button> TotemActionButtons = new Dictionary<string, Button>();
        public readonly Dictionary<string, Button> RuneButtons = new Dictionary<string, Button>();
        public readonly Dictionary<string, Text> RuneButtonTexts = new Dictionary<string, Text>();
        public readonly Dictionary<string, Button> RuneActionButtons = new Dictionary<string, Button>();
        public readonly Dictionary<int, Button> FormationRuneSlotButtons = new Dictionary<int, Button>();
        public readonly Dictionary<int, Text> FormationRuneSlotTexts = new Dictionary<int, Text>();
        public readonly Dictionary<int, Button> FormationRuneSlotRemoveButtons = new Dictionary<int, Button>();
        public readonly Dictionary<HeroPageTab, Button> PageTabButtons = new Dictionary<HeroPageTab, Button>();
        public readonly Dictionary<HeroDetailTab, Button> DetailTabButtons = new Dictionary<HeroDetailTab, Button>();
        public readonly Dictionary<EquipmentSlot, Button> DetailEquipmentSlotButtons = new Dictionary<EquipmentSlot, Button>();
        public readonly Dictionary<EquipmentSlot, Text> DetailEquipmentSlotTexts = new Dictionary<EquipmentSlot, Text>();
        public readonly Dictionary<EquipmentSlot, Button> DetailEquipmentSlotRemoveButtons = new Dictionary<EquipmentSlot, Button>();
        public readonly Dictionary<EquipmentSlot, Button> DetailEquipmentFilterButtons = new Dictionary<EquipmentSlot, Button>();
        public readonly Dictionary<EquipmentSlot, Button> EquipmentDismantleFilterButtons = new Dictionary<EquipmentSlot, Button>();
        public readonly Dictionary<string, Button> DetailEquipmentCardButtons = new Dictionary<string, Button>();
        public readonly Dictionary<string, Text> DetailEquipmentCardTexts = new Dictionary<string, Text>();
        public readonly Dictionary<string, Button> DetailEquipmentActionButtons = new Dictionary<string, Button>();
        public readonly Dictionary<string, Button> EquipmentDismantleCardButtons = new Dictionary<string, Button>();
        public readonly Dictionary<string, Text> EquipmentDismantleCardTexts = new Dictionary<string, Text>();
        public readonly List<Button> DetailTranscendSlotButtons = new List<Button>();
        public readonly List<Text> DetailTranscendSlotTexts = new List<Text>();
        public readonly List<Button> DetailTranscendLockButtons = new List<Button>();
        public readonly Dictionary<int, Button> PresetButtons = new Dictionary<int, Button>();
        public readonly Dictionary<int, Button> FormationSlotButtons = new Dictionary<int, Button>();
        public readonly Dictionary<int, Button> FormationSlotRemoveButtons = new Dictionary<int, Button>();
        public readonly Dictionary<string, Button> RosterButtons = new Dictionary<string, Button>();
        public readonly Dictionary<string, Button> RosterActionButtons = new Dictionary<string, Button>();
        public readonly Dictionary<string, GameObject> RosterDeployedOverlays = new Dictionary<string, GameObject>();
        public readonly List<Text> FormationSlotTexts = new List<Text>();
        public readonly Dictionary<string, GameObject> NotificationDots = new Dictionary<string, GameObject>();
        public readonly Dictionary<string, Text> SkillStatusTexts = new Dictionary<string, Text>();
        public readonly Dictionary<string, Text> PetStatusTexts = new Dictionary<string, Text>();

        public Button TraitLevelUpButton;
        public Button DetailExcludeButton;
        public Button DetailLevelUpButton;
        public Button DetailStarUpButton;
        public Button FormationTotemButton;
        public Button FormationTotemSecondButton;
        public Button FormationTotemRemoveButton;
        public Button FormationTotemSecondRemoveButton;
        public Button TotemEquipButton;
        public Button TotemLevelUpButton;
        public Button RuneEquipButton;
        public Button RuneLevelUpButton;
        public Button DetailTranscendChangeButton;
        public Button DetailTranscendAutoButton;
        public Button DetailTranscendStopButton;

        public void Reset()
        {
            DetailViewRefs = null;
            Panel = null;
            FormationContent = null;
            TraitContent = null;
            TotemContent = null;
            RuneContent = null;
            DetailPanel = null;
            DetailStatsPanel = null;
            DetailActionRow = null;
            DetailEquipmentContent = null;
            DetailTranscendContent = null;
            TranscendConfirmPrompt = null;
            FormationSavePrompt = null;
            RosterGridRect = null;

            FormationSummaryText = null;
            TraitSummaryText = null;
            TraitDetailText = null;
            TotemSummaryText = null;
            TotemDetailText = null;
            RuneSummaryText = null;
            RuneDetailText = null;
            FormationTotemText = null;
            FormationTotemSecondText = null;
            OwnedEffectText = null;
            PlaceholderText = null;
            DetailTranscendText = null;
            DetailTranscendNoticeText = null;
            TranscendConfirmMessageText = null;

            HeroButtonTexts.Clear();
            TraitButtons.Clear();
            TraitButtonTexts.Clear();
            TotemButtons.Clear();
            TotemButtonTexts.Clear();
            TotemActionButtons.Clear();
            RuneButtons.Clear();
            RuneButtonTexts.Clear();
            RuneActionButtons.Clear();
            FormationRuneSlotButtons.Clear();
            FormationRuneSlotTexts.Clear();
            FormationRuneSlotRemoveButtons.Clear();
            PageTabButtons.Clear();
            DetailTabButtons.Clear();
            DetailEquipmentSlotButtons.Clear();
            DetailEquipmentSlotTexts.Clear();
            DetailEquipmentSlotRemoveButtons.Clear();
            DetailEquipmentFilterButtons.Clear();
            EquipmentDismantleFilterButtons.Clear();
            DetailEquipmentCardButtons.Clear();
            DetailEquipmentCardTexts.Clear();
            DetailEquipmentActionButtons.Clear();
            EquipmentDismantleCardButtons.Clear();
            EquipmentDismantleCardTexts.Clear();
            DetailTranscendSlotButtons.Clear();
            DetailTranscendSlotTexts.Clear();
            DetailTranscendLockButtons.Clear();
            PresetButtons.Clear();
            FormationSlotButtons.Clear();
            FormationSlotRemoveButtons.Clear();
            RosterButtons.Clear();
            RosterActionButtons.Clear();
            RosterDeployedOverlays.Clear();
            FormationSlotTexts.Clear();
            NotificationDots.Clear();
            SkillStatusTexts.Clear();
            PetStatusTexts.Clear();

            TraitLevelUpButton = null;
            DetailExcludeButton = null;
            DetailLevelUpButton = null;
            DetailStarUpButton = null;
            FormationTotemButton = null;
            FormationTotemSecondButton = null;
            FormationTotemRemoveButton = null;
            FormationTotemSecondRemoveButton = null;
            TotemEquipButton = null;
            TotemLevelUpButton = null;
            RuneEquipButton = null;
            RuneLevelUpButton = null;
            DetailTranscendChangeButton = null;
            DetailTranscendAutoButton = null;
            DetailTranscendStopButton = null;
        }
    }
}
