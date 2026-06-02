using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;

namespace IdleGame.UI.Facility
{
    public sealed class FacilityViewRefs
    {
        public GameObject Content;
        public GameObject AssignmentModal;
        public Text SummaryText;
        public Text NoticeText;
    }

    public sealed class FacilityRewardPopupRefs
    {
        public GameObject Popup;
        public Text ListText;
    }

    public sealed class FacilityViewBuildArgs
    {
        public Transform Parent;
        public IReadOnlyList<FacilityDefinition> Facilities;
        public Action<string> OnCollectFacility;
        public Action<string> OnUpgradeFacility;
        public Action OnOpenAssignments;
        public Action OnCollectAll;
        public Action OnCloseAssignments;
        public Action OnAutoAssignAll;
        public Action OnClearAssignments;
        public Dictionary<string, Text> FacilityCardTexts;
        public Dictionary<string, Button> FacilityUpgradeButtons;
        public Dictionary<string, Button> FacilityCollectButtons;
        public Dictionary<string, Text> AssignmentRowTexts;
        public Dictionary<string, List<Text>> AssignmentSlotTexts;
    }

    public sealed class FacilityCardViewState
    {
        public string Text;
        public bool CollectInteractable;
        public string CollectText;
        public Color CollectColor;
        public bool UpgradeInteractable;
        public string UpgradeText;
        public Color UpgradeColor;

        public bool IsSameAs(FacilityCardViewState other)
        {
            return other != null
                && Text == other.Text
                && CollectInteractable == other.CollectInteractable
                && CollectText == other.CollectText
                && CollectColor == other.CollectColor
                && UpgradeInteractable == other.UpgradeInteractable
                && UpgradeText == other.UpgradeText
                && UpgradeColor == other.UpgradeColor;
        }
    }

    public sealed class FacilityAssignmentSlotViewState
    {
        public string Text;
        public Color TextColor;
        public Color CardColor;

        public bool IsSameAs(FacilityAssignmentSlotViewState other)
        {
            return other != null
                && Text == other.Text
                && TextColor == other.TextColor
                && CardColor == other.CardColor;
        }
    }
}
