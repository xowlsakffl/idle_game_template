using System;
using UnityEngine;

public sealed class GameBootstrap : MonoBehaviour
{
    private SaveManager saveManager;
    private StageProgressManager progressManager;
    private CurrencyWallet wallet;
    private BattleManager battleManager;
    private GachaManager gachaManager;
    private bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateRuntime()
    {
        if (FindObjectOfType<GameBootstrap>() != null)
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

        saveManager = gameObject.AddComponent<SaveManager>();
        progressManager = gameObject.AddComponent<StageProgressManager>();
        wallet = gameObject.AddComponent<CurrencyWallet>();
        battleManager = gameObject.AddComponent<BattleManager>();
        gachaManager = gameObject.AddComponent<GachaManager>();

        progressManager.Initialize(saveManager);
        wallet.Initialize(saveManager);
        battleManager.Initialize(progressManager, wallet, saveManager);
        gachaManager.Initialize(battleManager);

        ApplyOfflineReward();

        GameHud hud = gameObject.AddComponent<GameHud>();
        hud.Initialize(progressManager, wallet, battleManager, gachaManager);
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

        double elapsedSeconds = (DateTime.UtcNow - lastOnlineUtc.Value).TotalSeconds;
        if (elapsedSeconds <= 10)
        {
            SaveExitTime();
            return;
        }

        double cappedSeconds = Math.Min(elapsedSeconds, 28800);
        float goldPerSecond = GameData.GetOfflineGoldPerSecond(progressManager.GetOfflineRewardStageId());
        long reward = (long)Math.Floor(cappedSeconds * goldPerSecond);

        if (reward > 0)
        {
            wallet.AddGold(reward);
        }

        SaveExitTime();
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
}
