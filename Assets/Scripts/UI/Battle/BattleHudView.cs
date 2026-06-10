using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battlefield;
using IdleGame.Data;
using IdleGame.UI.Common;

namespace IdleGame.UI.Battle
{
    public sealed class BattleHudViewRefs
    {
        public RectTransform BattlefieldRect;
        public RawImage BattlefieldWorldImage;
        public Text CenterSpawnText;
        public Text FieldStagePillText;
        public Text DamagePopupText;
        public Text DamageMeterText;
        public Text GuideQuestText;
        public GameObject GuideQuestDot;
    }

    public sealed class BattleHudViewBuildArgs
    {
        public Transform Parent;
        public BattlefieldWorldView BattlefieldWorldView;
        public BattleHudVisualState VisualState;
        public Dictionary<string, Image> HeroBattleImages;
        public Dictionary<string, Text> HeroBattleTexts;
        public Dictionary<string, RectTransform> HeroBattleRects;
        public List<Image> EnemyBattleImages;
        public List<Text> EnemyBattleTexts;
        public List<RectTransform> EnemyBattleRects;
        public List<GameObject> EnemyHpBarObjects;
        public List<Image> EnemyHpFillImages;
        public List<GameObject> DamageMeterRows;
        public List<Image> DamageMeterFills;
        public List<Text> DamageMeterRowTexts;
    }

    public static class BattleHudView
    {
        public static BattleHudViewRefs Build(BattleHudViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new BattleHudViewRefs();
            }

            BattleHudViewRefs refs = new BattleHudViewRefs();
            GameObject field = HudUiFactory.CreatePanel("Battlefield", args.Parent, new Color(0.18f, 0.20f, 0.24f, 1f));
            HudUiFactory.StretchToParent(field);
            refs.BattlefieldRect = field.GetComponent<RectTransform>();

            if (args.BattlefieldWorldView != null && args.BattlefieldWorldView.OutputTexture != null)
            {
                GameObject worldRender = new GameObject("BattlefieldWorldRender", typeof(RectTransform), typeof(RawImage));
                worldRender.transform.SetParent(field.transform, false);
                HudUiFactory.StretchToParent(worldRender);
                refs.BattlefieldWorldImage = worldRender.GetComponent<RawImage>();
                refs.BattlefieldWorldImage.texture = args.BattlefieldWorldView.OutputTexture;
                refs.BattlefieldWorldImage.color = Color.white;
                refs.BattlefieldWorldImage.raycastTarget = false;
            }

            refs.CenterSpawnText = HudUiFactory.CreateText("SpawnPortal", field.transform, 82, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform portalRect = refs.CenterSpawnText.GetComponent<RectTransform>();
            portalRect.anchorMin = new Vector2(0.5f, 0.5f);
            portalRect.anchorMax = new Vector2(0.5f, 0.5f);
            portalRect.pivot = new Vector2(0.5f, 0.5f);
            portalRect.sizeDelta = new Vector2(120f, 120f);
            portalRect.anchoredPosition = Vector2.zero;
            refs.CenterSpawnText.text = "◎";
            refs.CenterSpawnText.color = new Color(0.95f, 0.12f, 0.10f, 0.85f);

            GameObject stagePill = HudUiFactory.CreatePanel("FieldStagePill", field.transform, Color.white);
            HudUiFactory.ApplySprite(stagePill.GetComponent<Image>(), HudSpriteKind.BlueRibbon, new Color(0.92f, 1f, 1f, 1f));
            RectTransform pillRect = stagePill.GetComponent<RectTransform>();
            pillRect.anchorMin = new Vector2(0.5f, 1f);
            pillRect.anchorMax = new Vector2(0.5f, 1f);
            pillRect.pivot = new Vector2(0.5f, 1f);
            pillRect.sizeDelta = new Vector2(170f, 44f);
            pillRect.anchoredPosition = new Vector2(0f, -8f);
            refs.FieldStagePillText = HudUiFactory.CreateText("FieldStagePillText", stagePill.transform, 21, FontStyle.Bold, TextAnchor.MiddleCenter);
            refs.FieldStagePillText.color = Color.white;
            HudUiFactory.StretchToParent(refs.FieldStagePillText.gameObject);

            refs.DamagePopupText = HudUiFactory.CreateText("DamagePopup", field.transform, 24, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform damageRect = refs.DamagePopupText.GetComponent<RectTransform>();
            damageRect.anchorMin = new Vector2(0.5f, 0.5f);
            damageRect.anchorMax = new Vector2(0.5f, 0.5f);
            damageRect.pivot = new Vector2(0.5f, 0.5f);
            damageRect.sizeDelta = new Vector2(260f, 92f);
            damageRect.anchoredPosition = new Vector2(0f, 18f);

            GameObject damageMeter = HudUiFactory.CreatePanel("DamageMeter", field.transform, Color.white);
            HudUiFactory.ApplySprite(damageMeter.GetComponent<Image>(), HudSpriteKind.CarvedPanel, Color.white);
            RectTransform damageMeterRect = damageMeter.GetComponent<RectTransform>();
            damageMeterRect.anchorMin = new Vector2(1f, 0f);
            damageMeterRect.anchorMax = new Vector2(1f, 0f);
            damageMeterRect.pivot = new Vector2(1f, 0f);
            damageMeterRect.sizeDelta = new Vector2(230f, 206f);
            damageMeterRect.anchoredPosition = new Vector2(-12f, 244f);
            refs.DamageMeterText = HudUiFactory.CreateText("DamageMeterTitle", damageMeter.transform, 17, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform damageMeterTextRect = refs.DamageMeterText.GetComponent<RectTransform>();
            damageMeterTextRect.anchorMin = new Vector2(0f, 1f);
            damageMeterTextRect.anchorMax = new Vector2(1f, 1f);
            damageMeterTextRect.pivot = new Vector2(0.5f, 1f);
            damageMeterTextRect.sizeDelta = new Vector2(-20f, 28f);
            damageMeterTextRect.anchoredPosition = new Vector2(0f, -8f);
            refs.DamageMeterText.text = "데미지 미터기";

            GameObject meterRows = new GameObject("DamageMeterRows", typeof(RectTransform));
            meterRows.transform.SetParent(damageMeter.transform, false);
            RectTransform rowsRect = meterRows.GetComponent<RectTransform>();
            rowsRect.anchorMin = new Vector2(0f, 0f);
            rowsRect.anchorMax = new Vector2(1f, 1f);
            rowsRect.offsetMin = new Vector2(10f, 8f);
            rowsRect.offsetMax = new Vector2(-10f, -40f);
            VerticalLayoutGroup meterLayout = meterRows.AddComponent<VerticalLayoutGroup>();
            meterLayout.spacing = 4;
            meterLayout.childControlWidth = true;
            meterLayout.childControlHeight = true;
            meterLayout.childForceExpandWidth = true;
            meterLayout.childForceExpandHeight = true;

            for (int i = 0; i < GameData.MaxPartyHeroes; i++)
            {
                CreateDamageMeterRow(args, meterRows.transform, i);
            }

            GameObject guideQuest = HudUiFactory.CreatePanel("GuideQuestCard", field.transform, Color.white);
            HudUiFactory.ApplySprite(guideQuest.GetComponent<Image>(), HudSpriteKind.ParchmentPanel, Color.white);
            RectTransform guideRect = guideQuest.GetComponent<RectTransform>();
            guideRect.anchorMin = new Vector2(1f, 1f);
            guideRect.anchorMax = new Vector2(1f, 1f);
            guideRect.pivot = new Vector2(1f, 1f);
            guideRect.sizeDelta = new Vector2(310f, 86f);
            guideRect.anchoredPosition = new Vector2(-12f, -80f);
            refs.GuideQuestText = HudUiFactory.CreateText("GuideQuestText", guideQuest.transform, 21, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform guideTextRect = refs.GuideQuestText.GetComponent<RectTransform>();
            guideTextRect.anchorMin = Vector2.zero;
            guideTextRect.anchorMax = Vector2.one;
            guideTextRect.offsetMin = new Vector2(18f, 8f);
            guideTextRect.offsetMax = new Vector2(-18f, -8f);
            refs.GuideQuestDot = HudUiFactory.CreateNotificationDot(guideQuest.transform, 34f, new Vector2(-12f, -12f));

            CreateHeroActors(args, field.transform);
            CreateEnemyActors(args, field.transform);

            stagePill.transform.SetAsLastSibling();
            guideQuest.transform.SetAsLastSibling();
            damageMeter.transform.SetAsLastSibling();
            refs.DamagePopupText.transform.SetAsLastSibling();
            return refs;
        }

        private static void CreateHeroActors(BattleHudViewBuildArgs args, Transform field)
        {
            foreach (HeroDefinition hero in GameData.Heroes)
            {
                GameObject actor = CreateBattleActor(hero.Id + "HeroActor", field, new Vector2(74f, 74f), new Color(0.16f, 0.24f, 0.34f, 1f));
                Image image = actor.GetComponent<Image>();
                Text label = HudUiFactory.CreateText(hero.Id + "BattleLabel", actor.transform, 19, FontStyle.Bold, TextAnchor.MiddleCenter);
                HudUiFactory.StretchToParent(label.gameObject);
                label.text = string.Empty;
                args.HeroBattleRects[hero.Id] = actor.GetComponent<RectTransform>();
                args.HeroBattleImages[hero.Id] = image;
                args.HeroBattleTexts[hero.Id] = label;
            }
        }

        private static void CreateEnemyActors(BattleHudViewBuildArgs args, Transform field)
        {
            for (int i = 0; i < GameData.MaxVisibleEnemies; i++)
            {
                GameObject enemy = CreateBattleActor("EnemyActor" + i, field, new Vector2(58f, 58f), new Color(0.56f, 0.13f, 0.11f, 1f));
                Image image = enemy.GetComponent<Image>();
                Text label = HudUiFactory.CreateText("Enemy" + i + "Text", enemy.transform, 18, FontStyle.Bold, TextAnchor.MiddleCenter);
                HudUiFactory.StretchToParent(label.gameObject);
                label.text = string.Empty;

                GameObject enemyHpBar = HudUiFactory.CreatePanel("EnemyHpBar" + i, enemy.transform, Color.white);
                HudUiFactory.ApplySprite(enemyHpBar.GetComponent<Image>(), HudSpriteKind.SmallBarBase, new Color(0.78f, 0.82f, 0.86f, 1f));
                RectTransform enemyHpRect = enemyHpBar.GetComponent<RectTransform>();
                enemyHpRect.anchorMin = new Vector2(0.5f, 1f);
                enemyHpRect.anchorMax = new Vector2(0.5f, 1f);
                enemyHpRect.pivot = new Vector2(0.5f, 0f);
                enemyHpRect.sizeDelta = new Vector2(62f, 10f);
                enemyHpRect.anchoredPosition = new Vector2(0f, 4f);

                Image enemyHpFill = HudUiFactory.CreateBarFill("EnemyHpFill" + i, enemyHpBar.transform, HudSpriteKind.SmallBarFill, new Color(0.95f, 0.20f, 0.16f, 1f));
                RectTransform enemyHpFillRect = enemyHpFill.GetComponent<RectTransform>();
                enemyHpFillRect.offsetMin = new Vector2(0f, 2f);
                enemyHpFillRect.offsetMax = new Vector2(0f, -2f);

                args.EnemyBattleRects.Add(enemy.GetComponent<RectTransform>());
                args.EnemyBattleImages.Add(image);
                args.EnemyBattleTexts.Add(label);
                args.EnemyHpBarObjects.Add(enemyHpBar);
                args.EnemyHpFillImages.Add(enemyHpFill);
                args.VisualState?.EnsureEnemyCapacity(i);
            }
        }

        private static void CreateDamageMeterRow(BattleHudViewBuildArgs args, Transform parent, int index)
        {
            GameObject row = HudUiFactory.CreatePanel("DamageMeterRow" + index, parent, Color.white);
            HudUiFactory.ApplySprite(row.GetComponent<Image>(), HudSpriteKind.SmallBarBase, new Color(0.92f, 0.96f, 1f, 0.95f));
            Image fill = HudUiFactory.CreateBarFill("DamageMeterFill" + index, row.transform, HudSpriteKind.SmallBarFill, new Color(0.95f, 0.20f, 0.16f, 1f));
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.offsetMin = new Vector2(0f, 2f);
            fillRect.offsetMax = new Vector2(0f, -2f);

            Text text = HudUiFactory.CreateText("DamageMeterRowText" + index, row.transform, 13, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 0f);
            textRect.offsetMax = new Vector2(-8f, 0f);
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 9;
            text.resizeTextMaxSize = 13;

            args.DamageMeterRows.Add(row);
            args.DamageMeterFills.Add(fill);
            args.DamageMeterRowTexts.Add(text);
        }

        private static GameObject CreateBattleActor(string name, Transform parent, Vector2 size, Color color)
        {
            GameObject actor = HudUiFactory.CreatePanel(name, parent, color);
            Image image = actor.GetComponent<Image>();
            image.raycastTarget = false;

            RectTransform rect = actor.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return actor;
        }
    }
}
