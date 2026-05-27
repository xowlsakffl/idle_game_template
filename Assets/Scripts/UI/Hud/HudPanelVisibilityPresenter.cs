using UnityEngine.UI;
using UnityEngine;

namespace IdleGame.UI.Hud
{
    public sealed class HudPanelVisibilityPresenterArgs
    {
        public HudPanelRefreshState PanelState;
        public GameObject BattlePanel;
        public LayoutElement BattleLayoutElement;
        public GameObject ContentRoot;
        public LayoutElement ContentLayoutElement;
        public GameObject GrowthPanel;
        public GameObject HeroPanel;
        public GameObject FortressPanel;
        public GameObject FacilityPanel;
        public GameObject StagePanel;
        public GameObject SummonPanel;
        public GameObject ShopPanel;
        public GameObject SupportPanel;
        public GameObject DebugPanel;
        public GameObject HeroFacilityContent;
    }

    public static class HudPanelVisibilityPresenter
    {
        public static void Refresh(HudPanelVisibilityPresenterArgs args)
        {
            if (args == null || args.PanelState == null)
            {
                return;
            }

            HudPanelRefreshState state = args.PanelState;
            SetActive(args.BattlePanel, state.BattlePanelHeight > 0.5f);
            SetPreferredHeight(args.BattleLayoutElement, state.BattlePanelHeight);
            SetPreferredHeight(args.ContentLayoutElement, state.ContentPanelHeight);
            SetActive(args.ContentRoot, state.ContentPanelHeight > 0.5f);

            SetActive(args.GrowthPanel, state.GrowthPanelOpen);
            SetActive(args.HeroPanel, state.HeroPanelOpen);
            SetActive(args.FortressPanel, state.FortressPanelOpen);
            SetActive(args.FacilityPanel, state.FacilityPanelOpen);
            SetActive(args.StagePanel, state.StagePanelOpen);
            SetActive(args.SummonPanel, state.SummonPanelOpen);
            SetActive(args.ShopPanel, state.ShopPanelOpen);
            SetActive(args.SupportPanel, state.SupportPanelOpen);
            SetActive(args.DebugPanel, state.DebugPanelOpen);
            SetActive(args.HeroFacilityContent, state.FacilityPanelOpen);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static void SetPreferredHeight(LayoutElement layoutElement, float preferredHeight)
        {
            if (layoutElement != null)
            {
                layoutElement.preferredHeight = preferredHeight;
            }
        }
    }
}
