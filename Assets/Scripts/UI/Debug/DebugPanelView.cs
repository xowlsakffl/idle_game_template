using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.UI.Common;

namespace IdleGame.UI.Debugging
{
    public sealed class DebugPanelViewRefs
    {
        public Text StatusText;
    }

    public sealed class DebugPanelButtonDescriptor
    {
        public readonly string Label;
        public readonly Action Action;
        public readonly Color Color;
        public readonly bool RefreshAfter;

        public DebugPanelButtonDescriptor(string label, Action action)
            : this(label, action, new Color(0.23f, 0.25f, 0.28f, 1f), true)
        {
        }

        public DebugPanelButtonDescriptor(string label, Action action, Color color, bool refreshAfter = true)
        {
            Label = label;
            Action = action;
            Color = color;
            RefreshAfter = refreshAfter;
        }
    }

    public sealed class DebugPanelViewBuildArgs
    {
        public Transform Parent;
        public IReadOnlyList<DebugPanelButtonDescriptor> Buttons;
        public IReadOnlyList<DebugPanelButtonDescriptor> TimeButtons;
        public Action OnRefresh;
    }

    public static class DebugPanelView
    {
        public static DebugPanelViewRefs Build(DebugPanelViewBuildArgs args)
        {
            DebugPanelViewRefs refs = new DebugPanelViewRefs();
            if (args == null || args.Parent == null)
            {
                return refs;
            }

            ConfigurePanelLayout(args.Parent);

            Text title = HudUiFactory.CreateText("DebugTitle", args.Parent, 36, FontStyle.Bold, TextAnchor.MiddleLeft);
            title.text = "QA 디버그";
            HudUiFactory.AddLayoutElement(title.gameObject, -1, 58);

            GameObject gridObject = new GameObject("DebugGrid", typeof(RectTransform));
            gridObject.transform.SetParent(args.Parent, false);
            GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(300, 82);
            grid.spacing = new Vector2(16, 16);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            HudUiFactory.AddLayoutElement(gridObject, -1, 820);

            BuildButtons(gridObject.transform, args.Buttons, args.OnRefresh);

            GameObject speedRow = new GameObject("SpeedButtons", typeof(RectTransform));
            speedRow.transform.SetParent(args.Parent, false);
            HorizontalLayoutGroup row = speedRow.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 16;
            row.childControlWidth = true;
            row.childForceExpandWidth = true;
            HudUiFactory.AddLayoutElement(speedRow, -1, 90);

            BuildButtons(speedRow.transform, args.TimeButtons, args.OnRefresh);

            refs.StatusText = HudUiFactory.CreateText("DebugStatus", args.Parent, 26, FontStyle.Normal, TextAnchor.UpperLeft);
            HudUiFactory.AddLayoutElement(refs.StatusText.gameObject, -1, 260);
            return refs;
        }

        private static void ConfigurePanelLayout(Transform parent)
        {
            VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 22, 22);
            layout.spacing = 16;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static void BuildButtons(Transform parent, IReadOnlyList<DebugPanelButtonDescriptor> buttons, Action onRefresh)
        {
            if (buttons == null)
            {
                return;
            }

            foreach (DebugPanelButtonDescriptor button in buttons)
            {
                CreateDebugButton(parent, button, onRefresh);
            }
        }

        private static void CreateDebugButton(Transform parent, DebugPanelButtonDescriptor descriptor, Action onRefresh)
        {
            if (parent == null || descriptor == null)
            {
                return;
            }

            Button button = HudUiFactory.CreateButton(descriptor.Label, parent, 25, descriptor.Color);
            button.onClick.AddListener(() =>
            {
                descriptor.Action?.Invoke();
                if (descriptor.RefreshAfter)
                {
                    onRefresh?.Invoke();
                }
            });
        }
    }
}
