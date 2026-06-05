using UnityEngine.UI;
using UnityEngine;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hud
{
    public sealed class ContentPanelsViewRefs
    {
        public GameObject Root;
        public LayoutElement LayoutElement;
        public GameObject GrowthPanel;
        public GameObject HeroPanel;
        public GameObject FortressPanel;
        public GameObject FacilityPanel;
        public GameObject StagePanel;
        public GameObject SummonPanel;
        public GameObject ShopPanel;
        public GameObject SupportPanel;
        public GameObject DebugPanel;
    }

    public sealed class ContentPanelsViewBuildArgs
    {
        public Transform Parent;
        public bool ShowDebugPanel;
    }

    public static class ContentPanelsView
    {
        public static ContentPanelsViewRefs Build(ContentPanelsViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new ContentPanelsViewRefs();
            }

            var refs = new ContentPanelsViewRefs();
            refs.Root = HudUiFactory.CreatePanel("Content", args.Parent, new Color(0.09f, 0.10f, 0.13f, 1f));
            refs.LayoutElement = HudUiFactory.AddLayoutElement(refs.Root, -1, HudLayoutConfig.GrowthContentPanelHeight);

            RectTransform contentRect = refs.Root.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 0f);
            contentRect.anchorMax = new Vector2(1f, 1f);

            refs.GrowthPanel = CreateTabPanel(refs.Root.transform, "GrowthPanel", new Color(0.13f, 0.16f, 0.22f, 1f));
            refs.HeroPanel = CreateTabPanel(refs.Root.transform, "HeroPanel", new Color(0.12f, 0.16f, 0.23f, 1f));
            refs.FortressPanel = CreateTabPanel(refs.Root.transform, "FortressPanel", new Color(0.13f, 0.17f, 0.22f, 1f));
            refs.FacilityPanel = CreateTabPanel(refs.Root.transform, "FacilityPanel", new Color(0.12f, 0.16f, 0.23f, 1f));
            refs.StagePanel = CreateTabPanel(refs.Root.transform, "StagePanel", new Color(0.12f, 0.15f, 0.19f, 1f));
            refs.SummonPanel = CreateTabPanel(refs.Root.transform, "SummonPanel", new Color(0.15f, 0.13f, 0.19f, 1f));
            refs.ShopPanel = CreateTabPanel(refs.Root.transform, "ShopPanel", new Color(0.16f, 0.13f, 0.10f, 1f));
            refs.SupportPanel = CreateTabPanel(refs.Root.transform, "SupportPanel", new Color(0.11f, 0.15f, 0.16f, 1f));

            if (args.ShowDebugPanel)
            {
                refs.DebugPanel = CreateTabPanel(refs.Root.transform, "DebugPanel", new Color(0.13f, 0.13f, 0.13f, 1f));
            }

            return refs;
        }

        private static GameObject CreateTabPanel(Transform parent, string name, Color color)
        {
            GameObject panel = HudUiFactory.CreatePanel(name, parent, color);
            HudUiFactory.ApplyNinePatchPanel(panel, HudSpriteKind.SpecialPaperPanel, new Color(0.74f, 0.80f, 0.88f, 0.98f));
            HudUiFactory.StretchToParent(panel);
            return panel;
        }
    }
}
