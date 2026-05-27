using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.UI.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace IdleGame.UI.Hud
{
    public sealed class GameHudBattleRefs
    {
        public GameHudBattleRefs(float enemyDeathVisualSeconds)
        {
            VisualState = new BattleHudVisualState(enemyDeathVisualSeconds);
        }

        public readonly BattleHudRenderer Renderer = new BattleHudRenderer();
        public readonly BattleHudVisualState VisualState;

        public Text StageText;
        public Text ModeText;
        public Text TargetText;
        public Text HpText;
        public Text ProgressText;
        public Text SupportText;
        public Text LogText;
        public Text RewardText;
        public Text DamagePopupText;
        public Text DamageMeterText;
        public Text CenterSpawnText;
        public Image HpFill;
        public RawImage BattlefieldWorldImage;
        public RectTransform BattlefieldRect;
        public LayoutElement LayoutElement;
        public GameObject Panel;

        public readonly Dictionary<string, Image> HeroImages = new Dictionary<string, Image>();
        public readonly Dictionary<string, Text> HeroTexts = new Dictionary<string, Text>();
        public readonly Dictionary<string, RectTransform> HeroRects = new Dictionary<string, RectTransform>();
        public readonly List<Image> EnemyImages = new List<Image>();
        public readonly List<Text> EnemyTexts = new List<Text>();
        public readonly List<RectTransform> EnemyRects = new List<RectTransform>();
        public readonly List<GameObject> EnemyHpBars = new List<GameObject>();
        public readonly List<Image> EnemyHpFills = new List<Image>();
        public readonly List<GameObject> DamageMeterRows = new List<GameObject>();
        public readonly List<Image> DamageMeterFills = new List<Image>();
        public readonly List<Text> DamageMeterRowTexts = new List<Text>();
        public readonly List<HeroState> DamageMeterHeroScratch = new List<HeroState>();
        public List<DamageMeterRowViewState> DamageMeterRowStates = new List<DamageMeterRowViewState>();

        public Button SkillAutoButton;
        public Button FeverAutoButton;
        public Button SpeedCycleButton;

        public void Reset()
        {
            StageText = null;
            ModeText = null;
            TargetText = null;
            HpText = null;
            ProgressText = null;
            SupportText = null;
            LogText = null;
            RewardText = null;
            DamagePopupText = null;
            DamageMeterText = null;
            CenterSpawnText = null;
            HpFill = null;
            BattlefieldWorldImage = null;
            BattlefieldRect = null;
            LayoutElement = null;
            Panel = null;
            SkillAutoButton = null;
            FeverAutoButton = null;
            SpeedCycleButton = null;

            Renderer.ResetRuntimeState();
            HeroImages.Clear();
            HeroTexts.Clear();
            HeroRects.Clear();
            EnemyImages.Clear();
            EnemyTexts.Clear();
            EnemyRects.Clear();
            EnemyHpBars.Clear();
            EnemyHpFills.Clear();
            VisualState.ResetAll();
            DamageMeterRows.Clear();
            DamageMeterFills.Clear();
            DamageMeterRowTexts.Clear();
            DamageMeterHeroScratch.Clear();
            DamageMeterRowStates.Clear();
        }
    }
}
