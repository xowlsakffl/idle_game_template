using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace IdleGame.UI.Battle
{
    public static class DamageMeterView
    {
        public static void Apply(
            Text titleText,
            IReadOnlyList<DamageMeterRowViewState> rowStates,
            IList<GameObject> rowObjects,
            IList<Image> fillImages,
            IList<Text> rowTexts)
        {
            if (titleText != null)
            {
                titleText.text = "데미지 미터기";
            }

            if (rowObjects == null)
            {
                return;
            }

            for (int i = 0; i < rowObjects.Count; i++)
            {
                DamageMeterRowViewState state = rowStates != null && i < rowStates.Count
                    ? rowStates[i]
                    : null;
                bool active = state != null && state.Active;

                GameObject row = rowObjects[i];
                if (row != null)
                {
                    row.SetActive(active);
                }

                if (!active)
                {
                    continue;
                }

                if (fillImages != null && i < fillImages.Count && fillImages[i] != null)
                {
                    Image fill = fillImages[i];
                    fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(state.FillRatio), 1f);
                    fill.color = state.FillColor;
                }

                if (rowTexts != null && i < rowTexts.Count && rowTexts[i] != null)
                {
                    rowTexts[i].text = state.Text ?? string.Empty;
                }
            }
        }
    }
}
