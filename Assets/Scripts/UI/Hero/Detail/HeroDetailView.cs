using System;
using UnityEngine;
using UnityEngine.UI;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Detail
{
    public static partial class HeroDetailView
    {
        public static HeroDetailViewRefs Build(HeroDetailViewBuildArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args.Parent == null)
            {
                throw new ArgumentException("Hero detail parent is required.", nameof(args));
            }

            var refs = new HeroDetailViewRefs();
            refs.Panel = HudUiFactory.CreatePanel("HeroDetailPanel", args.Parent, new Color(0.04f, 0.06f, 0.12f, 0.96f));
            LayoutElement overlayLayout = refs.Panel.AddComponent<LayoutElement>();
            overlayLayout.ignoreLayout = true;
            HudUiFactory.StretchToParent(refs.Panel);
            refs.Panel.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 130f);

            BuildHeader(args, refs);
            BuildEquipmentSlots(args, refs);
            BuildSummaryAndBasicInfo(args, refs);
            BuildHeroActionButtons(args, refs);
            BuildEquipmentContent(args, refs);
            BuildTranscendContent(args, refs);
            BuildBottomTabs(args, refs);
            BuildTranscendConfirmPrompt(args, refs);
            BuildEquipmentDetailPopup(args, refs);
            BuildEquipmentDismantlePopup(args, refs);

            refs.Panel.SetActive(false);
            return refs;
        }
    }
}
