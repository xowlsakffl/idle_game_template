using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameBootstrap : MonoBehaviour
{
    private SaveManager saveManager;
    private StageProgressManager progressManager;
    private CurrencyWallet wallet;
    private AbilityManager abilityManager;
    private EquipmentInventory equipmentInventory;
    private GameSpeedManager speedManager;
    private BattleManager battleManager;
    private GachaManager gachaManager;
    [NonSerialized]
    private bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateRuntime()
    {
        if (FindAnyObjectByType<GameBootstrap>() != null)
        {
            return;
        }

        GameObject runtime = new GameObject("IdleGameRuntime");
        runtime.AddComponent<GameBootstrap>();
    }

    private void Awake()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.Portrait;

        saveManager = GetOrAddComponent<SaveManager>();
        progressManager = GetOrAddComponent<StageProgressManager>();
        wallet = GetOrAddComponent<CurrencyWallet>();
        abilityManager = GetOrAddComponent<AbilityManager>();
        equipmentInventory = GetOrAddComponent<EquipmentInventory>();
        speedManager = GetOrAddComponent<GameSpeedManager>();
        battleManager = GetOrAddComponent<BattleManager>();
        gachaManager = GetOrAddComponent<GachaManager>();

        progressManager.Initialize(saveManager);
        wallet.Initialize(saveManager);
        abilityManager.Initialize(wallet, saveManager);
        equipmentInventory.Initialize(saveManager);
        speedManager.Initialize(saveManager);
        battleManager.Initialize(progressManager, wallet, saveManager, abilityManager, speedManager);
        gachaManager.Initialize(battleManager, wallet, equipmentInventory);

        ApplyOfflineReward();

        GameHud hud = GetOrAddComponent<GameHud>();
        hud.Initialize(progressManager, wallet, abilityManager, speedManager, battleManager, gachaManager, equipmentInventory, DebugResetSaveAndReload);
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        if (TryGetComponent(out T component))
        {
            return component;
        }

        return gameObject.AddComponent<T>();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveExitTime();
        }
    }

    private void OnApplicationQuit()
    {
        SaveExitTime();
    }

    private void ApplyOfflineReward()
    {
        DateTime? lastOnlineUtc = saveManager.LoadLastOnlineUtc();
        if (!lastOnlineUtc.HasValue)
        {
            SaveExitTime();
            return;
        }

        long reward = CalculateOfflineGoldReward(
            lastOnlineUtc,
            DateTime.UtcNow,
            progressManager.GetOfflineRewardStageId());

        if (reward > 0)
        {
            wallet.AddGold(reward);
        }

        SaveExitTime();
    }

    public static long CalculateOfflineGoldReward(DateTime? lastOnlineUtc, DateTime currentUtc, string offlineRewardStageId)
    {
        if (!lastOnlineUtc.HasValue)
        {
            return 0;
        }

        double elapsedSeconds = (currentUtc.ToUniversalTime() - lastOnlineUtc.Value.ToUniversalTime()).TotalSeconds;
        if (elapsedSeconds <= 10)
        {
            return 0;
        }

        double cappedSeconds = Math.Min(elapsedSeconds, 28800);
        float goldPerSecond = GameData.GetOfflineGoldPerSecond(offlineRewardStageId);
        return (long)Math.Floor(cappedSeconds * goldPerSecond);
    }

    private void SaveExitTime()
    {
        if (saveManager == null)
        {
            return;
        }

        saveManager.SaveLastOnlineUtc(DateTime.UtcNow);
        saveManager.Flush();
    }

    private void DebugResetSaveAndReload()
    {
        if (saveManager != null)
        {
            saveManager.ResetAll();
        }

        Time.timeScale = 1f;

        string sceneName = SceneManager.GetActiveScene().name;
        Destroy(gameObject);

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
