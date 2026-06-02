using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;
using IdleGame.Save;
using IdleGame.Speed;

namespace IdleGame.Battle
{
    public sealed partial class BattleManager
    {
        public GameNumber GetHeroDamageDone(string heroId)
        {
            return BattleRuntimeQueryService.GetHeroDamageDone(heroDamageMeter, heroId);
        }

        public GameNumber GetMaxHeroDamageDone()
        {
            return BattleRuntimeQueryService.GetMaxHeroDamageDone(heroDamageMeter, deployedHeroes);
        }

        public float GetVisibleEnemyHpRatio(int visualIndex)
        {
            return BattleRuntimeQueryService.GetVisibleEnemyHpRatio(IsBossFight, TargetHp, TargetMaxHp, visibleEnemies, visualIndex);
        }

        public int GetVisibleEnemyDisplayNumber(int visualIndex)
        {
            return BattleRuntimeQueryService.GetVisibleEnemyDisplayNumber(IsBossFight, KillsThisStage, visibleEnemies, visualIndex);
        }

        public int GetVisibleEnemySpawnSequence(int visualIndex)
        {
            return BattleRuntimeQueryService.GetVisibleEnemySpawnSequence(IsBossFight, visibleEnemies, visualIndex);
        }

        public int GetHeroTargetVisualIndex(string heroId)
        {
            return BattleRuntimeQueryService.GetHeroTargetVisualIndex(IsBossFight, heroTargetSpawnSequences, visibleEnemies, heroId);
        }

        public int GetHeroTargetSpawnSequence(string heroId)
        {
            return BattleRuntimeQueryService.GetHeroTargetSpawnSequence(heroTargetSpawnSequences, heroId);
        }

        public Vector2 GetHeroBattlePosition(string heroId)
        {
            return BattleRuntimeQueryService.GetHeroBattlePosition(heroRuntimeStates, heroId);
        }

        public float GetHeroHpRatio(string heroId)
        {
            return BattleRuntimeQueryService.GetHeroHpRatio(heroRuntimeStates, heroId);
        }

        public bool IsHeroBattleAlive(string heroId)
        {
            return BattleRuntimeQueryService.IsHeroBattleAlive(heroRuntimeStates, heroId);
        }

        public Vector2 GetVisibleEnemyBattlePosition(int visualIndex)
        {
            return BattleRuntimeQueryService.GetVisibleEnemyBattlePosition(IsBossFight, visibleEnemies, visualIndex);
        }
    }
}
