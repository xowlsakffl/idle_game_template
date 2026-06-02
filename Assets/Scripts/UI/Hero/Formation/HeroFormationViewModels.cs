using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;

namespace IdleGame.UI.Hero.Formation
{
    public sealed class HeroFormationViewRefs
    {
        public GameObject Content;
        public Text SummaryText;
        public Text OwnedEffectText;
        public RectTransform RosterGridRect;
    }

    public sealed class HeroRosterCardState
    {
        public bool IsOwned;
        public bool IsDeployed;
        public bool NeedsAttention;
        public bool ActionInteractable;
        public string DisplayText;
        public string ActionText;
        public Color ButtonColor;
        public Color ActionColor;

        public bool IsSameAs(HeroRosterCardState other)
        {
            return other != null
                && IsOwned == other.IsOwned
                && IsDeployed == other.IsDeployed
                && NeedsAttention == other.NeedsAttention
                && ActionInteractable == other.ActionInteractable
                && DisplayText == other.DisplayText
                && ActionText == other.ActionText
                && ButtonColor == other.ButtonColor
                && ActionColor == other.ActionColor;
        }
    }

    public sealed class HeroFormationSlotState
    {
        public bool Interactable;
        public bool RemoveVisible;
        public string Text;
        public Color TextColor;
        public Color ButtonColor;
    }

    public sealed class HeroFormationRuneSlotState
    {
        public bool Interactable;
        public bool RemoveVisible;
        public string Text;
        public Color ButtonColor;
    }

    public sealed class HeroFormationViewBuildArgs
    {
        public Transform Parent;
        public IReadOnlyList<HeroDefinition> RosterHeroes;
        public Func<HeroRarity, Color> GetRarityColor;
        public Action<int> OnFormationSlotClick;
        public Action<int> OnFormationSlotRemove;
        public Action<int> OnPresetClick;
        public Action<int> OnRuneSlotClick;
        public Action<int> OnRuneSlotRemove;
        public Action<string> OnHeroCardClick;
        public Action<string> OnHeroRosterActionClick;
        public Action OnAutoArrange;
        public Action OnBulkStarUp;
        public Dictionary<int, Button> PresetButtons;
        public Dictionary<int, Button> FormationSlotButtons;
        public Dictionary<int, Button> FormationSlotRemoveButtons;
        public Dictionary<int, Button> RuneSlotButtons;
        public Dictionary<int, Text> RuneSlotTexts;
        public Dictionary<int, Button> RuneSlotRemoveButtons;
        public Dictionary<string, Button> HeroRosterButtons;
        public Dictionary<string, Text> HeroButtonTexts;
        public Dictionary<string, Button> HeroRosterActionButtons;
        public Dictionary<string, GameObject> HeroRosterDeployedOverlays;
        public Dictionary<string, GameObject> HeroNotificationDots;
        public List<Text> FormationSlotTexts;
    }
}
