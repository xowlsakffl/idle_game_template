using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class GameHud : MonoBehaviour
{
    private StageProgressManager progressManager;
    private CurrencyWallet wallet;
    private BattleManager battleManager;
    private GachaManager gachaManager;

    private Text goldText;
    private Text stageText;
    private Text modeText;
    private Text targetText;
    private Text progressText;
    private Text logText;
    private Text gachaText;
    private readonly Dictionary<string, Text> heroButtonTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, Button> stageButtons = new Dictionary<string, Button>();

    public void Initialize(
        StageProgressManager progress,
        CurrencyWallet currency,
        BattleManager battle,
        GachaManager gacha)
    {
        progressManager = progress;
        wallet = currency;
        battleManager = battle;
        gachaManager = gacha;

        CreateEventSystemIfNeeded();
        CreateHud();

        progressManager.Changed += UpdateView;
        wallet.Changed += UpdateView;
        battleManager.Changed += UpdateView;
        gachaManager.Changed += UpdateView;

        UpdateView();
    }

    private void CreateEventSystemIfNeeded()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private void CreateHud()
    {
        GameObject canvasObject = new GameObject("IdleGameCanvas", typeof(RectTransform));
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject root = CreatePanel("Root", canvasObject.transform, new Color(0.08f, 0.09f, 0.11f, 1f));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup rootLayout = root.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(36, 36, 36, 36);
        rootLayout.spacing = 18;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        CreateHeader(root.transform);
        CreateBattlePanel(root.transform);
        CreateHeroPanel(root.transform);
        CreateStagePanel(root.transform);
        CreateGachaPanel(root.transform);
    }

    private void CreateHeader(Transform parent)
    {
        GameObject panel = CreatePanel("Header", parent, new Color(0.13f, 0.15f, 0.18f, 1f));
        AddLayoutElement(panel, -1, 170);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 18, 18);
        layout.spacing = 6;

        goldText = CreateText("Gold", panel.transform, 36, FontStyle.Bold, TextAnchor.MiddleLeft);
        stageText = CreateText("Stage", panel.transform, 30, FontStyle.Normal, TextAnchor.MiddleLeft);
        modeText = CreateText("Mode", panel.transform, 26, FontStyle.Normal, TextAnchor.MiddleLeft);
    }

    private void CreateBattlePanel(Transform parent)
    {
        GameObject panel = CreatePanel("Battle", parent, new Color(0.11f, 0.12f, 0.15f, 1f));
        AddLayoutElement(panel, -1, 330);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 28, 28);
        layout.spacing = 16;
        layout.childAlignment = TextAnchor.MiddleCenter;

        targetText = CreateText("Target", panel.transform, 38, FontStyle.Bold, TextAnchor.MiddleCenter);
        progressText = CreateText("Progress", panel.transform, 28, FontStyle.Normal, TextAnchor.MiddleCenter);
        logText = CreateText("Log", panel.transform, 24, FontStyle.Normal, TextAnchor.MiddleCenter);
    }

    private void CreateHeroPanel(Transform parent)
    {
        GameObject panel = CreatePanel("Heroes", parent, new Color(0.13f, 0.15f, 0.18f, 1f));
        AddLayoutElement(panel, -1, 310);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 18, 18);
        layout.spacing = 12;

        foreach (HeroDefinition hero in GameData.Heroes)
        {
            Button button = CreateButton(hero.DisplayName, panel.transform, 30, new Color(0.24f, 0.28f, 0.33f, 1f));
            AddLayoutElement(button.gameObject, -1, 82);

            string heroId = hero.Id;
            button.onClick.AddListener(() => battleManager.TryLevelUpHero(heroId));
            heroButtonTexts[hero.Id] = button.GetComponentInChildren<Text>();
        }
    }

    private void CreateStagePanel(Transform parent)
    {
        GameObject panel = CreatePanel("Stages", parent, new Color(0.11f, 0.12f, 0.15f, 1f));
        AddLayoutElement(panel, -1, 500);

        VerticalLayoutGroup vertical = panel.AddComponent<VerticalLayoutGroup>();
        vertical.padding = new RectOffset(18, 18, 18, 18);
        vertical.spacing = 14;
        vertical.childControlWidth = true;
        vertical.childControlHeight = true;

        Button resumeButton = CreateButton("자동 진행 재개", panel.transform, 28, new Color(0.12f, 0.34f, 0.30f, 1f));
        AddLayoutElement(resumeButton.gameObject, -1, 76);
        resumeButton.onClick.AddListener(() => progressManager.ResumeAutoProgress());

        GameObject gridObject = new GameObject("StageGrid", typeof(RectTransform));
        gridObject.transform.SetParent(panel.transform, false);
        GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(235, 64);
        grid.spacing = new Vector2(12, 12);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        AddLayoutElement(gridObject, -1, 370);

        foreach (StageDefinition stage in GameData.Stages)
        {
            Button button = CreateButton(stage.Id, gridObject.transform, 24, new Color(0.20f, 0.23f, 0.27f, 1f));
            string stageId = stage.Id;
            button.onClick.AddListener(() => progressManager.SelectStage(stageId));
            stageButtons[stage.Id] = button;
        }
    }

    private void CreateGachaPanel(Transform parent)
    {
        GameObject panel = CreatePanel("Gacha", parent, new Color(0.13f, 0.15f, 0.18f, 1f));
        AddLayoutElement(panel, -1, 300);

        VerticalLayoutGroup vertical = panel.AddComponent<VerticalLayoutGroup>();
        vertical.padding = new RectOffset(18, 18, 18, 18);
        vertical.spacing = 12;

        GameObject buttonRow = new GameObject("GachaButtons", typeof(RectTransform));
        buttonRow.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup row = buttonRow.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 12;
        row.childControlWidth = true;
        row.childForceExpandWidth = true;
        AddLayoutElement(buttonRow, -1, 72);

        Button rollOne = CreateButton("1회 뽑기", buttonRow.transform, 26, new Color(0.30f, 0.22f, 0.35f, 1f));
        Button rollTen = CreateButton("10회 뽑기", buttonRow.transform, 26, new Color(0.30f, 0.22f, 0.35f, 1f));
        rollOne.onClick.AddListener(() => gachaManager.Roll(1));
        rollTen.onClick.AddListener(() => gachaManager.Roll(10));

        gachaText = CreateText("GachaResult", panel.transform, 22, FontStyle.Normal, TextAnchor.UpperLeft);
        AddLayoutElement(gachaText.gameObject, -1, 180);
    }

    private void UpdateView()
    {
        if (goldText == null)
        {
            return;
        }

        StageDefinition stage = progressManager.CurrentStage;
        goldText.text = "Gold: " + wallet.Gold;
        stageText.text = "Stage: " + stage.Id + " / Highest: " + progressManager.HighestStageId;
        modeText.text = "Mode: " + GetModeLabel(progressManager.Mode);

        targetText.text = battleManager.TargetName + " HP " + battleManager.TargetHp + " / " + battleManager.TargetMaxHp;
        if (battleManager.IsBossFight)
        {
            progressText.text = "Boss Timer: " + Mathf.CeilToInt(battleManager.BossTimeRemaining) + "s";
        }
        else
        {
            progressText.text = "Kills: " + battleManager.KillsThisStage + " / " + battleManager.RequiredKills;
        }

        logText.text = battleManager.LastBattleLog;
        gachaText.text = gachaManager.LastResult;

        foreach (HeroState hero in battleManager.Heroes)
        {
            if (heroButtonTexts.TryGetValue(hero.Definition.Id, out Text text))
            {
                text.text = hero.Definition.DisplayName
                    + " Lv." + hero.Level
                    + " / ATK " + hero.AttackPower
                    + " / Cost " + hero.LevelUpCost
                    + " / Shard " + hero.Shards;
            }
        }

        foreach (KeyValuePair<string, Button> pair in stageButtons)
        {
            bool unlocked = GameData.IsStageUnlocked(pair.Key, progressManager.HighestStageId);
            pair.Value.interactable = unlocked;
            Text text = pair.Value.GetComponentInChildren<Text>();
            text.text = pair.Key == progressManager.CurrentStageId ? "[" + pair.Key + "]" : pair.Key;
        }
    }

    private string GetModeLabel(ProgressMode mode)
    {
        switch (mode)
        {
            case ProgressMode.AutoProgress:
                return "자동 진행";
            case ProgressMode.RepeatSelected:
                return "선택 반복";
            case ProgressMode.BossBlocked:
                return "보스 막힘";
            default:
                return mode.ToString();
        }
    }

    private GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private Button CreateButton(string label, Transform parent, int fontSize, Color color)
    {
        GameObject buttonObject = CreatePanel(label + "Button", parent, color);
        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.15f;
        colors.pressedColor = color * 0.85f;
        colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.6f);
        button.colors = colors;

        Text text = CreateText(label + "Text", buttonObject.transform, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8, 4);
        textRect.offsetMax = new Vector2(-8, -4);
        text.text = label;

        return button;
    }

    private Text CreateText(string name, Transform parent, int fontSize, FontStyle fontStyle, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
        {
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, fontSize + 18);
        return text;
    }

    private void AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
    {
        LayoutElement element = target.AddComponent<LayoutElement>();
        if (preferredWidth > 0)
        {
            element.preferredWidth = preferredWidth;
        }

        if (preferredHeight > 0)
        {
            element.preferredHeight = preferredHeight;
        }
    }
}
