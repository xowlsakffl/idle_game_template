using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class GameHud : MonoBehaviour
{
    private enum HudTab
    {
        Growth,
        Stage,
        Summon
    }

    private StageProgressManager progressManager;
    private CurrencyWallet wallet;
    private AbilityManager abilityManager;
    private BattleManager battleManager;
    private GachaManager gachaManager;

    private HudTab activeTab = HudTab.Growth;

    private Text resourceText;
    private Text stageText;
    private Text modeText;
    private Text targetText;
    private Text hpText;
    private Text progressText;
    private Text logText;
    private Image hpFill;

    private GameObject growthPanel;
    private GameObject stagePanel;
    private GameObject summonPanel;
    private Text gachaText;

    private readonly Dictionary<AbilityKind, Text> abilityButtonTexts = new Dictionary<AbilityKind, Text>();
    private readonly Dictionary<string, Text> heroButtonTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, Button> stageButtons = new Dictionary<string, Button>();
    private readonly Dictionary<HudTab, Button> tabButtons = new Dictionary<HudTab, Button>();

    public void Initialize(
        StageProgressManager progress,
        CurrencyWallet currency,
        AbilityManager abilities,
        BattleManager battle,
        GachaManager gacha)
    {
        progressManager = progress;
        wallet = currency;
        abilityManager = abilities;
        battleManager = battle;
        gachaManager = gacha;

        CreateEventSystemIfNeeded();
        CreateHud();

        progressManager.Changed += UpdateView;
        wallet.Changed += UpdateView;
        abilityManager.Changed += UpdateView;
        battleManager.Changed += UpdateView;
        gachaManager.Changed += UpdateView;

        UpdateView();
    }

    private void CreateEventSystemIfNeeded()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();
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

        GameObject root = CreatePanel("Root", canvasObject.transform, new Color(0.07f, 0.08f, 0.10f, 1f));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup rootLayout = root.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(24, 24, 24, 24);
        rootLayout.spacing = 14;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        CreateHeader(root.transform);
        CreateBattlePanel(root.transform);
        CreateContentPanels(root.transform);
        CreateBottomNav(root.transform);
    }

    private void CreateHeader(Transform parent)
    {
        GameObject panel = CreatePanel("Header", parent, new Color(0.12f, 0.16f, 0.22f, 1f));
        AddLayoutElement(panel, -1, 190);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 18, 18);
        layout.spacing = 8;

        resourceText = CreateText("Resources", panel.transform, 34, FontStyle.Bold, TextAnchor.MiddleLeft);
        AddLayoutElement(resourceText.gameObject, -1, 48);

        stageText = CreateText("Stage", panel.transform, 32, FontStyle.Bold, TextAnchor.MiddleLeft);
        AddLayoutElement(stageText.gameObject, -1, 44);

        modeText = CreateText("Mode", panel.transform, 25, FontStyle.Normal, TextAnchor.MiddleLeft);
        AddLayoutElement(modeText.gameObject, -1, 38);
    }

    private void CreateBattlePanel(Transform parent)
    {
        GameObject panel = CreatePanel("Battle", parent, new Color(0.10f, 0.11f, 0.14f, 1f));
        AddLayoutElement(panel, -1, 500);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(30, 30, 28, 28);
        layout.spacing = 18;
        layout.childAlignment = TextAnchor.MiddleCenter;

        targetText = CreateText("Target", panel.transform, 44, FontStyle.Bold, TextAnchor.MiddleCenter);
        AddLayoutElement(targetText.gameObject, -1, 58);

        GameObject hpBar = CreatePanel("HpBar", panel.transform, new Color(0.03f, 0.04f, 0.05f, 1f));
        AddLayoutElement(hpBar, -1, 54);
        hpFill = CreatePanel("HpFill", hpBar.transform, new Color(0.86f, 0.18f, 0.16f, 1f)).GetComponent<Image>();
        RectTransform fillRect = hpFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        hpText = CreateText("HpText", hpBar.transform, 28, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform hpTextRect = hpText.GetComponent<RectTransform>();
        hpTextRect.anchorMin = Vector2.zero;
        hpTextRect.anchorMax = Vector2.one;
        hpTextRect.offsetMin = Vector2.zero;
        hpTextRect.offsetMax = Vector2.zero;

        progressText = CreateText("Progress", panel.transform, 34, FontStyle.Bold, TextAnchor.MiddleCenter);
        AddLayoutElement(progressText.gameObject, -1, 54);

        logText = CreateText("Log", panel.transform, 26, FontStyle.Normal, TextAnchor.MiddleCenter);
        AddLayoutElement(logText.gameObject, -1, 92);
    }

    private void CreateContentPanels(Transform parent)
    {
        GameObject contentRoot = CreatePanel("Content", parent, new Color(0.09f, 0.10f, 0.13f, 1f));
        AddLayoutElement(contentRoot, -1, 1000);

        RectTransform contentRect = contentRoot.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);

        growthPanel = CreatePanel("GrowthPanel", contentRoot.transform, new Color(0.13f, 0.16f, 0.22f, 1f));
        stagePanel = CreatePanel("StagePanel", contentRoot.transform, new Color(0.12f, 0.15f, 0.19f, 1f));
        summonPanel = CreatePanel("SummonPanel", contentRoot.transform, new Color(0.15f, 0.13f, 0.19f, 1f));

        StretchToParent(growthPanel);
        StretchToParent(stagePanel);
        StretchToParent(summonPanel);

        CreateGrowthPanel(growthPanel.transform);
        CreateStagePanel(stagePanel.transform);
        CreateSummonPanel(summonPanel.transform);
    }

    private void CreateGrowthPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("GrowthTitle", parent, 36, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = "성장 - 골드 능력치 / EXP 히어로 레벨업";
        AddLayoutElement(title.gameObject, -1, 58);

        foreach (AbilityState ability in abilityManager.States)
        {
            Button button = CreateButton(ability.Definition.DisplayName, parent, 27, new Color(0.31f, 0.29f, 0.20f, 1f));
            AddLayoutElement(button.gameObject, -1, 104);

            AbilityKind kind = ability.Definition.Kind;
            button.onClick.AddListener(() => abilityManager.TryLevelUp(kind));
            abilityButtonTexts[kind] = button.GetComponentInChildren<Text>();
        }

        Text heroTitle = CreateText("HeroGrowthTitle", parent, 30, FontStyle.Bold, TextAnchor.MiddleLeft);
        heroTitle.text = "히어로";
        AddLayoutElement(heroTitle.gameObject, -1, 46);

        foreach (HeroDefinition hero in GameData.Heroes)
        {
            Button button = CreateButton(hero.DisplayName, parent, 28, new Color(0.23f, 0.29f, 0.37f, 1f));
            AddLayoutElement(button.gameObject, -1, 106);

            string heroId = hero.Id;
            button.onClick.AddListener(() => battleManager.TryLevelUpHero(heroId));
            heroButtonTexts[hero.Id] = button.GetComponentInChildren<Text>();
        }
    }

    private void CreateStagePanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Button resumeButton = CreateButton("자동 진행 재개", parent, 30, new Color(0.12f, 0.34f, 0.30f, 1f));
        AddLayoutElement(resumeButton.gameObject, -1, 86);
        resumeButton.onClick.AddListener(() => progressManager.ResumeAutoProgress());

        GameObject gridObject = new GameObject("StageGrid", typeof(RectTransform));
        gridObject.transform.SetParent(parent, false);
        GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(228, 72);
        grid.spacing = new Vector2(14, 14);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        AddLayoutElement(gridObject, -1, 430);

        foreach (StageDefinition stage in GameData.Stages)
        {
            Color buttonColor = stage.Type == StageType.Boss
                ? new Color(0.42f, 0.18f, 0.16f, 1f)
                : new Color(0.20f, 0.24f, 0.31f, 1f);
            Button button = CreateButton(stage.Id, gridObject.transform, 26, buttonColor);
            string stageId = stage.Id;
            button.onClick.AddListener(() => progressManager.SelectStage(stageId));
            stageButtons[stage.Id] = button;
        }
    }

    private void CreateSummonPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("SummonTitle", parent, 36, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = "소환 - 히어로 뽑기";
        AddLayoutElement(title.gameObject, -1, 58);

        GameObject buttonRow = new GameObject("SummonButtons", typeof(RectTransform));
        buttonRow.transform.SetParent(parent, false);
        HorizontalLayoutGroup row = buttonRow.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 16;
        row.childControlWidth = true;
        row.childForceExpandWidth = true;
        AddLayoutElement(buttonRow, -1, 92);

        Button rollOne = CreateButton("1회", buttonRow.transform, 30, new Color(0.36f, 0.24f, 0.45f, 1f));
        Button rollTen = CreateButton("10회", buttonRow.transform, 30, new Color(0.36f, 0.24f, 0.45f, 1f));
        rollOne.onClick.AddListener(() => gachaManager.Roll(1));
        rollTen.onClick.AddListener(() => gachaManager.Roll(10));

        Text rule = CreateText("SummonRule", parent, 25, FontStyle.Normal, TextAnchor.UpperLeft);
        rule.text = "소모 순서: 히어로 뽑기권 먼저 사용, 부족한 횟수는 루비 150개씩 사용";
        AddLayoutElement(rule.gameObject, -1, 86);

        gachaText = CreateText("GachaResult", parent, 26, FontStyle.Normal, TextAnchor.UpperLeft);
        AddLayoutElement(gachaText.gameObject, -1, 420);
    }

    private void CreateBottomNav(Transform parent)
    {
        GameObject panel = CreatePanel("BottomNav", parent, new Color(0.10f, 0.13f, 0.19f, 1f));
        AddLayoutElement(panel, -1, 150);

        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 18, 18);
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;

        CreateTabButton(panel.transform, HudTab.Growth, "성장");
        CreateTabButton(panel.transform, HudTab.Stage, "스테이지");
        CreateTabButton(panel.transform, HudTab.Summon, "소환");
    }

    private void CreateTabButton(Transform parent, HudTab tab, string label)
    {
        Button button = CreateButton(label, parent, 30, new Color(0.20f, 0.25f, 0.34f, 1f));
        button.onClick.AddListener(() =>
        {
            activeTab = tab;
            UpdateView();
        });
        tabButtons[tab] = button;
    }

    private void UpdateView()
    {
        if (resourceText == null)
        {
            return;
        }

        StageDefinition stage = progressManager.CurrentStage;
        resourceText.text = "Gold " + wallet.Gold
            + "    EXP " + wallet.HeroExpItem
            + "    Ruby " + wallet.Ruby
            + "    Ticket " + wallet.HeroSummonTicket;
        stageText.text = "Stage " + stage.Id + "    Highest " + progressManager.HighestStageId;
        modeText.text = "Mode: " + GetModeLabel(progressManager.Mode);

        targetText.text = battleManager.TargetName;
        hpText.text = "HP " + battleManager.TargetHp + " / " + battleManager.TargetMaxHp;
        float hpRatio = battleManager.TargetMaxHp <= 0 ? 0f : Mathf.Clamp01((float)battleManager.TargetHp / battleManager.TargetMaxHp);
        hpFill.rectTransform.anchorMax = new Vector2(hpRatio, 1f);

        if (battleManager.IsBossFight)
        {
            progressText.text = "Boss Timer: " + Mathf.CeilToInt(battleManager.BossTimeRemaining) + "s";
        }
        else
        {
            progressText.text = "Kills: " + battleManager.KillsThisStage + " / " + battleManager.RequiredKills;
        }

        logText.text = battleManager.LastBattleLog;
        if (!string.IsNullOrEmpty(battleManager.LastDamageLog))
        {
            logText.text += "\n" + battleManager.LastDamageLog;
        }

        gachaText.text = gachaManager.LastResult;

        foreach (AbilityState ability in abilityManager.States)
        {
            if (abilityButtonTexts.TryGetValue(ability.Definition.Kind, out Text text))
            {
                string costText = ability.IsMaxed ? "MAX" : "Cost Gold " + ability.LevelUpCost;
                text.text = ability.Definition.DisplayName
                    + "  Lv." + ability.Level
                    + "\n" + abilityManager.GetDisplayValue(ability)
                    + "  " + costText;
            }
        }

        foreach (HeroState hero in battleManager.Heroes)
        {
            if (heroButtonTexts.TryGetValue(hero.Definition.Id, out Text text))
            {
                text.text = hero.Definition.DisplayName
                    + "  Lv." + hero.Level
                    + "\nATK " + hero.AttackPower
                    + "  Cost EXP " + hero.LevelUpCost
                    + "  Shard " + hero.Shards;
            }
        }

        foreach (KeyValuePair<string, Button> pair in stageButtons)
        {
            bool unlocked = GameData.IsStageUnlocked(pair.Key, progressManager.HighestStageId);
            pair.Value.interactable = unlocked;
            Text text = pair.Value.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = pair.Key == progressManager.CurrentStageId ? "[" + pair.Key + "]" : pair.Key;
            }
        }

        growthPanel.SetActive(activeTab == HudTab.Growth);
        stagePanel.SetActive(activeTab == HudTab.Stage);
        summonPanel.SetActive(activeTab == HudTab.Summon);

        foreach (KeyValuePair<HudTab, Button> pair in tabButtons)
        {
            Text text = pair.Value.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.color = pair.Key == activeTab ? new Color(1f, 0.91f, 0.40f, 1f) : Color.white;
            }
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
        colors.highlightedColor = color * 1.12f;
        colors.pressedColor = color * 0.84f;
        colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.55f);
        button.colors = colors;

        Text text = CreateText(label + "Text", buttonObject.transform, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 6);
        textRect.offsetMax = new Vector2(-10, -6);
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
        rect.sizeDelta = new Vector2(0, fontSize + 22);
        return text;
    }

    private void StretchToParent(GameObject target)
    {
        RectTransform rect = target.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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
