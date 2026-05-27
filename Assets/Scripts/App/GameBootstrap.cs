using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Battlefield;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Gacha;
using IdleGame.Progression;
using IdleGame.Save;
using IdleGame.Speed;
using IdleGame.UI.Hud;

namespace IdleGame.App
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private SaveManager saveManager;
        private StageProgressManager progressManager;
        private CurrencyWallet wallet;
        private AccountProgressManager accountProgressManager;
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
            accountProgressManager = GetOrAddComponent<AccountProgressManager>();
            abilityManager = GetOrAddComponent<AbilityManager>();
            equipmentInventory = GetOrAddComponent<EquipmentInventory>();
            speedManager = GetOrAddComponent<GameSpeedManager>();
            battleManager = GetOrAddComponent<BattleManager>();
            gachaManager = GetOrAddComponent<GachaManager>();
            BattlefieldWorldView battlefieldWorldView = GetOrAddComponent<BattlefieldWorldView>();

            progressManager.Initialize(saveManager);
            wallet.Initialize(saveManager);
            accountProgressManager.Initialize(saveManager);
            abilityManager.Initialize(wallet, saveManager);
            equipmentInventory.Initialize(saveManager);
            speedManager.Initialize(saveManager);
            battleManager.Initialize(progressManager, wallet, saveManager, abilityManager, speedManager, accountProgressManager);
            gachaManager.Initialize(battleManager, wallet, equipmentInventory);
            battlefieldWorldView.Initialize(battleManager, speedManager);

            ApplyOfflineReward();

            GameHud hud = GetOrAddComponent<GameHud>();
            hud.Initialize(progressManager, wallet, accountProgressManager, abilityManager, speedManager, battleManager, gachaManager, equipmentInventory, DebugResetSaveAndReload, battlefieldWorldView);
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

            GameNumber reward = CalculateOfflineGoldReward(
                lastOnlineUtc,
                DateTime.UtcNow,
                progressManager.GetOfflineRewardStageId());

            if (reward > GameNumber.Zero)
            {
                wallet.AddGold(reward);
            }

            SaveExitTime();
        }

        public static GameNumber CalculateOfflineGoldReward(DateTime? lastOnlineUtc, DateTime currentUtc, string offlineRewardStageId)
        {
            if (!lastOnlineUtc.HasValue)
            {
                return GameNumber.Zero;
            }

            double elapsedSeconds = (currentUtc.ToUniversalTime() - lastOnlineUtc.Value.ToUniversalTime()).TotalSeconds;
            if (elapsedSeconds <= 10)
            {
                return GameNumber.Zero;
            }

            double cappedSeconds = Math.Min(elapsedSeconds, 28800);
            GameNumber goldPerSecond = GameData.GetOfflineGoldPerSecond(offlineRewardStageId);
            return GameNumber.Floor(goldPerSecond * cappedSeconds);
        }

        private void SaveExitTime()
        {
            if (saveManager == null)
            {
                return;
            }

            saveManager.SaveLastOnlineUtc(DateTime.UtcNow);
            saveManager.FlushImmediate();
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
}
