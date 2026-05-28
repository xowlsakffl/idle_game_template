using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Trait
{
    public sealed class HeroTraitViewRefs
    {
        public GameObject Content;
        public Text SummaryText;
        public Text DetailText;
        public Button LevelUpButton;
    }

    public sealed class HeroTraitViewBuildArgs
    {
        public Transform Parent;
        public Action<string> OnTalentSelected;
        public Action OnLevelUp;
        public Func<bool> CanLevelUp;
        public Dictionary<string, Button> TalentButtons;
        public Dictionary<string, Text> TalentButtonTexts;
    }

    public static class HeroTraitView
    {
        public static HeroTraitViewRefs Build(HeroTraitViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new HeroTraitViewRefs();
            }

            HeroTraitViewRefs refs = new HeroTraitViewRefs();
            refs.Content = HudUiFactory.CreatePanel("HeroTraitContent", args.Parent, new Color(0.25f, 0.33f, 0.48f, 1f));
            VerticalLayoutGroup layout = refs.Content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 12, 12);
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            LayoutElement contentLayout = HudUiFactory.AddLayoutElement(refs.Content, -1, 594);
            contentLayout.flexibleHeight = 1f;

            refs.SummaryText = HudUiFactory.CreateText("HeroTraitSummary", refs.Content.transform, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            HudUiFactory.AddLayoutElement(refs.SummaryText.gameObject, -1, 42);

            CreateTalentTree(args, refs.Content.transform);
            CreateDetailPanel(args, refs);

            refs.Content.SetActive(false);
            return refs;
        }

        private static void CreateTalentTree(HeroTraitViewBuildArgs args, Transform parent)
        {
            GameObject treePanel = HudUiFactory.CreatePanel("HeroTraitTree", parent, new Color(0.30f, 0.39f, 0.56f, 1f));
            HudUiFactory.AddLayoutElement(treePanel, -1, 338);
            ScrollRect treeScroll = treePanel.AddComponent<ScrollRect>();
            treeScroll.horizontal = true;
            treeScroll.vertical = false;
            treeScroll.inertia = true;
            treeScroll.scrollSensitivity = 34f;

            GameObject viewport = new GameObject("HeroTraitTreeViewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(treePanel.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(10f, 10f);
            viewportRect.offsetMax = new Vector2(-10f, -10f);

            GameObject content = new GameObject("HeroTraitTreeContent", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 0f);
            contentRect.anchorMax = new Vector2(0f, 1f);
            contentRect.pivot = new Vector2(0f, 0.5f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup treeLayout = content.AddComponent<HorizontalLayoutGroup>();
            treeLayout.padding = new RectOffset(4, 4, 0, 0);
            treeLayout.spacing = 0;
            treeLayout.childControlWidth = true;
            treeLayout.childControlHeight = true;
            treeLayout.childForceExpandWidth = false;
            treeLayout.childForceExpandHeight = true;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            treeScroll.viewport = viewportRect;
            treeScroll.content = contentRect;

            for (int depth = 0; depth < TalentData.DepthCount; depth++)
            {
                IReadOnlyList<TalentDefinition> depthTalents = TalentData.GetTalentsInDepth(depth);
                if (depth > 0)
                {
                    CreateConnector(content.transform, TalentData.GetTalentsInDepth(depth - 1), depthTalents);
                }

                CreateDepthColumn(args, content.transform, depth, depthTalents);
            }
        }

        private static void CreateDetailPanel(HeroTraitViewBuildArgs args, HeroTraitViewRefs refs)
        {
            GameObject detailPanel = HudUiFactory.CreatePanel("HeroTraitDetail", refs.Content.transform, new Color(0.20f, 0.27f, 0.40f, 1f));
            HudUiFactory.AddLayoutElement(detailPanel, -1, 132);
            HorizontalLayoutGroup detailLayout = detailPanel.AddComponent<HorizontalLayoutGroup>();
            detailLayout.padding = new RectOffset(14, 14, 12, 12);
            detailLayout.spacing = 12;
            detailLayout.childControlWidth = true;
            detailLayout.childControlHeight = true;
            detailLayout.childForceExpandWidth = true;
            detailLayout.childForceExpandHeight = true;

            refs.DetailText = HudUiFactory.CreateText("HeroTraitDetailText", detailPanel.transform, 23, FontStyle.Bold, TextAnchor.MiddleLeft);
            refs.LevelUpButton = HudUiFactory.CreateButton("레벨업", detailPanel.transform, 24, new Color(0.54f, 0.78f, 0.22f, 1f));
            HudUiFactory.AddLayoutElement(refs.LevelUpButton.gameObject, 220, -1);
            HudUiFactory.ConfigureHoldRepeat(refs.LevelUpButton, args.OnLevelUp, args.CanLevelUp);
        }

        private static void CreateDepthColumn(
            HeroTraitViewBuildArgs args,
            Transform parent,
            int depth,
            IReadOnlyList<TalentDefinition> depthTalents)
        {
            GameObject column = new GameObject("HeroTraitDepth" + depth, typeof(RectTransform));
            column.transform.SetParent(parent, false);
            HudUiFactory.AddLayoutElement(column, 132, -1);

            Text depthLabel = HudUiFactory.CreateText("HeroTraitDepthLabel" + depth, column.transform, 14, FontStyle.Bold, TextAnchor.MiddleCenter);
            depthLabel.text = "D" + (depth + 1);
            depthLabel.color = new Color(0.78f, 0.86f, 1f, 1f);
            RectTransform depthLabelRect = depthLabel.GetComponent<RectTransform>();
            depthLabelRect.anchorMin = new Vector2(0f, 0.92f);
            depthLabelRect.anchorMax = new Vector2(1f, 1f);
            depthLabelRect.offsetMin = Vector2.zero;
            depthLabelRect.offsetMax = Vector2.zero;

            for (int i = 0; i < depthTalents.Count; i++)
            {
                TalentDefinition talent = depthTalents[i];
                Button node = HudUiFactory.CreateButton(string.Empty, column.transform, 15, new Color(0.22f, 0.28f, 0.38f, 1f));
                RectTransform nodeRect = node.GetComponent<RectTransform>();
                float laneY = GetLaneY(depthTalents.Count, i);
                nodeRect.anchorMin = new Vector2(0.5f, laneY);
                nodeRect.anchorMax = new Vector2(0.5f, laneY);
                nodeRect.pivot = new Vector2(0.5f, 0.5f);
                nodeRect.sizeDelta = new Vector2(112f, 78f);
                nodeRect.anchoredPosition = Vector2.zero;

                Text nodeText = node.GetComponentInChildren<Text>();
                if (nodeText != null)
                {
                    nodeText.resizeTextForBestFit = true;
                    nodeText.resizeTextMinSize = 10;
                    nodeText.resizeTextMaxSize = 15;
                    nodeText.lineSpacing = 0.88f;
                }

                string talentId = talent.Id;
                node.onClick.AddListener(() => args.OnTalentSelected?.Invoke(talentId));
                args.TalentButtons[talent.Id] = node;
                args.TalentButtonTexts[talent.Id] = nodeText;
            }
        }

        private static void CreateConnector(
            Transform parent,
            IReadOnlyList<TalentDefinition> previousDepth,
            IReadOnlyList<TalentDefinition> currentDepth)
        {
            GameObject connector = new GameObject("HeroTraitConnector", typeof(RectTransform));
            connector.transform.SetParent(parent, false);
            HudUiFactory.AddLayoutElement(connector, 44, -1);

            for (int currentIndex = 0; currentIndex < currentDepth.Count; currentIndex++)
            {
                TalentDefinition current = currentDepth[currentIndex];
                for (int prerequisiteIndex = 0; prerequisiteIndex < current.PrerequisiteIds.Count; prerequisiteIndex++)
                {
                    int previousIndex = FindDepthIndex(previousDepth, current.PrerequisiteIds[prerequisiteIndex]);
                    if (previousIndex < 0)
                    {
                        continue;
                    }

                    CreateConnectorLine(
                        connector.transform,
                        GetLaneY(previousDepth.Count, previousIndex),
                        GetLaneY(currentDepth.Count, currentIndex));
                }
            }
        }

        private static void CreateConnectorLine(Transform parent, float fromLaneY, float toLaneY)
        {
            const float width = 44f;
            const float height = 252f;
            Vector2 start = new Vector2(width * -0.5f, (fromLaneY - 0.5f) * height);
            Vector2 end = new Vector2(width * 0.5f, (toLaneY - 0.5f) * height);
            Vector2 delta = end - start;

            GameObject line = new GameObject("HeroTraitConnectorLine", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(parent, false);
            Image image = line.GetComponent<Image>();
            image.color = new Color(0.16f, 0.22f, 0.35f, 0.95f);
            image.raycastTarget = false;

            RectTransform rect = line.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(delta.magnitude, 5f);
            rect.anchoredPosition = (start + end) * 0.5f;
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private static int FindDepthIndex(IReadOnlyList<TalentDefinition> depthTalents, string talentId)
        {
            for (int i = 0; i < depthTalents.Count; i++)
            {
                if (depthTalents[i].Id == talentId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static float GetLaneY(int nodeCount, int nodeIndex)
        {
            if (nodeCount <= 1)
            {
                return 0.50f;
            }

            if (nodeCount == 2)
            {
                return nodeIndex == 0 ? 0.64f : 0.36f;
            }

            return 0.78f - Mathf.Clamp(nodeIndex, 0, 2) * 0.28f;
        }
    }
}
