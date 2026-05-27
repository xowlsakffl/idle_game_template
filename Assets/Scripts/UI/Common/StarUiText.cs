using UnityEngine;
using IdleGame.Data;

namespace IdleGame.UI.Common
{
    public static class StarUiText
    {
        public static string FormatStars(int stars)
        {
            int clampedStars = Mathf.Clamp(stars, 0, HeroDefinition.MaxStars);
            if (clampedStars <= 0)
            {
                return "<color=#6F778A>★★★★★</color>";
            }

            int completedLayers = (clampedStars - 1) / 5;
            int starsInCurrentLayer = ((clampedStars - 1) % 5) + 1;
            string baseColor = completedLayers == 0 ? "#6F778A" : GetStarLayerColor(completedLayers - 1);
            string currentColor = GetStarLayerColor(completedLayers);
            string result = string.Empty;
            for (int i = 1; i <= 5; i++)
            {
                string color = i <= starsInCurrentLayer ? currentColor : baseColor;
                result += "<color=" + color + ">★</color>";
            }

            return result;
        }

        private static string GetStarLayerColor(int layer)
        {
            switch (layer)
            {
                case 0:
                    return "#FFD84D";
                case 1:
                    return "#51A7FF";
                default:
                    return "#C15CFF";
            }
        }
    }
}
