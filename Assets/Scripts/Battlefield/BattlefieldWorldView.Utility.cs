using System.Collections.Generic;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Speed;

namespace IdleGame.Battlefield
{
    public sealed partial class BattlefieldWorldView
    {
        private static float GetPulseRatio(float remaining, float duration)
        {
            return Mathf.Clamp01(remaining / Mathf.Max(0.001f, duration));
        }

        private static float EaseOut(float value)
        {
            float t = Mathf.Clamp01(value);
            return 1f - (1f - t) * (1f - t);
        }

        private static Vector3 ToWorld(Vector2 localPosition)
        {
            return new Vector3(OriginX + localPosition.x, localPosition.y, 0f);
        }

        private static Vector2 ClampField(Vector2 position, float margin = 0.15f)
        {
            return new Vector2(
                Mathf.Clamp(position.x, -FieldHalfWidth + margin, FieldHalfWidth - margin),
                Mathf.Clamp(position.y, -FieldHalfHeight + margin, FieldHalfHeight - margin));
        }

        private static Color GetRarityColor(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Common:
                    return new Color(0.58f, 0.66f, 0.72f, 1f);
                case HeroRarity.Uncommon:
                    return new Color(0.34f, 0.84f, 0.30f, 1f);
                case HeroRarity.Rare:
                    return new Color(0.30f, 0.58f, 1f, 1f);
                case HeroRarity.Epic:
                    return new Color(0.72f, 0.28f, 0.96f, 1f);
                case HeroRarity.Legendary:
                    return new Color(1f, 0.62f, 0.16f, 1f);
                case HeroRarity.Mythic:
                    return new Color(1f, 0.18f, 0.18f, 1f);
                default:
                    return Color.white;
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static float PseudoRandom01(int seed)
        {
            unchecked
            {
                uint value = (uint)(seed * 747796405 + 2891336453);
                value = ((value >> ((int)(value >> 28) + 4)) ^ value) * 277803737;
                value = (value >> 22) ^ value;
                return (value & 0xFFFFFF) / 16777215f;
            }
        }
    }
}
