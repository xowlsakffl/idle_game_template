using UnityEngine;

namespace IdleGame.UI.Common
{
    public static class HudSpriteFactory
    {
        private static Sprite circleSprite;
        private static Sprite ringSprite;
        private static Sprite coinIconSprite;
        private static Sprite gemIconSprite;
        private static Sprite powerIconSprite;

        public static Sprite GetCircleSprite()
        {
            if (circleSprite == null)
            {
                circleSprite = CreateCircleSprite(96, 0f);
            }

            return circleSprite;
        }

        public static Sprite GetRingSprite()
        {
            if (ringSprite == null)
            {
                ringSprite = CreateCircleSprite(128, 0.72f);
            }

            return ringSprite;
        }

        public static Sprite GetCoinIconSprite()
        {
            if (coinIconSprite == null)
            {
                coinIconSprite = CreateCoinIconSprite(64);
            }

            return coinIconSprite;
        }

        public static Sprite GetGemIconSprite()
        {
            if (gemIconSprite == null)
            {
                gemIconSprite = CreateGemIconSprite(64);
            }

            return gemIconSprite;
        }

        public static Sprite GetPowerIconSprite()
        {
            if (powerIconSprite == null)
            {
                powerIconSprite = CreatePowerIconSprite(64);
            }

            return powerIconSprite;
        }

        private static Sprite CreateCircleSprite(int size, float innerCutoutRatio)
        {
            Texture2D texture = CreateTransparentTexture(size);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outerRadius = size * 0.47f;
            float innerRadius = outerRadius * Mathf.Clamp01(innerCutoutRatio);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float outerAlpha = Mathf.Clamp01(outerRadius + 0.8f - distance);
                    float innerAlpha = innerRadius > 0.01f ? Mathf.Clamp01(distance - innerRadius + 0.8f) : 1f;
                    float alpha = Mathf.Clamp01(outerAlpha * innerAlpha);
                    if (alpha > 0f)
                    {
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
            }

            return CreateSpriteFromTexture(texture);
        }

        private static Sprite CreateCoinIconSprite(int size)
        {
            Texture2D texture = CreateTransparentTexture(size);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.44f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x, y);
                    float distance = Vector2.Distance(point, center);
                    if (distance > radius)
                    {
                        continue;
                    }

                    float vertical = Mathf.InverseLerp(0f, size, y);
                    Color color = Color.Lerp(new Color(0.88f, 0.42f, 0.04f, 1f), new Color(1f, 0.92f, 0.20f, 1f), vertical);
                    if (distance > radius - 5f)
                    {
                        color = new Color(0.54f, 0.25f, 0.02f, 1f);
                    }
                    else if (Mathf.Abs(distance - radius * 0.62f) < 2.1f)
                    {
                        color = new Color(1f, 0.98f, 0.48f, 1f);
                    }

                    Vector2 highlightCenter = center + new Vector2(-radius * 0.25f, radius * 0.25f);
                    if (Vector2.Distance(point, highlightCenter) < radius * 0.24f)
                    {
                        color = Color.Lerp(color, Color.white, 0.45f);
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            return CreateSpriteFromTexture(texture);
        }

        private static Sprite CreateGemIconSprite(int size)
        {
            Texture2D texture = CreateTransparentTexture(size);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.43f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - center.x) / radius;
                    float ny = (y - center.y) / radius;
                    float diamond = Mathf.Abs(nx) + Mathf.Abs(ny);
                    if (diamond > 1f)
                    {
                        continue;
                    }

                    Color color = ny > 0.10f
                        ? new Color(0.46f, 1f, 0.76f, 1f)
                        : new Color(0.10f, 0.72f, 0.96f, 1f);
                    if (nx > 0.18f)
                    {
                        color = Color.Lerp(color, new Color(0.03f, 0.36f, 0.86f, 1f), 0.45f);
                    }
                    else if (nx < -0.18f)
                    {
                        color = Color.Lerp(color, Color.white, 0.18f);
                    }

                    if (diamond > 0.88f || Mathf.Abs(nx) < 0.025f || Mathf.Abs(nx + ny) < 0.025f || Mathf.Abs(nx - ny) < 0.025f)
                    {
                        color = Color.Lerp(color, new Color(0.02f, 0.20f, 0.50f, 1f), diamond > 0.88f ? 0.75f : 0.30f);
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            return CreateSpriteFromTexture(texture);
        }

        private static Sprite CreatePowerIconSprite(int size)
        {
            Texture2D texture = CreateTransparentTexture(size);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.44f;
            Vector2 bladeStart = new Vector2(size * 0.30f, size * 0.25f);
            Vector2 bladeEnd = new Vector2(size * 0.70f, size * 0.74f);
            Vector2 guardStart = new Vector2(size * 0.26f, size * 0.34f);
            Vector2 guardEnd = new Vector2(size * 0.42f, size * 0.20f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x, y);
                    float distance = Vector2.Distance(point, center);
                    if (distance <= radius)
                    {
                        Color color = distance > radius - 4f
                            ? new Color(0.02f, 0.08f, 0.18f, 1f)
                            : Color.Lerp(new Color(0.10f, 0.32f, 0.54f, 1f), new Color(0.20f, 0.68f, 0.95f, 1f), Mathf.InverseLerp(0f, size, y));
                        texture.SetPixel(x, y, color);
                    }

                    float bladeDistance = DistanceToSegment(point, bladeStart, bladeEnd);
                    float guardDistance = DistanceToSegment(point, guardStart, guardEnd);
                    if (bladeDistance < 3.2f || guardDistance < 3.0f)
                    {
                        texture.SetPixel(x, y, bladeDistance < 1.5f ? Color.white : new Color(0.82f, 0.90f, 0.98f, 1f));
                    }

                    if (Vector2.Distance(point, bladeEnd) < 5f)
                    {
                        texture.SetPixel(x, y, new Color(1f, 0.92f, 0.42f, 1f));
                    }
                }
            }

            return CreateSpriteFromTexture(texture);
        }

        private static Texture2D CreateTransparentTexture(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            return texture;
        }

        private static Sprite CreateSpriteFromTexture(Texture2D texture)
        {
            texture.Apply(false, true);
            texture.hideFlags = HideFlags.HideAndDontSave;
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 segment = end - start;
            float segmentLengthSquared = segment.sqrMagnitude;
            if (segmentLengthSquared <= 0.001f)
            {
                return Vector2.Distance(point, start);
            }

            float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / segmentLengthSquared);
            return Vector2.Distance(point, start + segment * t);
        }
    }
}
