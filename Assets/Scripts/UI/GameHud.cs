using System;
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
        Hero,
        Stage,
        Summon,
        Shop,
        Support,
        Debug
    }

    private enum HeroPageTab
    {
        Formation,
        Trait,
        Statue,
        Seal,
        Relic
    }

    private StageProgressManager progressManager;
    private CurrencyWallet wallet;
    private AbilityManager abilityManager;
    private GameSpeedManager speedManager;
    private BattleManager battleManager;
    private GachaManager gachaManager;
    private Action resetSaveAction;

    private HudTab activeTab = HudTab.Growth;
    private bool contentPanelOpen = true;
    private GameObject canvasObject;

    private Text resourceText;
    private Text stageText;
    private Text modeText;
    private Text targetText;
    private Text hpText;
    private Text progressText;
    private Text supportText;
    private Text logText;
    private Text rewardText;
    private Text damagePopupText;
    private Text supportSummaryText;
    private Text totalCombatPowerText;
    private Text growthNoticeText;
    private Text fieldStagePillText;
    private Text guideQuestText;
    private Text damageMeterText;
    private Text centerSpawnText;
    private Image hpFill;
    private RectTransform battlefieldRect;
    private LayoutElement battleLayoutElement;
    private LayoutElement contentLayoutElement;
    private int observedHitSequence = -1;
    private float hitFlashRemaining;

    private GameObject contentRoot;
    private GameObject growthPanel;
    private GameObject heroPanel;
    private GameObject heroFormationContent;
    private Text heroFormationSummaryText;
    private Text heroOwnedEffectText;
    private Text heroPlaceholderText;
    private GameObject stagePanel;
    private GameObject summonPanel;
    private GameObject shopPanel;
    private GameObject supportPanel;
    private GameObject debugPanel;
    private GameObject heroFormationSavePrompt;
    private GameObject guideQuestDot;
    private Text gachaText;
    private Text debugText;
    private int selectedGrowthLevelStep = 1;
    private int selectedHeroPreset = 1;
    private int pendingHeroPreset = 0;
    private HudTab pendingTabAfterHeroFormationPrompt = HudTab.Growth;
    private string selectedHeroForPlacement = string.Empty;
    private HeroPageTab activeHeroPageTab = HeroPageTab.Formation;
    private string growthNoticeMessage = string.Empty;
    private float growthNoticeUntil;
    private bool heroFormationDirty;
    private bool heroFormationSavePromptOpen;
    private bool pendingContentOpenAfterHeroFormationPrompt = true;
    private bool pendingHeroPresetSwitch;

    private readonly Dictionary<AbilityKind, Text> abilityButtonTexts = new Dictionary<AbilityKind, Text>();
    private readonly Dictionary<AbilityKind, Text> abilityCostBadgeTexts = new Dictionary<AbilityKind, Text>();
    private readonly Dictionary<string, Text> heroButtonTexts = new Dictionary<string, Text>();
    private readonly Dictionary<HeroPageTab, Button> heroPageTabButtons = new Dictionary<HeroPageTab, Button>();
    private readonly Dictionary<int, Button> heroPresetButtons = new Dictionary<int, Button>();
    private readonly Dictionary<int, Button> heroFormationSlotButtons = new Dictionary<int, Button>();
    private readonly Dictionary<int, Button> heroFormationSlotRemoveButtons = new Dictionary<int, Button>();
    private readonly Dictionary<string, Button> heroRosterActionButtons = new Dictionary<string, Button>();
    private readonly List<Text> heroFormationSlotTexts = new List<Text>();
    private readonly Dictionary<AbilityKind, GameObject> abilityNotificationDots = new Dictionary<AbilityKind, GameObject>();
    private readonly Dictionary<string, GameObject> heroNotificationDots = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, Text> skillStatusTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, Text> petStatusTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, Image> heroBattleImages = new Dictionary<string, Image>();
    private readonly Dictionary<string, Text> heroBattleTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, RectTransform> heroBattleRects = new Dictionary<string, RectTransform>();
    private readonly Dictionary<int, Button> growthStepButtons = new Dictionary<int, Button>();
    private readonly Dictionary<string, Button> stageButtons = new Dictionary<string, Button>();
    private readonly Dictionary<int, Button> speedButtons = new Dictionary<int, Button>();
    private readonly Dictionary<HudTab, Button> tabButtons = new Dictionary<HudTab, Button>();
    private readonly Dictionary<HudTab, string> tabButtonLabels = new Dictionary<HudTab, string>();
    private readonly Dictionary<HudTab, List<GameObject>> tabNotificationDots = new Dictionary<HudTab, List<GameObject>>();
    private readonly List<string> editingFormationHeroIds = new List<string>();
    private readonly List<Image> enemyBattleImages = new List<Image>();
    private readonly List<Text> enemyBattleTexts = new List<Text>();
    private readonly List<RectTransform> enemyBattleRects = new List<RectTransform>();

    public void Initialize(
        StageProgressManager progress,
        CurrencyWallet currency,
        AbilityManager abilities,
        GameSpeedManager speed,
        BattleManager battle,
        GachaManager gacha,
        Action resetSave)
    {
        UnsubscribeEvents();

        progressManager = progress;
        wallet = currency;
        abilityManager = abilities;
        speedManager = speed;
        battleManager = battle;
        gachaManager = gacha;
        resetSaveAction = resetSave;

        ResetRuntimeUiState();
        CreateEventSystemIfNeeded();
        DestroyExistingHudCanvas();
        CreateHud();

        SubscribeEvents();

        UpdateView();
    }

    private void Update()
    {
        if (battleManager == null || damagePopupText == null)
        {
            return;
        }

        if (observedHitSequence != battleManager.HitSequence)
        {
            observedHitSequence = battleManager.HitSequence;
            hitFlashRemaining = 0.28f;
        }

        if (hitFlashRemaining > 0f)
        {
            hitFlashRemaining = Mathf.Max(0f, hitFlashRemaining - Time.deltaTime);
        }

        if (growthNoticeText != null
            && !string.IsNullOrEmpty(growthNoticeMessage)
            && Time.unscaledTime >= growthNoticeUntil)
        {
            growthNoticeMessage = string.Empty;
            growthNoticeText.text = string.Empty;
        }

        RefreshBattlefieldVisuals();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        progressManager.Changed += UpdateView;
        wallet.Changed += UpdateView;
        abilityManager.Changed += UpdateView;
        speedManager.Changed += UpdateView;
        battleManager.Changed += UpdateView;
        gachaManager.Changed += UpdateView;
    }

    private void UnsubscribeEvents()
    {
        if (progressManager != null)
        {
            progressManager.Changed -= UpdateView;
        }

        if (wallet != null)
        {
            wallet.Changed -= UpdateView;
        }

        if (abilityManager != null)
        {
            abilityManager.Changed -= UpdateView;
        }

        if (speedManager != null)
        {
            speedManager.Changed -= UpdateView;
        }

        if (battleManager != null)
        {
            battleManager.Changed -= UpdateView;
        }

        if (gachaManager != null)
        {
            gachaManager.Changed -= UpdateView;
        }
    }

    private void ResetRuntimeUiState()
    {
        resourceText = null;
        stageText = null;
        modeText = null;
        targetText = null;
        hpText = null;
        progressText = null;
        supportText = null;
        logText = null;
        rewardText = null;
        damagePopupText = null;
        supportSummaryText = null;
        totalCombatPowerText = null;
        growthNoticeText = null;
        fieldStagePillText = null;
        guideQuestText = null;
        damageMeterText = null;
        centerSpawnText = null;
        hpFill = null;
        battlefieldRect = null;
        battleLayoutElement = null;
        contentLayoutElement = null;
        observedHitSequence = -1;
        hitFlashRemaining = 0f;

        contentPanelOpen = true;
        contentRoot = null;
        growthPanel = null;
        heroPanel = null;
        heroFormationContent = null;
        heroFormationSummaryText = null;
        heroOwnedEffectText = null;
        heroPlaceholderText = null;
        stagePanel = null;
        summonPanel = null;
        shopPanel = null;
        supportPanel = null;
        debugPanel = null;
        heroFormationSavePrompt = null;
        guideQuestDot = null;
        gachaText = null;
        debugText = null;
        selectedGrowthLevelStep = 1;
        selectedHeroPreset = 1;
        pendingHeroPreset = 0;
        pendingTabAfterHeroFormationPrompt = HudTab.Growth;
        selectedHeroForPlacement = string.Empty;
        activeHeroPageTab = HeroPageTab.Formation;
        growthNoticeMessage = string.Empty;
        growthNoticeUntil = 0f;
        heroFormationDirty = false;
        heroFormationSavePromptOpen = false;
        pendingContentOpenAfterHeroFormationPrompt = true;
        pendingHeroPresetSwitch = false;

        abilityButtonTexts.Clear();
        abilityCostBadgeTexts.Clear();
        heroButtonTexts.Clear();
        heroPageTabButtons.Clear();
        heroPresetButtons.Clear();
        heroFormationSlotButtons.Clear();
        heroFormationSlotRemoveButtons.Clear();
        heroRosterActionButtons.Clear();
        heroFormationSlotTexts.Clear();
        abilityNotificationDots.Clear();
        heroNotificationDots.Clear();
        skillStatusTexts.Clear();
        petStatusTexts.Clear();
        heroBattleImages.Clear();
        heroBattleTexts.Clear();
        heroBattleRects.Clear();
        growthStepButtons.Clear();
        stageButtons.Clear();
        speedButtons.Clear();
        tabButtons.Clear();
        tabButtonLabels.Clear();
        tabNotificationDots.Clear();
        editingFormationHeroIds.Clear();
        enemyBattleImages.Clear();
        enemyBattleTexts.Clear();
        enemyBattleRects.Clear();
    }

    private void DestroyExistingHudCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.name == "IdleGameCanvas")
            {
                Destroy(canvas.gameObject);
            }
        }
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
        canvasObject = new GameObject("IdleGameCanvas", typeof(RectTransform));
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
        rootLayout.padding = new RectOffset(0, 0, 0, 0);
        rootLayout.spacing = 0;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        CreateHeader(root.transform);
        CreateBattlePanel(root.transform);
        CreateContentPanels(root.transform);
        CreateBottomNav(root.transform);
        CreateHeroFormationSavePrompt(root.transform);
    }

    private void CreateHeader(Transform parent)
    {
        GameObject panel = CreatePanel("Header", parent, new Color(0.02f, 0.025f, 0.035f, 0.98f));
        AddLayoutElement(panel, -1, 160);

        GameObject avatar = CreatePanel("PlayerAvatar", panel.transform, new Color(0.12f, 0.22f, 0.32f, 1f));
        RectTransform avatarRect = avatar.GetComponent<RectTransform>();
        avatarRect.anchorMin = new Vector2(0f, 0.5f);
        avatarRect.anchorMax = new Vector2(0f, 0.5f);
        avatarRect.pivot = new Vector2(0f, 0.5f);
        avatarRect.sizeDelta = new Vector2(112f, 112f);
        avatarRect.anchoredPosition = new Vector2(22f, 0f);

        Text avatarText = CreateText("AvatarText", avatar.transform, 38, FontStyle.Bold, TextAnchor.MiddleCenter);
        avatarText.text = "G";
        StretchToParent(avatarText.gameObject);

        stageText = CreateText("Stage", panel.transform, 31, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform stageRect = stageText.GetComponent<RectTransform>();
        stageRect.anchorMin = new Vector2(0f, 0.5f);
        stageRect.anchorMax = new Vector2(0f, 0.5f);
        stageRect.pivot = new Vector2(0f, 0.5f);
        stageRect.sizeDelta = new Vector2(360f, 44f);
        stageRect.anchoredPosition = new Vector2(152f, 24f);

        modeText = CreateText("Mode", panel.transform, 22, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform modeRect = modeText.GetComponent<RectTransform>();
        modeRect.anchorMin = new Vector2(0f, 0.5f);
        modeRect.anchorMax = new Vector2(0f, 0.5f);
        modeRect.pivot = new Vector2(0f, 0.5f);
        modeRect.sizeDelta = new Vector2(360f, 34f);
        modeRect.anchoredPosition = new Vector2(152f, -20f);

        GameObject resourcePill = CreatePanel("ResourcePill", panel.transform, new Color(0.09f, 0.10f, 0.13f, 0.95f));
        RectTransform resourceRect = resourcePill.GetComponent<RectTransform>();
        resourceRect.anchorMin = new Vector2(1f, 0.5f);
        resourceRect.anchorMax = new Vector2(1f, 0.5f);
        resourceRect.pivot = new Vector2(1f, 0.5f);
        bool showDebugGrantButton = IsDebugPanelEnabled();
        resourceRect.sizeDelta = new Vector2(showDebugGrantButton ? 430f : 520f, 66f);
        resourceRect.anchoredPosition = new Vector2(showDebugGrantButton ? -194f : -112f, 18f);
        resourceText = CreateText("Resources", resourcePill.transform, 28, FontStyle.Bold, TextAnchor.MiddleCenter);
        StretchToParent(resourceText.gameObject);

        if (showDebugGrantButton)
        {
            Button debugGrantButton = CreateButton("DBG", panel.transform, 24, new Color(0.26f, 0.18f, 0.12f, 1f));
            RectTransform debugRect = debugGrantButton.GetComponent<RectTransform>();
            debugRect.anchorMin = new Vector2(1f, 0.5f);
            debugRect.anchorMax = new Vector2(1f, 0.5f);
            debugRect.pivot = new Vector2(1f, 0.5f);
            debugRect.sizeDelta = new Vector2(76f, 66f);
            debugRect.anchoredPosition = new Vector2(-108f, 18f);
            debugGrantButton.onClick.AddListener(DebugGrantTestCurrency);
        }

        Button menuButton = CreateButton("≡", panel.transform, 36, new Color(0.12f, 0.16f, 0.22f, 1f));
        RectTransform menuRect = menuButton.GetComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(1f, 0.5f);
        menuRect.anchorMax = new Vector2(1f, 0.5f);
        menuRect.pivot = new Vector2(1f, 0.5f);
        menuRect.sizeDelta = new Vector2(76f, 66f);
        menuRect.anchoredPosition = new Vector2(-22f, 18f);
    }

    private void CreateHeroFormationSavePrompt(Transform parent)
    {
        heroFormationSavePrompt = CreatePanel("HeroFormationSavePrompt", parent, new Color(0f, 0f, 0f, 0.62f));
        LayoutElement overlayLayout = heroFormationSavePrompt.AddComponent<LayoutElement>();
        overlayLayout.ignoreLayout = true;
        StretchToParent(heroFormationSavePrompt);

        GameObject dialog = CreatePanel("HeroFormationSaveDialog", heroFormationSavePrompt.transform, new Color(0.12f, 0.16f, 0.24f, 1f));
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.sizeDelta = new Vector2(660f, 320f);
        dialogRect.anchoredPosition = Vector2.zero;

        Text title = CreateText("HeroFormationSaveTitle", dialog.transform, 34, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 70f);
        titleRect.anchoredPosition = new Vector2(0f, -26f);
        title.text = "편성 저장";

        Text message = CreateText("HeroFormationSaveMessage", dialog.transform, 25, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform messageRect = message.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0f, 0.5f);
        messageRect.anchorMax = new Vector2(1f, 0.5f);
        messageRect.pivot = new Vector2(0.5f, 0.5f);
        messageRect.sizeDelta = new Vector2(0f, 92f);
        messageRect.anchoredPosition = new Vector2(0f, 12f);
        message.text = "변경된 히어로 편성을 저장하시겠습니까?\n저장하면 현재 스테이지가 다시 시작됩니다.";

        GameObject buttonRow = new GameObject("HeroFormationSaveButtons", typeof(RectTransform));
        buttonRow.transform.SetParent(dialog.transform, false);
        RectTransform rowRect = buttonRow.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 0f);
        rowRect.anchorMax = new Vector2(1f, 0f);
        rowRect.pivot = new Vector2(0.5f, 0f);
        rowRect.sizeDelta = new Vector2(-72f, 76f);
        rowRect.anchoredPosition = new Vector2(0f, 34f);
        HorizontalLayoutGroup rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 18;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;

        Button confirmButton = CreateButton("확인", buttonRow.transform, 28, new Color(0.36f, 0.52f, 0.22f, 1f));
        Button cancelButton = CreateButton("취소", buttonRow.transform, 28, new Color(0.26f, 0.29f, 0.34f, 1f));
        confirmButton.onClick.AddListener(ConfirmHeroFormationSavePrompt);
        cancelButton.onClick.AddListener(CancelHeroFormationSavePrompt);

        heroFormationSavePrompt.SetActive(false);
    }

    private void CreateBattlePanel(Transform parent)
    {
        GameObject panel = CreatePanel("Battle", parent, new Color(0.14f, 0.16f, 0.20f, 1f));
        battleLayoutElement = AddLayoutElement(panel, -1, 870);

        CreateBattlefieldPanel(panel.transform);

        targetText = CreateText("Target", panel.transform, 36, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform targetRect = targetText.GetComponent<RectTransform>();
        targetRect.anchorMin = new Vector2(0.5f, 1f);
        targetRect.anchorMax = new Vector2(0.5f, 1f);
        targetRect.pivot = new Vector2(0.5f, 1f);
        targetRect.sizeDelta = new Vector2(420f, 48f);
        targetRect.anchoredPosition = new Vector2(0f, -88f);

        GameObject hpBar = CreatePanel("HpBar", panel.transform, new Color(0.03f, 0.04f, 0.05f, 1f));
        RectTransform hpRect = hpBar.GetComponent<RectTransform>();
        hpRect.anchorMin = new Vector2(0.5f, 1f);
        hpRect.anchorMax = new Vector2(0.5f, 1f);
        hpRect.pivot = new Vector2(0.5f, 1f);
        hpRect.sizeDelta = new Vector2(360f, 34f);
        hpRect.anchoredPosition = new Vector2(0f, -136f);
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

        progressText = CreateText("Progress", panel.transform, 30, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform progressRect = progressText.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0.5f, 1f);
        progressRect.anchorMax = new Vector2(0.5f, 1f);
        progressRect.pivot = new Vector2(0.5f, 1f);
        progressRect.sizeDelta = new Vector2(450f, 42f);
        progressRect.anchoredPosition = new Vector2(0f, -174f);

        supportText = CreateText("Support", panel.transform, 23, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform supportRect = supportText.GetComponent<RectTransform>();
        supportRect.anchorMin = new Vector2(0f, 1f);
        supportRect.anchorMax = new Vector2(0f, 1f);
        supportRect.pivot = new Vector2(0f, 1f);
        supportRect.sizeDelta = new Vector2(330f, 86f);
        supportRect.anchoredPosition = new Vector2(26f, -214f);

        CreateCombatSpeedControls(panel.transform);

        logText = CreateText("Log", panel.transform, 22, FontStyle.Bold, TextAnchor.LowerLeft);
        RectTransform logRect = logText.GetComponent<RectTransform>();
        logRect.anchorMin = new Vector2(0f, 0f);
        logRect.anchorMax = new Vector2(0f, 0f);
        logRect.pivot = new Vector2(0f, 0f);
        logRect.sizeDelta = new Vector2(460f, 80f);
        logRect.anchoredPosition = new Vector2(26f, 26f);

        rewardText = CreateText("Reward", panel.transform, 22, FontStyle.Bold, TextAnchor.LowerLeft);
        rewardText.color = new Color(1f, 0.86f, 0.36f, 1f);
        RectTransform rewardRect = rewardText.GetComponent<RectTransform>();
        rewardRect.anchorMin = new Vector2(0f, 0f);
        rewardRect.anchorMax = new Vector2(0f, 0f);
        rewardRect.pivot = new Vector2(0f, 0f);
        rewardRect.sizeDelta = new Vector2(460f, 38f);
        rewardRect.anchoredPosition = new Vector2(26f, 106f);

        targetText.transform.SetAsLastSibling();
        hpBar.transform.SetAsLastSibling();
        progressText.transform.SetAsLastSibling();
        supportText.transform.SetAsLastSibling();
        logText.transform.SetAsLastSibling();
        rewardText.transform.SetAsLastSibling();
    }

    private void CreateBattlefieldPanel(Transform parent)
    {
        GameObject field = CreatePanel("Battlefield", parent, new Color(0.18f, 0.20f, 0.24f, 1f));
        StretchToParent(field);
        battlefieldRect = field.GetComponent<RectTransform>();

        centerSpawnText = CreateText("SpawnPortal", field.transform, 82, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform portalRect = centerSpawnText.GetComponent<RectTransform>();
        portalRect.anchorMin = new Vector2(0.5f, 0.5f);
        portalRect.anchorMax = new Vector2(0.5f, 0.5f);
        portalRect.pivot = new Vector2(0.5f, 0.5f);
        portalRect.sizeDelta = new Vector2(120f, 120f);
        portalRect.anchoredPosition = Vector2.zero;
        centerSpawnText.text = "●";
        centerSpawnText.color = new Color(0.95f, 0.12f, 0.10f, 0.85f);

        GameObject stagePill = CreatePanel("FieldStagePill", field.transform, new Color(0.88f, 0.90f, 0.92f, 0.94f));
        RectTransform pillRect = stagePill.GetComponent<RectTransform>();
        pillRect.anchorMin = new Vector2(0.5f, 1f);
        pillRect.anchorMax = new Vector2(0.5f, 1f);
        pillRect.pivot = new Vector2(0.5f, 1f);
        pillRect.sizeDelta = new Vector2(210f, 58f);
        pillRect.anchoredPosition = new Vector2(0f, -10f);
        fieldStagePillText = CreateText("FieldStagePillText", stagePill.transform, 27, FontStyle.Bold, TextAnchor.MiddleCenter);
        fieldStagePillText.color = new Color(0.04f, 0.05f, 0.07f, 1f);
        StretchToParent(fieldStagePillText.gameObject);

        damagePopupText = CreateText("DamagePopup", field.transform, 32, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform damageRect = damagePopupText.GetComponent<RectTransform>();
        damageRect.anchorMin = new Vector2(0.5f, 0.5f);
        damageRect.anchorMax = new Vector2(0.5f, 0.5f);
        damageRect.pivot = new Vector2(0.5f, 0.5f);
        damageRect.sizeDelta = new Vector2(260f, 92f);
        damageRect.anchoredPosition = new Vector2(0f, 18f);

        GameObject damageMeter = CreatePanel("DamageMeter", field.transform, new Color(0.02f, 0.025f, 0.035f, 0.76f));
        RectTransform damageMeterRect = damageMeter.GetComponent<RectTransform>();
        damageMeterRect.anchorMin = new Vector2(1f, 0f);
        damageMeterRect.anchorMax = new Vector2(1f, 0f);
        damageMeterRect.pivot = new Vector2(1f, 0f);
        damageMeterRect.sizeDelta = new Vector2(250f, 132f);
        damageMeterRect.anchoredPosition = new Vector2(-12f, 12f);
        damageMeterText = CreateText("DamageMeterText", damageMeter.transform, 20, FontStyle.Bold, TextAnchor.UpperLeft);
        RectTransform damageMeterTextRect = damageMeterText.GetComponent<RectTransform>();
        damageMeterTextRect.anchorMin = Vector2.zero;
        damageMeterTextRect.anchorMax = Vector2.one;
        damageMeterTextRect.offsetMin = new Vector2(12f, 8f);
        damageMeterTextRect.offsetMax = new Vector2(-12f, -8f);

        GameObject guideQuest = CreatePanel("GuideQuestCard", field.transform, new Color(0.03f, 0.04f, 0.06f, 0.78f));
        RectTransform guideRect = guideQuest.GetComponent<RectTransform>();
        guideRect.anchorMin = new Vector2(1f, 1f);
        guideRect.anchorMax = new Vector2(1f, 1f);
        guideRect.pivot = new Vector2(1f, 1f);
        guideRect.sizeDelta = new Vector2(310f, 86f);
        guideRect.anchoredPosition = new Vector2(-12f, -80f);
        guideQuestText = CreateText("GuideQuestText", guideQuest.transform, 21, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform guideTextRect = guideQuestText.GetComponent<RectTransform>();
        guideTextRect.anchorMin = Vector2.zero;
        guideTextRect.anchorMax = Vector2.one;
        guideTextRect.offsetMin = new Vector2(18f, 8f);
        guideTextRect.offsetMax = new Vector2(-18f, -8f);
        guideQuestDot = CreateNotificationDot(guideQuest.transform, 34f, new Vector2(-12f, -12f));

        foreach (HeroDefinition hero in GameData.Heroes)
        {
            GameObject actor = CreateBattleActor(hero.Id + "HeroActor", field.transform, new Vector2(74f, 74f), new Color(0.16f, 0.24f, 0.34f, 1f));
            Image image = actor.GetComponent<Image>();
            Text label = CreateText(hero.Id + "BattleLabel", actor.transform, 19, FontStyle.Bold, TextAnchor.MiddleCenter);
            StretchToParent(label.gameObject);
            label.text = GetRarityBadge(hero.Rarity) + "\n" + GetShortHeroLabel(hero);
            heroBattleRects[hero.Id] = actor.GetComponent<RectTransform>();
            heroBattleImages[hero.Id] = image;
            heroBattleTexts[hero.Id] = label;
        }

        for (int i = 0; i < GameData.MaxVisibleEnemies; i++)
        {
            GameObject enemy = CreateBattleActor("EnemyActor" + i, field.transform, new Vector2(58f, 58f), new Color(0.56f, 0.13f, 0.11f, 1f));
            Image image = enemy.GetComponent<Image>();
            Text label = CreateText("Enemy" + i + "Text", enemy.transform, 18, FontStyle.Bold, TextAnchor.MiddleCenter);
            StretchToParent(label.gameObject);
            label.text = "M";
            enemyBattleRects.Add(enemy.GetComponent<RectTransform>());
            enemyBattleImages.Add(image);
            enemyBattleTexts.Add(label);
        }

        stagePill.transform.SetAsLastSibling();
        guideQuest.transform.SetAsLastSibling();
        damageMeter.transform.SetAsLastSibling();
        damagePopupText.transform.SetAsLastSibling();
    }

    private void CreateCombatSpeedControls(Transform parent)
    {
        GameObject speedRow = new GameObject("CombatSpeedButtons", typeof(RectTransform));
        speedRow.transform.SetParent(parent, false);
        RectTransform speedRect = speedRow.GetComponent<RectTransform>();
        speedRect.anchorMin = new Vector2(1f, 0f);
        speedRect.anchorMax = new Vector2(1f, 0f);
        speedRect.pivot = new Vector2(1f, 0f);
        speedRect.sizeDelta = new Vector2(310f, 74f);
        speedRect.anchoredPosition = new Vector2(-26f, 154f);

        HorizontalLayoutGroup row = speedRow.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 10;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = true;
        row.childForceExpandHeight = true;

        CreateSpeedButton(speedRow.transform, GameSpeedManager.NormalSpeed);
        CreateSpeedButton(speedRow.transform, GameSpeedManager.FreeSpeed);
        CreateSpeedButton(speedRow.transform, GameSpeedManager.PremiumSpeed);
    }

    private void CreateSpeedButton(Transform parent, int multiplier)
    {
        Button button = CreateButton(multiplier + "x", parent, 26, new Color(0.18f, 0.24f, 0.32f, 1f));
        button.onClick.AddListener(() => speedManager.TrySelectSpeed(multiplier));
        speedButtons[multiplier] = button;
    }

    private void CreateContentPanels(Transform parent)
    {
        contentRoot = CreatePanel("Content", parent, new Color(0.09f, 0.10f, 0.13f, 1f));
        contentLayoutElement = AddLayoutElement(contentRoot, -1, 760);

        RectTransform contentRect = contentRoot.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);

        growthPanel = CreatePanel("GrowthPanel", contentRoot.transform, new Color(0.13f, 0.16f, 0.22f, 1f));
        heroPanel = CreatePanel("HeroPanel", contentRoot.transform, new Color(0.12f, 0.16f, 0.23f, 1f));
        stagePanel = CreatePanel("StagePanel", contentRoot.transform, new Color(0.12f, 0.15f, 0.19f, 1f));
        summonPanel = CreatePanel("SummonPanel", contentRoot.transform, new Color(0.15f, 0.13f, 0.19f, 1f));
        shopPanel = CreatePanel("ShopPanel", contentRoot.transform, new Color(0.16f, 0.13f, 0.10f, 1f));
        supportPanel = CreatePanel("SupportPanel", contentRoot.transform, new Color(0.11f, 0.15f, 0.16f, 1f));

        StretchToParent(growthPanel);
        StretchToParent(heroPanel);
        StretchToParent(stagePanel);
        StretchToParent(summonPanel);
        StretchToParent(shopPanel);
        StretchToParent(supportPanel);

        CreateGrowthPanel(growthPanel.transform);
        CreateHeroPanel(heroPanel.transform);
        CreateStagePanel(stagePanel.transform);
        CreateSummonPanel(summonPanel.transform);
        CreateShopPanel(shopPanel.transform);
        CreateSupportPanel(supportPanel.transform);

        if (IsDebugPanelEnabled())
        {
            debugPanel = CreatePanel("DebugPanel", contentRoot.transform, new Color(0.13f, 0.13f, 0.13f, 1f));
            StretchToParent(debugPanel);
            CreateDebugPanel(debugPanel.transform);
        }
    }

    private void CreateGrowthPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 6;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("GrowthTitle", parent, 32, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = "성장";
        AddLayoutElement(title.gameObject, -1, 38);

        totalCombatPowerText = CreateText("TotalCombatPower", parent, 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        totalCombatPowerText.text = "종합 전투력 0";
        AddLayoutElement(totalCombatPowerText.gameObject, -1, 42);

        growthNoticeText = CreateText("GrowthNotice", parent, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
        growthNoticeText.color = new Color(1f, 0.55f, 0.34f, 1f);
        AddLayoutElement(growthNoticeText.gameObject, -1, 30);

        GameObject stepRow = new GameObject("GrowthStepRow", typeof(RectTransform));
        stepRow.transform.SetParent(parent, false);
        HorizontalLayoutGroup stepLayout = stepRow.AddComponent<HorizontalLayoutGroup>();
        stepLayout.spacing = 10;
        stepLayout.childControlWidth = true;
        stepLayout.childControlHeight = true;
        stepLayout.childForceExpandWidth = true;
        stepLayout.childForceExpandHeight = true;
        AddLayoutElement(stepRow, -1, 50);

        int[] steps = { 1, 10, 100, 1000 };
        foreach (int step in steps)
        {
            Button stepButton = CreateButton(step + "x", stepRow.transform, 24, new Color(0.18f, 0.24f, 0.38f, 1f));
            int capturedStep = step;
            stepButton.onClick.AddListener(() =>
            {
                selectedGrowthLevelStep = capturedStep;
                UpdateView();
            });
            growthStepButtons[step] = stepButton;
        }

        foreach (AbilityState ability in abilityManager.States)
        {
            Button button = CreateButton(ability.Definition.DisplayName, parent, 22, new Color(0.48f, 0.54f, 0.66f, 1f));
            AddLayoutElement(button.gameObject, -1, 64);

            AbilityKind kind = ability.Definition.Kind;
            button.onClick.AddListener(() => TryLevelUpAbilityFromHud(kind));
            Text rowText = button.GetComponentInChildren<Text>();
            rowText.alignment = TextAnchor.MiddleLeft;
            rowText.color = Color.white;
            RectTransform rowTextRect = rowText.GetComponent<RectTransform>();
            rowTextRect.anchorMin = Vector2.zero;
            rowTextRect.anchorMax = new Vector2(0.70f, 1f);
            rowTextRect.offsetMin = new Vector2(24f, 4f);
            rowTextRect.offsetMax = new Vector2(-8f, -4f);
            abilityButtonTexts[kind] = rowText;

            GameObject costBadge = CreatePanel(ability.Definition.Id + "CostBadge", button.transform, new Color(0.56f, 0.88f, 0.24f, 1f));
            RectTransform costBadgeRect = costBadge.GetComponent<RectTransform>();
            costBadgeRect.anchorMin = new Vector2(0.73f, 0.14f);
            costBadgeRect.anchorMax = new Vector2(0.98f, 0.86f);
            costBadgeRect.offsetMin = Vector2.zero;
            costBadgeRect.offsetMax = Vector2.zero;
            Text costText = CreateText(ability.Definition.Id + "CostText", costBadge.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
            costText.color = new Color(0.04f, 0.06f, 0.05f, 1f);
            StretchToParent(costText.gameObject);
            abilityCostBadgeTexts[kind] = costText;

            abilityNotificationDots[kind] = CreateNotificationDot(button.transform, 40f, new Vector2(-16f, -16f));
        }
    }

    private void CreateHeroPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text heroTitle = CreateText("HeroGrowthTitle", parent, 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        heroTitle.text = "편성";
        AddLayoutElement(heroTitle.gameObject, -1, 38);

        GameObject heroPageTabs = new GameObject("HeroPageTabs", typeof(RectTransform));
        heroPageTabs.transform.SetParent(parent, false);
        HorizontalLayoutGroup tabLayout = heroPageTabs.AddComponent<HorizontalLayoutGroup>();
        tabLayout.spacing = 8;
        tabLayout.childControlWidth = true;
        tabLayout.childControlHeight = true;
        tabLayout.childForceExpandWidth = true;
        tabLayout.childForceExpandHeight = true;
        AddLayoutElement(heroPageTabs, -1, 54);

        CreateHeroPageTabButton(heroPageTabs.transform, HeroPageTab.Formation, "편성");
        CreateHeroPageTabButton(heroPageTabs.transform, HeroPageTab.Trait, "특성");
        CreateHeroPageTabButton(heroPageTabs.transform, HeroPageTab.Statue, "석상");
        CreateHeroPageTabButton(heroPageTabs.transform, HeroPageTab.Seal, "인장");
        CreateHeroPageTabButton(heroPageTabs.transform, HeroPageTab.Relic, "유물");

        heroFormationContent = new GameObject("HeroFormationContent", typeof(RectTransform));
        heroFormationContent.transform.SetParent(parent, false);
        VerticalLayoutGroup formationLayout = heroFormationContent.AddComponent<VerticalLayoutGroup>();
        formationLayout.spacing = 8;
        formationLayout.childControlWidth = true;
        formationLayout.childControlHeight = true;
        formationLayout.childForceExpandWidth = true;
        formationLayout.childForceExpandHeight = false;
        AddLayoutElement(heroFormationContent, -1, 594);

        heroFormationSummaryText = CreateText("HeroFormationSummary", heroFormationContent.transform, 23, FontStyle.Bold, TextAnchor.MiddleLeft);
        AddLayoutElement(heroFormationSummaryText.gameObject, -1, 30);

        GameObject formationArea = CreatePanel("FormationArea", heroFormationContent.transform, new Color(0.33f, 0.42f, 0.58f, 1f));
        AddLayoutElement(formationArea, -1, 188);
        HorizontalLayoutGroup formationAreaLayout = formationArea.AddComponent<HorizontalLayoutGroup>();
        formationAreaLayout.padding = new RectOffset(14, 14, 14, 14);
        formationAreaLayout.spacing = 14;
        formationAreaLayout.childControlWidth = true;
        formationAreaLayout.childControlHeight = true;
        formationAreaLayout.childForceExpandWidth = true;
        formationAreaLayout.childForceExpandHeight = true;

        GameObject slotGrid = new GameObject("FormationSlots", typeof(RectTransform));
        slotGrid.transform.SetParent(formationArea.transform, false);
        GridLayoutGroup slotGridLayout = slotGrid.AddComponent<GridLayoutGroup>();
        slotGridLayout.cellSize = new Vector2(148f, 72f);
        slotGridLayout.spacing = new Vector2(10f, 12f);
        slotGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        slotGridLayout.constraintCount = 4;
        AddLayoutElement(slotGrid, -1, -1);

        for (int i = 0; i < GameData.MaxPartyHeroes; i++)
        {
            int slotIndex = i;
            Button slot = CreateButton(string.Empty, slotGrid.transform, 18, new Color(0.18f, 0.22f, 0.31f, 1f));
            slot.onClick.AddListener(() => TryPlaceSelectedHeroInSlot(slotIndex));
            Text slotText = slot.GetComponentInChildren<Text>();
            slotText.name = "FormationSlotText" + i;
            slotText.fontSize = 18;
            slotText.alignment = TextAnchor.MiddleCenter;
            StretchToParent(slotText.gameObject);
            heroFormationSlotTexts.Add(slotText);
            heroFormationSlotButtons[i] = slot;

            Button removeButton = CreateCornerActionButton("-", slot.transform, new Color(0.58f, 0.12f, 0.12f, 1f));
            removeButton.onClick.AddListener(() =>
            {
                RemoveHeroFromEditingFormationSlot(slotIndex);
            });
            heroFormationSlotRemoveButtons[i] = removeButton;
        }

        GameObject presetColumn = new GameObject("PresetColumn", typeof(RectTransform));
        presetColumn.transform.SetParent(formationArea.transform, false);
        VerticalLayoutGroup presetLayout = presetColumn.AddComponent<VerticalLayoutGroup>();
        presetLayout.spacing = 10;
        presetLayout.childControlWidth = true;
        presetLayout.childControlHeight = true;
        presetLayout.childForceExpandWidth = true;
        presetLayout.childForceExpandHeight = true;
        AddLayoutElement(presetColumn, 132, -1);

        Text presetTitle = CreateText("PresetTitle", presetColumn.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
        presetTitle.text = "프리셋";
        AddLayoutElement(presetTitle.gameObject, -1, 34);
        for (int preset = 1; preset <= GameData.MaxHeroPresets; preset++)
        {
            Button presetButton = CreateButton(preset.ToString(), presetColumn.transform, 24, new Color(0.21f, 0.29f, 0.45f, 1f));
            int capturedPreset = preset;
            presetButton.onClick.AddListener(() =>
            {
                RequestHeroPresetChange(capturedPreset);
            });
            heroPresetButtons[preset] = presetButton;
        }

        GameObject rosterGrid = new GameObject("HeroRosterGrid", typeof(RectTransform));
        rosterGrid.transform.SetParent(heroFormationContent.transform, false);
        GridLayoutGroup rosterLayout = rosterGrid.AddComponent<GridLayoutGroup>();
        rosterLayout.cellSize = new Vector2(154f, 124f);
        rosterLayout.spacing = new Vector2(10f, 10f);
        rosterLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        rosterLayout.constraintCount = 5;
        AddLayoutElement(rosterGrid, -1, 258);

        foreach (HeroDefinition hero in GameData.Heroes)
        {
            Button button = CreateButton(hero.DisplayName, rosterGrid.transform, 15, GetRarityColor(hero.Rarity));
            string heroId = hero.Id;
            heroButtonTexts[hero.Id] = button.GetComponentInChildren<Text>();
            Button actionButton = CreateCornerActionButton("+", button.transform, new Color(0.88f, 0.72f, 0.20f, 1f));
            actionButton.onClick.AddListener(() => SelectOrRemoveRosterHero(heroId));
            heroRosterActionButtons[hero.Id] = actionButton;
            heroNotificationDots[hero.Id] = CreateNotificationDot(button.transform, 40f, new Vector2(-16f, -16f));
        }

        heroOwnedEffectText = CreateText("HeroOwnedEffect", heroFormationContent.transform, 24, FontStyle.Bold, TextAnchor.MiddleCenter);
        AddLayoutElement(heroOwnedEffectText.gameObject, -1, 34);

        GameObject actionRow = new GameObject("HeroFormationActions", typeof(RectTransform));
        actionRow.transform.SetParent(heroFormationContent.transform, false);
        HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 14;
        actionLayout.childControlWidth = true;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = true;
        actionLayout.childForceExpandHeight = true;
        AddLayoutElement(actionRow, -1, 58);
        CreateButton("자동 배치", actionRow.transform, 24, new Color(0.72f, 0.56f, 0.15f, 1f));
        CreateButton("일괄 승급", actionRow.transform, 24, new Color(0.34f, 0.35f, 0.37f, 1f));

        heroPlaceholderText = CreateText("HeroPagePlaceholder", parent, 28, FontStyle.Bold, TextAnchor.MiddleCenter);
        heroPlaceholderText.text = "준비 중";
        AddLayoutElement(heroPlaceholderText.gameObject, -1, 594);
        heroPlaceholderText.gameObject.SetActive(false);
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

    private void CreateShopPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("ShopTitle", parent, 36, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = "상점";
        AddLayoutElement(title.gameObject, -1, 58);

        Button speedProduct = CreateButton("4x", parent, 30, new Color(0.42f, 0.28f, 0.14f, 1f));
        AddLayoutElement(speedProduct.gameObject, -1, 96);
        speedProduct.onClick.AddListener(() => speedManager.TrySelectSpeed(GameSpeedManager.PremiumSpeed));

        Button rubyProduct = CreateButton("Ruby", parent, 30, new Color(0.36f, 0.18f, 0.42f, 1f));
        AddLayoutElement(rubyProduct.gameObject, -1, 96);

        Button ticketProduct = CreateButton("Ticket", parent, 30, new Color(0.24f, 0.30f, 0.42f, 1f));
        AddLayoutElement(ticketProduct.gameObject, -1, 96);
    }

    private void CreateSupportPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 14;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("SupportTitle", parent, 34, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = "지원 - 자동 스킬과 펫";
        AddLayoutElement(title.gameObject, -1, 54);

        supportSummaryText = CreateText("PartySupportInfo", parent, 26, FontStyle.Bold, TextAnchor.MiddleLeft);
        AddLayoutElement(supportSummaryText.gameObject, -1, 46);

        Text skillTitle = CreateText("SkillSupportTitle", parent, 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        skillTitle.text = "자동 스킬";
        AddLayoutElement(skillTitle.gameObject, -1, 42);

        foreach (CombatSkillState skill in battleManager.Skills)
        {
            GameObject row = CreatePanel(skill.Definition.Id + "SupportRow", parent, new Color(0.19f, 0.24f, 0.28f, 1f));
            AddLayoutElement(row, -1, 96);
            Text text = CreateText(skill.Definition.Id + "SupportText", row.transform, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18, 8);
            textRect.offsetMax = new Vector2(-18, -8);
            skillStatusTexts[skill.Definition.Id] = text;
        }

        Text petTitle = CreateText("PetSupportTitle", parent, 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        petTitle.text = "펫";
        AddLayoutElement(petTitle.gameObject, -1, 42);

        foreach (PetState pet in battleManager.Pets)
        {
            GameObject row = CreatePanel(pet.Definition.Id + "SupportRow", parent, new Color(0.18f, 0.26f, 0.22f, 1f));
            AddLayoutElement(row, -1, 106);
            Text text = CreateText(pet.Definition.Id + "SupportText", row.transform, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18, 8);
            textRect.offsetMax = new Vector2(-18, -8);
            petStatusTexts[pet.Definition.Id] = text;
        }
    }

    private void CreateDebugPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("DebugTitle", parent, 36, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = "QA 디버그";
        AddLayoutElement(title.gameObject, -1, 58);

        GameObject gridObject = new GameObject("DebugGrid", typeof(RectTransform));
        gridObject.transform.SetParent(parent, false);
        GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(300, 82);
        grid.spacing = new Vector2(16, 16);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        AddLayoutElement(gridObject, -1, 420);

        CreateDebugButton("Gold +5000", gridObject.transform, () => wallet.AddGold(5000));
        CreateDebugButton("EXP +5000", gridObject.transform, () => wallet.AddHeroExpItem(5000));
        CreateDebugButton("Ruby +1500", gridObject.transform, () => wallet.AddRuby(1500));
        CreateDebugButton("Ticket +10", gridObject.transform, () => wallet.AddHeroSummonTicket(10));
        CreateDebugButton("Hero Lv +5", gridObject.transform, () => battleManager.DebugLevelAllHeroes(5));
        CreateDebugButton("Unlock 4x", gridObject.transform, () => speedManager.DebugSetFourTimesEntitlement(true));
        CreateDebugButton("Unlock All", gridObject.transform, () => progressManager.DebugUnlockThrough(GameData.ChapterOneBossStageId));
        CreateDebugButton("1-19 Repeat", gridObject.transform, () => progressManager.DebugJumpToStage(GameData.BossFallbackStageId, ProgressMode.RepeatSelected));
        CreateDebugButton("1-20 Boss", gridObject.transform, () => progressManager.DebugJumpToStage(GameData.ChapterOneBossStageId, ProgressMode.AutoProgress));
        CreateDebugButton("Reset Save", gridObject.transform, () => resetSaveAction?.Invoke(), new Color(0.45f, 0.16f, 0.14f, 1f), false);

        GameObject speedRow = new GameObject("SpeedButtons", typeof(RectTransform));
        speedRow.transform.SetParent(parent, false);
        HorizontalLayoutGroup row = speedRow.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 16;
        row.childControlWidth = true;
        row.childForceExpandWidth = true;
        AddLayoutElement(speedRow, -1, 90);

        CreateDebugButton("Time x1", speedRow.transform, () => SetTimeScale(1f));
        CreateDebugButton("Time x5", speedRow.transform, () => SetTimeScale(5f));
        CreateDebugButton("Time x20", speedRow.transform, () => SetTimeScale(20f));

        debugText = CreateText("DebugStatus", parent, 26, FontStyle.Normal, TextAnchor.UpperLeft);
        AddLayoutElement(debugText.gameObject, -1, 260);
    }

    private void CreateDebugButton(string label, Transform parent, Action action)
    {
        CreateDebugButton(label, parent, action, new Color(0.23f, 0.25f, 0.28f, 1f));
    }

    private void CreateDebugButton(string label, Transform parent, Action action, Color color, bool refreshAfter = true)
    {
        Button button = CreateButton(label, parent, 25, color);
        button.onClick.AddListener(() =>
        {
            action?.Invoke();
            if (refreshAfter)
            {
                UpdateView();
            }
        });
    }

    private void DebugGrantTestCurrency()
    {
        wallet.AddGold(50000000);
        wallet.AddRuby(15000);
        wallet.AddHeroExpItem(100000);
        wallet.AddHeroSummonTicket(100);
        UpdateView();
    }

    private void CreateBottomNav(Transform parent)
    {
        GameObject panel = CreatePanel("BottomNav", parent, new Color(0.08f, 0.11f, 0.17f, 1f));
        AddLayoutElement(panel, -1, 130);

        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        CreateTabButton(panel.transform, HudTab.Growth, "⚔\n성장");
        CreateTabButton(panel.transform, HudTab.Hero, "★\n히어로");
        CreateTabButton(panel.transform, HudTab.Summon, "◇\n소환");
        CreateTabButton(panel.transform, HudTab.Stage, "▣\n던전 배틀");
        CreateTabButton(panel.transform, HudTab.Shop, "▤\n상점");
    }

    private void CreateHeroPageTabButton(Transform parent, HeroPageTab tab, string label)
    {
        Button button = CreateButton(label, parent, 22, new Color(0.18f, 0.24f, 0.38f, 1f));
        button.onClick.AddListener(() =>
        {
            activeHeroPageTab = tab;
            UpdateView();
        });
        heroPageTabButtons[tab] = button;
    }

    private Button CreateCornerActionButton(string label, Transform parent, Color color)
    {
        Button button = CreateButton(label, parent, 20, color);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(34f, 34f);
        rect.anchoredPosition = new Vector2(-4f, -4f);
        return button;
    }

    private void CreateTabButton(Transform parent, HudTab tab, string label)
    {
        Button button = CreateButton(label, parent, 24, new Color(0.13f, 0.17f, 0.25f, 1f));
        RegisterTabNotificationDot(tab, CreateNotificationDot(button.transform, 38f, new Vector2(-14f, -14f)));
        button.onClick.AddListener(() =>
        {
            RequestTabChange(tab);
        });
        tabButtons[tab] = button;
        tabButtonLabels[tab] = label;
    }

    private void RequestTabChange(HudTab tab)
    {
        HudTab targetTab;
        bool targetContentOpen;
        if (tab == HudTab.Growth && activeTab == HudTab.Growth && contentPanelOpen)
        {
            targetTab = activeTab;
            targetContentOpen = false;
        }
        else
        {
            targetTab = tab;
            targetContentOpen = true;
        }

        if (ShouldPromptHeroFormationSave(targetTab, targetContentOpen))
        {
            ShowHeroFormationSavePromptForTab(targetTab, targetContentOpen);
            return;
        }

        ApplyTabState(targetTab, targetContentOpen);
    }

    private bool ShouldPromptHeroFormationSave(HudTab targetTab, bool targetContentOpen)
    {
        return !heroFormationSavePromptOpen
            && contentPanelOpen
            && activeTab == HudTab.Hero
            && HasHeroFormationPendingChanges()
            && (targetTab != HudTab.Hero || !targetContentOpen);
    }

    private void ShowHeroFormationSavePromptForTab(HudTab targetTab, bool targetContentOpen)
    {
        pendingHeroPresetSwitch = false;
        pendingHeroPreset = 0;
        pendingTabAfterHeroFormationPrompt = targetTab;
        pendingContentOpenAfterHeroFormationPrompt = targetContentOpen;
        heroFormationSavePromptOpen = true;
        UpdateView();
    }

    private void ShowHeroFormationSavePromptForPreset(int preset)
    {
        pendingHeroPresetSwitch = true;
        pendingHeroPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        heroFormationSavePromptOpen = true;
        UpdateView();
    }

    private void ApplyTabState(HudTab targetTab, bool targetContentOpen)
    {
        bool openingHeroPanel = targetTab == HudTab.Hero && (!contentPanelOpen || activeTab != HudTab.Hero);
        activeTab = targetTab;
        contentPanelOpen = targetContentOpen;
        if (openingHeroPanel)
        {
            LoadHeroFormationDraftFromPreset(battleManager.ActiveHeroPreset);
        }

        UpdateView();
    }

    private void ConfirmHeroFormationSavePrompt()
    {
        if (!battleManager.ApplyHeroFormation(selectedHeroPreset, editingFormationHeroIds))
        {
            heroFormationSavePromptOpen = false;
            UpdateView();
            return;
        }

        heroFormationDirty = false;
        selectedHeroForPlacement = string.Empty;
        heroFormationSavePromptOpen = false;

        if (pendingHeroPresetSwitch)
        {
            int preset = pendingHeroPreset;
            ClearHeroFormationPromptTarget();
            LoadHeroFormationDraftFromPreset(preset);
            UpdateView();
            return;
        }

        HudTab targetTab = pendingTabAfterHeroFormationPrompt;
        bool targetContentOpen = pendingContentOpenAfterHeroFormationPrompt;
        ClearHeroFormationPromptTarget();
        ApplyTabState(targetTab, targetContentOpen);
    }

    private void CancelHeroFormationSavePrompt()
    {
        heroFormationSavePromptOpen = false;
        ClearHeroFormationPromptTarget();
        UpdateView();
    }

    private void ClearHeroFormationPromptTarget()
    {
        pendingHeroPresetSwitch = false;
        pendingHeroPreset = 0;
        pendingTabAfterHeroFormationPrompt = activeTab;
        pendingContentOpenAfterHeroFormationPrompt = contentPanelOpen;
    }

    private void UpdateView()
    {
        if (resourceText == null)
        {
            return;
        }

        StageDefinition stage = progressManager.CurrentStage;
        resourceText.text = "G " + FormatShortNumber(wallet.Gold)
            + "    R " + FormatShortNumber(wallet.Ruby)
            + "    T " + FormatShortNumber(wallet.HeroSummonTicket)
            + "    x" + speedManager.CurrentMultiplier;
        stageText.text = "Guardian";
        modeText.text = "전투력 " + FormatShortNumber(battleManager.TotalCombatPower)
            + "    " + GetModeLabel(progressManager.Mode)
            + "    MAX " + progressManager.HighestStageId;
        if (fieldStagePillText != null)
        {
            fieldStagePillText.text = battleManager.IsBossFight ? stage.Id + " BOSS" : stage.Id;
        }

        targetText.text = battleManager.TargetName;
        hpText.text = "HP " + FormatShortNumber(battleManager.TargetHp) + " / " + FormatShortNumber(battleManager.TargetMaxHp);
        float hpRatio = battleManager.TargetMaxHp <= 0 ? 0f : Mathf.Clamp01((float)battleManager.TargetHp / battleManager.TargetMaxHp);
        hpFill.rectTransform.anchorMax = new Vector2(hpRatio, 1f);

        if (battleManager.IsBossFight)
        {
            progressText.text = "Boss Timer: " + Mathf.CeilToInt(battleManager.BossTimeRemaining) + "s";
        }
        else
        {
            progressText.text = "Kills: " + battleManager.KillsThisStage + " / " + battleManager.RequiredKills
                + "    Visible " + battleManager.VisibleEnemyCount;
        }

        if (guideQuestText != null)
        {
            string questGoal = battleManager.IsBossFight
                ? "보스 처치"
                : "스테이지 " + stage.Id + " 클리어";
            string questProgress = battleManager.IsBossFight
                ? Mathf.CeilToInt(battleManager.BossTimeRemaining) + "초 남음"
                : battleManager.KillsThisStage + "/" + battleManager.RequiredKills;
            guideQuestText.text = "가이드 퀘스트\n" + questGoal + "  " + questProgress;
        }

        if (damageMeterText != null)
        {
            string source = string.IsNullOrEmpty(battleManager.LastHitSourceName) ? "대기" : battleManager.LastHitSourceName;
            damageMeterText.text = "데미지 미터기"
                + "\n" + source + "  " + FormatShortNumber(battleManager.LastHitDamage)
                + "\n파티 ATK " + FormatShortNumber(battleManager.PartyAttackPower)
                + "\n전투력 " + FormatShortNumber(battleManager.TotalCombatPower);
        }

        supportText.text = battleManager.SupportStatusText;

        logText.text = battleManager.LastBattleLog;
        if (!string.IsNullOrEmpty(battleManager.LastDamageLog))
        {
            logText.text += "\n" + battleManager.LastDamageLog;
        }

        rewardText.text = battleManager.LastRewardLog;
        RefreshBattlefieldVisuals();

        gachaText.text = gachaManager.LastResult;

        if (totalCombatPowerText != null)
        {
            totalCombatPowerText.text = "종합 전투력  " + FormatShortNumber(battleManager.TotalCombatPower);
        }

        if (growthNoticeText != null)
        {
            growthNoticeText.text = Time.unscaledTime < growthNoticeUntil ? growthNoticeMessage : string.Empty;
        }

        foreach (KeyValuePair<int, Button> pair in growthStepButtons)
        {
            bool selected = pair.Key == selectedGrowthLevelStep;
            SetButtonColor(pair.Value, selected ? new Color(0.32f, 0.29f, 0.18f, 1f) : new Color(0.18f, 0.24f, 0.38f, 1f));

            Text text = pair.Value.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = selected ? "[" + pair.Key + "x]" : pair.Key + "x";
                text.color = selected ? new Color(1f, 0.91f, 0.40f, 1f) : Color.white;
            }
        }

        bool hasGrowthAttention = false;
        foreach (AbilityState ability in abilityManager.States)
        {
            if (abilityButtonTexts.TryGetValue(ability.Definition.Kind, out Text text))
            {
                int cappedLevels = abilityManager.GetCappedLevelCount(ability, selectedGrowthLevelStep);
                long selectedCost = abilityManager.GetLevelUpCost(ability, cappedLevels);
                bool canBuySelected = !ability.IsMaxed && cappedLevels > 0 && selectedCost > 0 && selectedCost <= wallet.Gold;
                bool canBuyOne = !ability.IsMaxed && ability.LevelUpCost > 0 && ability.LevelUpCost <= wallet.Gold;
                hasGrowthAttention |= canBuyOne;
                SetNotificationDot(abilityNotificationDots, ability.Definition.Kind, canBuySelected);

                string costText = ability.IsMaxed ? "MAX" : "G " + FormatShortNumber(selectedCost);
                string levelText = ability.IsMaxed ? "MAX" : "Lv." + ability.Level + "/" + ability.Definition.MaxLevel;
                text.text = ability.Definition.DisplayName
                    + "  " + levelText
                    + "\n" + abilityManager.GetDisplayValue(ability);

                Button rowButton = text.GetComponentInParent<Button>();
                if (rowButton != null)
                {
                    SetButtonColor(rowButton, ability.IsMaxed
                        ? new Color(0.26f, 0.30f, 0.39f, 1f)
                        : canBuySelected ? new Color(0.48f, 0.54f, 0.66f, 1f) : new Color(0.24f, 0.26f, 0.30f, 1f));
                }

                if (abilityCostBadgeTexts.TryGetValue(ability.Definition.Kind, out Text costBadgeText))
                {
                    costBadgeText.text = costText;
                    costBadgeText.color = ability.IsMaxed ? new Color(1f, 0.88f, 0.24f, 1f) : new Color(0.04f, 0.06f, 0.05f, 1f);
                    Image badgeImage = costBadgeText.GetComponentInParent<Image>();
                    if (badgeImage != null)
                    {
                        badgeImage.color = ability.IsMaxed
                            ? new Color(0.14f, 0.19f, 0.31f, 1f)
                            : canBuySelected ? new Color(0.56f, 0.88f, 0.24f, 1f) : new Color(0.20f, 0.24f, 0.33f, 1f);
                    }
                }
            }
        }

        RefreshHeroFormationPanel();

        bool hasHeroAttention = false;
        foreach (HeroState hero in battleManager.Heroes)
        {
            if (heroButtonTexts.TryGetValue(hero.Definition.Id, out Text text))
            {
                bool canLevel = hero.LevelUpCost <= wallet.HeroExpItem;
                bool needsAttention = hero.CanStarUp || canLevel;
                hasHeroAttention |= needsAttention;
                SetNotificationDot(heroNotificationDots, hero.Definition.Id, needsAttention);

                string starCostText = hero.IsMaxStars
                    ? "S MAX"
                    : "S " + hero.Shards + "/" + hero.StarUpCost;
                bool isDeployed = IsHeroInEditingFormation(hero.Definition.Id);
                bool isSelectedForPlacement = selectedHeroForPlacement == hero.Definition.Id;
                string actionText = isSelectedForPlacement ? "선택됨" : "대기";

                text.text = (isDeployed ? "배치중 " : string.Empty)
                    + GetTraitBadge(hero.Definition.Trait)
                    + " " + GetRarityBadge(hero.Definition.Rarity)
                    + " Lv." + hero.Level
                    + "\n" + GetShortHeroLabel(hero.Definition)
                    + "  " + hero.Definition.RarityLabel + "  " + FormatStars(hero.Stars)
                    + "\n공 " + FormatShortNumber(hero.AttackPower) + "  체 " + FormatShortNumber(hero.MaxHp)
                    + "\n속 " + hero.AttackSpeed.ToString("0.##") + "  이 " + hero.MoveSpeed.ToString("0.#")
                    + "\n" + hero.Definition.PassiveLabel + "  " + starCostText + (isDeployed ? string.Empty : "  " + actionText);

                Button heroButton = text.GetComponentInParent<Button>();
                if (heroButton != null)
                {
                    SetButtonColor(heroButton, isDeployed
                        ? new Color(0.13f, 0.15f, 0.18f, 1f)
                        : isSelectedForPlacement ? new Color(0.55f, 0.49f, 0.20f, 1f) : GetRarityColor(hero.Definition.Rarity));
                }

                if (heroRosterActionButtons.TryGetValue(hero.Definition.Id, out Button actionButton))
                {
                    Text actionTextComponent = actionButton.GetComponentInChildren<Text>(true);
                    if (actionTextComponent != null)
                    {
                        actionTextComponent.text = isDeployed ? "-" : "+";
                    }

                    SetButtonColor(actionButton, isDeployed
                        ? new Color(0.58f, 0.12f, 0.12f, 1f)
                        : new Color(0.88f, 0.72f, 0.20f, 1f));
                }
            }
        }

        if (supportSummaryText != null)
        {
            supportSummaryText.text = "Party ATK " + FormatShortNumber(battleManager.PartyAttackPower)
                + "    Pet Gold +" + battleManager.PetGoldBonusPercent.ToString("0.#") + "%";
        }

        foreach (CombatSkillState skill in battleManager.Skills)
        {
            if (skillStatusTexts.TryGetValue(skill.Definition.Id, out Text text))
            {
                double projectedDamage = battleManager.PartyAttackPower * skill.Definition.PartyAttackMultiplier;
                text.text = skill.Definition.DisplayName
                    + "    Cooldown " + Mathf.CeilToInt(skill.CooldownRemaining) + "s"
                    + "\nDamage " + FormatShortNumber(projectedDamage)
                    + "    Party ATK x" + skill.Definition.PartyAttackMultiplier.ToString("0.0");
            }
        }

        foreach (PetState pet in battleManager.Pets)
        {
            if (petStatusTexts.TryGetValue(pet.Definition.Id, out Text text))
            {
                text.text = pet.Definition.DisplayName
                    + "    Next " + Mathf.CeilToInt(pet.AttackCooldown) + "s"
                    + "\nATK " + FormatShortNumber(pet.Definition.AttackPower)
                    + "    Interval " + pet.Definition.AttackInterval.ToString("0.0") + "s"
                    + "    Gold +" + battleManager.PetGoldBonusPercent.ToString("0.#") + "%";
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

        foreach (KeyValuePair<int, Button> pair in speedButtons)
        {
            int multiplier = pair.Key;
            Button button = pair.Value;
            bool canUse = speedManager.CanUseSpeed(multiplier);
            button.interactable = canUse;

            Text text = button.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                string label = multiplier + "x";
                if (multiplier == GameSpeedManager.PremiumSpeed && !canUse)
                {
                    label += " Locked";
                }
                else if (multiplier == speedManager.CurrentMultiplier)
                {
                    label = "[" + label + "]";
                }

                text.text = label;
            }
        }

        if (debugText != null)
        {
            debugText.text = "Time Scale x" + Time.timeScale.ToString("0.##")
                + "\nCombat Speed x" + speedManager.CurrentMultiplier
                + "\n4x Entitlement: " + speedManager.HasFourTimesSpeedEntitlement
                + "\nOffline Reward Stage: " + progressManager.GetOfflineRewardStageId()
                + "\nBoss Cleared: " + progressManager.ChapterOneBossCleared
                + "\nLast Battle: " + battleManager.LastBattleLog;
        }

        bool hasSummonAttention = wallet.HeroSummonTicket > 0 || wallet.Ruby >= 150;
        bool hasStageAttention = progressManager.Mode == ProgressMode.BossBlocked;
        if (guideQuestDot != null)
        {
            guideQuestDot.SetActive(hasStageAttention);
        }

        bool hasSupportAttention = false;
        foreach (CombatSkillState skill in battleManager.Skills)
        {
            if (skill.CooldownRemaining <= 0.5f)
            {
                hasSupportAttention = true;
                break;
            }
        }

        SetTabNotificationDots(HudTab.Growth, hasGrowthAttention);
        SetTabNotificationDots(HudTab.Hero, hasHeroAttention);
        SetTabNotificationDots(HudTab.Summon, hasSummonAttention);
        SetTabNotificationDots(HudTab.Stage, hasStageAttention);
        SetTabNotificationDots(HudTab.Shop, false);
        SetTabNotificationDots(HudTab.Support, hasSupportAttention);
        SetTabNotificationDots(HudTab.Debug, false);

        if (battleLayoutElement != null)
        {
            battleLayoutElement.preferredHeight = contentPanelOpen ? 870f : 1630f;
        }

        if (contentLayoutElement != null)
        {
            contentLayoutElement.preferredHeight = contentPanelOpen ? 760f : 0f;
        }

        if (contentRoot != null)
        {
            contentRoot.SetActive(contentPanelOpen);
        }

        growthPanel.SetActive(contentPanelOpen && activeTab == HudTab.Growth);
        heroPanel.SetActive(contentPanelOpen && activeTab == HudTab.Hero);
        stagePanel.SetActive(contentPanelOpen && activeTab == HudTab.Stage);
        summonPanel.SetActive(contentPanelOpen && activeTab == HudTab.Summon);
        shopPanel.SetActive(contentPanelOpen && activeTab == HudTab.Shop);
        supportPanel.SetActive(contentPanelOpen && activeTab == HudTab.Support);
        if (debugPanel != null)
        {
            debugPanel.SetActive(contentPanelOpen && activeTab == HudTab.Debug);
        }

        if (heroFormationSavePrompt != null)
        {
            heroFormationSavePrompt.SetActive(heroFormationSavePromptOpen);
        }

        foreach (KeyValuePair<HudTab, Button> pair in tabButtons)
        {
            Text text = pair.Value.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                bool activeAndOpen = contentPanelOpen && pair.Key == activeTab;
                string label = tabButtonLabels.TryGetValue(pair.Key, out string savedLabel) ? savedLabel : text.text;
                text.text = pair.Key == HudTab.Growth && activeAndOpen ? "X\n성장" : label;
                text.color = activeAndOpen ? new Color(1f, 0.91f, 0.40f, 1f) : Color.white;
            }
        }
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = Mathf.Max(0.1f, scale);
        UpdateView();
    }

    private void LoadHeroFormationDraftFromPreset(int preset)
    {
        selectedHeroPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        selectedHeroForPlacement = string.Empty;
        editingFormationHeroIds.Clear();

        IReadOnlyList<string> savedIds = battleManager.GetHeroFormationHeroIds(selectedHeroPreset);
        for (int i = 0; i < GameData.MaxPartyHeroes; i++)
        {
            editingFormationHeroIds.Add(i < savedIds.Count ? savedIds[i] : string.Empty);
        }

        heroFormationDirty = false;
    }

    private void EnsureHeroFormationDraft()
    {
        if (editingFormationHeroIds.Count == GameData.MaxPartyHeroes)
        {
            return;
        }

        LoadHeroFormationDraftFromPreset(battleManager.ActiveHeroPreset);
    }

    private bool HasHeroFormationPendingChanges()
    {
        EnsureHeroFormationDraft();

        IReadOnlyList<string> savedIds = battleManager.GetHeroFormationHeroIds(selectedHeroPreset);
        for (int i = 0; i < GameData.MaxPartyHeroes; i++)
        {
            string editingHeroId = i < editingFormationHeroIds.Count ? editingFormationHeroIds[i] : string.Empty;
            string savedHeroId = i < savedIds.Count ? savedIds[i] : string.Empty;
            if (editingHeroId != savedHeroId)
            {
                return true;
            }
        }

        if (heroFormationDirty)
        {
            return true;
        }

        return selectedHeroPreset != battleManager.ActiveHeroPreset && GetEditingFormationFilledCount() > 0;
    }

    private void RequestHeroPresetChange(int preset)
    {
        int nextPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        if (nextPreset == selectedHeroPreset)
        {
            return;
        }

        if (heroFormationDirty)
        {
            ShowHeroFormationSavePromptForPreset(nextPreset);
            return;
        }

        LoadHeroFormationDraftFromPreset(nextPreset);
        UpdateView();
    }

    private void RefreshHeroFormationPanel()
    {
        EnsureHeroFormationDraft();
        bool formationOpen = activeHeroPageTab == HeroPageTab.Formation;
        if (heroFormationContent != null)
        {
            heroFormationContent.SetActive(formationOpen);
        }

        if (heroPlaceholderText != null)
        {
            heroPlaceholderText.gameObject.SetActive(!formationOpen);
            heroPlaceholderText.text = GetHeroPageTabLabel(activeHeroPageTab) + " 준비 중";
        }

        foreach (KeyValuePair<HeroPageTab, Button> pair in heroPageTabButtons)
        {
            SetButtonColor(pair.Value, pair.Key == activeHeroPageTab
                ? new Color(0.42f, 0.54f, 0.82f, 1f)
                : new Color(0.18f, 0.24f, 0.38f, 1f));
        }

        foreach (KeyValuePair<int, Button> pair in heroPresetButtons)
        {
            SetButtonColor(pair.Value, pair.Key == selectedHeroPreset
                ? new Color(0.50f, 0.64f, 0.96f, 1f)
                : new Color(0.21f, 0.29f, 0.45f, 1f));
        }

        int deployedCount = Mathf.Min(GameData.MaxPartyHeroes, GetEditingFormationFilledCount());
        if (heroFormationSummaryText != null)
        {
            heroFormationSummaryText.text = "출전 " + deployedCount + "/" + GameData.MaxPartyHeroes
                + "    프리셋 " + selectedHeroPreset
                + (HasHeroFormationPendingChanges() ? "    저장 필요" : string.Empty)
                + (string.IsNullOrEmpty(selectedHeroForPlacement)
                    ? string.Empty
                    : "    배치 선택: " + GameData.GetHero(selectedHeroForPlacement).DisplayName);
        }

        for (int i = 0; i < heroFormationSlotTexts.Count; i++)
        {
            Text slotText = heroFormationSlotTexts[i];
            string heroId = i < editingFormationHeroIds.Count ? editingFormationHeroIds[i] : string.Empty;
            HeroState hero = FindHeroState(heroId);
            bool occupied = hero != null;
            if (heroFormationSlotButtons.TryGetValue(i, out Button slotButton))
            {
                SetButtonColor(slotButton, occupied ? GetRarityColor(hero.Definition.Rarity) : new Color(0.18f, 0.22f, 0.31f, 1f));
            }

            if (heroFormationSlotRemoveButtons.TryGetValue(i, out Button removeButton))
            {
                removeButton.gameObject.SetActive(occupied);
            }

            if (occupied)
            {
                slotText.text = GetTraitBadge(hero.Definition.Trait)
                    + " " + GetRarityBadge(hero.Definition.Rarity)
                    + " Lv." + hero.Level
                    + "\n" + GetShortHeroLabel(hero.Definition) + "  " + FormatStars(hero.Stars)
                    + "\n공 " + FormatShortNumber(hero.AttackPower) + " 체 " + FormatShortNumber(hero.MaxHp);
                slotText.color = Color.white;
            }
            else
            {
                slotText.text = "빈 슬롯";
                slotText.color = new Color(0.64f, 0.70f, 0.78f, 1f);
            }
        }

        if (heroOwnedEffectText != null)
        {
            int totalStars = 0;
            foreach (HeroState hero in battleManager.Heroes)
            {
                totalStars += hero.Stars;
            }

            heroOwnedEffectText.text = "보유 효과 : 공격력+" + (battleManager.Heroes.Count * 10 + totalStars * 2) + "%";
        }
    }

    private void TryLevelUpAbilityFromHud(AbilityKind kind)
    {
        AbilityState ability = FindAbilityState(kind);
        if (ability == null)
        {
            return;
        }

        int cappedLevels = abilityManager.GetCappedLevelCount(ability, selectedGrowthLevelStep);
        long cost = abilityManager.GetLevelUpCost(ability, cappedLevels);
        if (ability.IsMaxed)
        {
            ShowGrowthNotice("이미 최대 레벨입니다.");
            return;
        }

        if (cappedLevels <= 0 || cost <= 0 || wallet.Gold < cost)
        {
            ShowGrowthNotice("골드가 부족합니다.");
            return;
        }

        if (!abilityManager.TryLevelUp(kind, selectedGrowthLevelStep))
        {
            ShowGrowthNotice("골드가 부족합니다.");
        }
    }

    private bool IsHeroInEditingFormation(string heroId)
    {
        EnsureHeroFormationDraft();
        foreach (string editingHeroId in editingFormationHeroIds)
        {
            if (editingHeroId == heroId)
            {
                return true;
            }
        }

        return false;
    }

    private int GetEditingFormationHeroIndex(string heroId)
    {
        EnsureHeroFormationDraft();
        for (int i = 0; i < editingFormationHeroIds.Count; i++)
        {
            if (editingFormationHeroIds[i] == heroId)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetEditingFormationFilledCount()
    {
        EnsureHeroFormationDraft();
        int count = 0;
        foreach (string heroId in editingFormationHeroIds)
        {
            if (!string.IsNullOrEmpty(heroId))
            {
                count += 1;
            }
        }

        return count;
    }

    private void SelectOrRemoveRosterHero(string heroId)
    {
        HeroState hero = FindHeroState(heroId);
        if (hero == null)
        {
            return;
        }

        int existingIndex = GetEditingFormationHeroIndex(heroId);
        if (existingIndex >= 0)
        {
            if (selectedHeroForPlacement == heroId)
            {
                selectedHeroForPlacement = string.Empty;
            }

            RemoveHeroFromEditingFormationSlot(existingIndex);
            return;
        }

        selectedHeroForPlacement = selectedHeroForPlacement == heroId ? string.Empty : heroId;
        UpdateView();
    }

    private void RemoveHeroFromEditingFormationSlot(int slotIndex)
    {
        EnsureHeroFormationDraft();
        if (slotIndex < 0 || slotIndex >= editingFormationHeroIds.Count || string.IsNullOrEmpty(editingFormationHeroIds[slotIndex]))
        {
            return;
        }

        if (GetEditingFormationFilledCount() <= 1)
        {
            return;
        }

        editingFormationHeroIds[slotIndex] = string.Empty;
        selectedHeroForPlacement = string.Empty;
        heroFormationDirty = true;
        UpdateView();
    }

    private void TryPlaceSelectedHeroInSlot(int slotIndex)
    {
        if (string.IsNullOrEmpty(selectedHeroForPlacement))
        {
            return;
        }

        EnsureHeroFormationDraft();
        if (slotIndex < 0 || slotIndex >= editingFormationHeroIds.Count)
        {
            return;
        }

        string heroId = selectedHeroForPlacement;
        for (int i = 0; i < editingFormationHeroIds.Count; i++)
        {
            if (editingFormationHeroIds[i] == heroId)
            {
                editingFormationHeroIds[i] = string.Empty;
            }
        }

        editingFormationHeroIds[slotIndex] = heroId;
        selectedHeroForPlacement = string.Empty;
        heroFormationDirty = true;
        UpdateView();
    }

    private HeroState FindHeroState(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return null;
        }

        foreach (HeroState hero in battleManager.Heroes)
        {
            if (hero.Definition.Id == heroId)
            {
                return hero;
            }
        }

        return null;
    }

    private AbilityState FindAbilityState(AbilityKind kind)
    {
        foreach (AbilityState ability in abilityManager.States)
        {
            if (ability.Definition.Kind == kind)
            {
                return ability;
            }
        }

        return null;
    }

    private void ShowGrowthNotice(string message)
    {
        growthNoticeMessage = message;
        growthNoticeUntil = Time.unscaledTime + 1.6f;
        if (growthNoticeText != null)
        {
            growthNoticeText.text = message;
        }
    }

    private void RegisterTabNotificationDot(HudTab tab, GameObject dot)
    {
        if (!tabNotificationDots.TryGetValue(tab, out List<GameObject> dots))
        {
            dots = new List<GameObject>();
            tabNotificationDots[tab] = dots;
        }

        dots.Add(dot);
        dot.SetActive(false);
    }

    private void SetTabNotificationDots(HudTab tab, bool visible)
    {
        if (!tabNotificationDots.TryGetValue(tab, out List<GameObject> dots))
        {
            return;
        }

        foreach (GameObject dot in dots)
        {
            if (dot != null)
            {
                dot.SetActive(visible);
            }
        }
    }

    private void SetNotificationDot<TKey>(Dictionary<TKey, GameObject> dots, TKey key, bool visible)
    {
        if (dots.TryGetValue(key, out GameObject dot) && dot != null)
        {
            dot.SetActive(visible);
        }
    }

    private void RefreshBattlefieldVisuals()
    {
        if (battleManager == null || damagePopupText == null || battlefieldRect == null)
        {
            return;
        }

        bool flashActive = hitFlashRemaining > 0f;
        float flashRatio = hitFlashRemaining / 0.28f;
        float time = Time.time;
        float fieldWidth = Mathf.Max(760f, battlefieldRect.rect.width);
        float fieldHeight = Mathf.Max(260f, battlefieldRect.rect.height);
        Vector2[] heroFormation =
        {
            new Vector2(0f, -16f),
            new Vector2(-86f, -26f),
            new Vector2(86f, -26f),
            new Vector2(-48f, 56f),
            new Vector2(48f, 56f),
            new Vector2(-134f, 22f),
            new Vector2(134f, 22f),
            new Vector2(0f, 104f)
        };

        foreach (RectTransform heroRect in heroBattleRects.Values)
        {
            heroRect.localScale = Vector3.zero;
        }

        int heroIndex = 0;
        foreach (HeroState hero in battleManager.DeployedHeroes)
        {
            if (!heroBattleImages.TryGetValue(hero.Definition.Id, out Image image))
            {
                continue;
            }

            bool isLastSource = battleManager.LastHitSourceName == hero.Definition.DisplayName && flashActive;
            float movementTempo = 3.2f + hero.MoveSpeed * 0.45f;
            float bob = Mathf.Sin(time * movementTempo + heroIndex * 0.7f) * (4f + hero.MoveSpeed);
            float attackLift = isLastSource ? 28f * flashRatio : 0f;
            if (heroBattleRects.TryGetValue(hero.Definition.Id, out RectTransform heroRect))
            {
                Vector2 formationPosition = heroFormation[heroIndex % heroFormation.Length];
                heroRect.anchoredPosition = formationPosition + new Vector2(0f, bob + attackLift);
                heroRect.localScale = Vector3.one * (isLastSource ? 1f + 0.18f * flashRatio : 1f);
            }

            Color baseColor = GetRarityColor(hero.Definition.Rarity);
            image.color = isLastSource
                ? Color.Lerp(baseColor, new Color(1f, 0.86f, 0.22f, 1f), flashRatio)
                : baseColor;

            if (heroBattleTexts.TryGetValue(hero.Definition.Id, out Text text))
            {
                text.text = GetRarityBadge(hero.Definition.Rarity) + "\n" + GetShortHeroLabel(hero.Definition);
            }

            heroIndex += 1;
        }

        if (centerSpawnText != null)
        {
            RectTransform portalRect = centerSpawnText.GetComponent<RectTransform>();
            float pulse = 1f + Mathf.Sin(time * 5.6f) * 0.13f;
            portalRect.localScale = new Vector3(pulse, pulse, 1f);
            centerSpawnText.text = "◎";
            centerSpawnText.color = battleManager.IsBossFight
                ? new Color(1f, 0.70f, 0.16f, 0.72f)
                : new Color(0.35f, 0.72f, 1f, 0.42f + Mathf.Sin(time * 3.2f) * 0.12f);
        }

        int visible = Mathf.Clamp(battleManager.VisibleEnemyCount, 0, enemyBattleImages.Count);
        int firstIndex = battleManager.KillsThisStage % Mathf.Max(1, enemyBattleImages.Count);
        for (int i = 0; i < enemyBattleImages.Count; i++)
        {
            bool active = i < visible;
            int shiftedIndex = (i + firstIndex) % enemyBattleImages.Count;
            Image image = enemyBattleImages[shiftedIndex];
            Text text = enemyBattleTexts[shiftedIndex];
            RectTransform enemyRect = enemyBattleRects[shiftedIndex];

            if (!active)
            {
                enemyRect.anchoredPosition = Vector2.zero;
                enemyRect.localScale = Vector3.zero;
                image.color = new Color(0.13f, 0.10f, 0.10f, 0f);
                text.text = string.Empty;
                continue;
            }

            bool frontTarget = i == 0 && flashActive;
            if (battleManager.IsBossFight)
            {
                enemyRect.anchoredPosition = new Vector2(
                    Mathf.Sin(time * 1.4f) * 18f,
                    Mathf.Cos(time * 1.1f) * 10f + fieldHeight * 0.18f);
                enemyRect.localScale = Vector3.one * (frontTarget ? 1.65f + 0.18f * flashRatio : 1.52f);
            }
            else
            {
                Vector2 direction = GetEnemySpreadDirection(i);
                float cycle = Mathf.Repeat(time * (0.18f + speedManager.CurrentMultiplier * 0.035f) + i * 0.11f + battleManager.KillsThisStage * 0.013f, 1f);
                float approach = Mathf.SmoothStep(0f, 1f, cycle);
                float spawnDistance = Mathf.Max(fieldWidth * 0.58f, fieldHeight * 0.58f);
                float targetDistance = 112f + 18f * (i % 3);
                float distance = Mathf.Lerp(spawnDistance, targetDistance, approach);
                Vector2 drift = new Vector2(-direction.y, direction.x) * Mathf.Sin(time * 2.3f + i) * 11f;
                enemyRect.anchoredPosition = direction * distance + drift;
                enemyRect.localScale = Vector3.one * (0.68f + 0.30f * approach + (frontTarget ? 0.18f * flashRatio : 0f));
            }

            if (battleManager.IsBossFight)
            {
                image.color = frontTarget
                    ? new Color(1f, 0.34f, 0.22f, 1f)
                    : new Color(0.62f, 0.12f, 0.10f, 1f);
                text.text = "BOSS";
            }
            else
            {
                image.color = frontTarget
                    ? new Color(1f, 0.48f, 0.24f, 1f)
                    : new Color(0.52f, 0.16f + 0.03f * (i % 3), 0.12f, 1f);
                text.text = "M" + (battleManager.KillsThisStage + i + 1);
            }
        }

        if (battleManager.HitSequence <= 0)
        {
            damagePopupText.text = "READY";
            damagePopupText.color = new Color(0.72f, 0.78f, 0.86f, 1f);
            return;
        }

        damagePopupText.text = battleManager.LastHitSourceName
            + "\n-" + FormatShortNumber(battleManager.LastHitDamage)
            + (battleManager.LastHitWasCritical ? " CRIT" : string.Empty);
        damagePopupText.color = battleManager.LastHitWasCritical
            ? new Color(1f, 0.91f, 0.24f, 1f)
            : new Color(1f, 0.55f, 0.32f, 1f);

        RectTransform damageRect = damagePopupText.GetComponent<RectTransform>();
        damageRect.anchoredPosition = new Vector2(0f, 24f + 40f * flashRatio);
        damageRect.localScale = Vector3.one * (1f + 0.25f * flashRatio);
    }

    private string GetShortHeroLabel(HeroDefinition hero)
    {
        if (hero.DisplayName.Length <= 2)
        {
            return hero.DisplayName;
        }

        return hero.DisplayName.Substring(hero.DisplayName.Length - 2);
    }

    private bool IsDebugPanelEnabled()
    {
        return Application.isEditor || Debug.isDebugBuild;
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

    private string GetHeroPageTabLabel(HeroPageTab tab)
    {
        switch (tab)
        {
            case HeroPageTab.Formation:
                return "편성";
            case HeroPageTab.Trait:
                return "특성";
            case HeroPageTab.Statue:
                return "석상";
            case HeroPageTab.Seal:
                return "인장";
            case HeroPageTab.Relic:
                return "유물";
            default:
                return tab.ToString();
        }
    }

    private GameObject CreateNotificationDot(Transform parent, float size, Vector2 anchoredPosition)
    {
        Text dot = CreateText("RedDot", parent, Mathf.RoundToInt(size), FontStyle.Bold, TextAnchor.MiddleCenter);
        dot.text = "●";
        dot.color = new Color(1f, 0.04f, 0.04f, 1f);
        dot.raycastTarget = false;

        RectTransform rect = dot.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = anchoredPosition;

        dot.gameObject.SetActive(false);
        return dot.gameObject;
    }

    private string GetRarityBadge(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Uncommon:
                return "UC";
            case HeroRarity.Rare:
                return "R";
            case HeroRarity.Epic:
                return "E";
            case HeroRarity.Legendary:
                return "L";
            case HeroRarity.Mythic:
                return "M";
            default:
                return "?";
        }
    }

    private string GetTraitBadge(HeroTrait trait)
    {
        switch (trait)
        {
            case HeroTrait.Melee:
                return "근";
            case HeroTrait.Ranged:
                return "원";
            case HeroTrait.Support:
                return "지";
            case HeroTrait.Defense:
                return "방";
            default:
                return "?";
        }
    }

    private string FormatStars(int stars)
    {
        int clampedStars = Mathf.Clamp(stars, 0, HeroDefinition.MaxStars);
        if (clampedStars <= 0)
        {
            return "<color=#6F778A>☆☆☆☆☆</color>";
        }

        string result = string.Empty;
        for (int i = 1; i <= clampedStars; i++)
        {
            string color = i <= 5 ? "#FFD84D" : i <= 10 ? "#51A7FF" : "#C15CFF";
            result += "<color=" + color + ">★</color>";
        }

        return result;
    }

    private Color GetRarityColor(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Uncommon:
                return new Color(0.18f, 0.36f, 0.25f, 1f);
            case HeroRarity.Rare:
                return new Color(0.16f, 0.32f, 0.58f, 1f);
            case HeroRarity.Epic:
                return new Color(0.44f, 0.20f, 0.62f, 1f);
            case HeroRarity.Legendary:
                return new Color(0.76f, 0.47f, 0.12f, 1f);
            case HeroRarity.Mythic:
                return new Color(0.64f, 0.16f, 0.18f, 1f);
            default:
                return new Color(0.16f, 0.24f, 0.34f, 1f);
        }
    }

    private Vector2 GetEnemySpreadDirection(int index)
    {
        float angle = index * 137.5f * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        return direction.sqrMagnitude <= 0.001f ? Vector2.right : direction.normalized;
    }

    private string FormatShortNumber(double value)
    {
        return NumberFormatter.Format(value);
    }

    private GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private GameObject CreateBattleActor(string name, Transform parent, Vector2 size, Color color)
    {
        GameObject actor = CreatePanel(name, parent, color);
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

    private Button CreateButton(string label, Transform parent, int fontSize, Color color)
    {
        GameObject buttonObject = CreatePanel(label + "Button", parent, color);
        Button button = buttonObject.AddComponent<Button>();
        SetButtonColor(button, color);

        Text text = CreateText(label + "Text", buttonObject.transform, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 6);
        textRect.offsetMax = new Vector2(-10, -6);
        text.text = label;

        return button;
    }

    private void SetButtonColor(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.12f;
        colors.pressedColor = color * 0.84f;
        colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.55f);
        button.colors = colors;

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }
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
        text.supportRichText = true;
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

    private LayoutElement AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
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

        return element;
    }
}
