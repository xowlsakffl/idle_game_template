using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Save;

namespace IdleGame.Gacha
{
    public sealed class GachaManager : MonoBehaviour
    {
        private readonly System.Random random = new System.Random();
        private readonly Dictionary<GachaPoolKind, int> totalPullsByPool = new Dictionary<GachaPoolKind, int>();
        private readonly Dictionary<string, int> pityCountByPool = new Dictionary<string, int>();
        private readonly List<GachaRollOutcome> lastOutcomes = new List<GachaRollOutcome>(10);

        private BattleManager battleManager;
        private CurrencyWallet wallet;
        private EquipmentInventory equipmentInventory;

        public event Action Changed;

        public string LastResult { get; private set; } = "소환 대기";
        public IReadOnlyList<GachaRollOutcome> LastOutcomes => lastOutcomes;
        public int ResultSequence { get; private set; }
        public IReadOnlyList<GachaPoolDefinition> Pools => GachaPoolDefinitions.All;
        public static IReadOnlyList<GachaRarityRate> Rates => GachaRateTable.GetRates(1);

        public void Initialize(BattleManager battle, CurrencyWallet currency, EquipmentInventory equipment)
        {
            battleManager = battle;
            wallet = currency;
            equipmentInventory = equipment;
            LoadProgress();
        }

        public void Roll(int count)
        {
            Roll(GachaPoolKind.Hero, count);
        }

        public void RollHeroes(int count)
        {
            Roll(GachaPoolKind.Hero, count);
        }

        public void RollEquipment(int count)
        {
            Roll(GachaPoolKind.Equipment, count);
        }

        public void RollEventHeroes(int count)
        {
            Roll(GachaPoolKind.Event, count);
        }

        public void RollRunes(int count)
        {
            Roll(GachaPoolKind.Rune, count);
        }

        public void Roll(GachaPoolKind kind, int count)
        {
            Roll(kind, count, string.Empty);
        }

        public void Roll(GachaPoolKind kind, int count, string eventTargetId)
        {
            count = Mathf.Clamp(count, 1, 10);
            GachaPoolDefinition pool = GachaPoolDefinitions.Get(kind);
            if (!SpendCost(pool, count))
            {
                lastOutcomes.Clear();
                LastResult = pool.Title + " 실패: 재화 부족";
                Changed?.Invoke();
                return;
            }

            lastOutcomes.Clear();
            var result = new StringBuilder();
            GachaEventTargetDefinition eventTarget = kind == GachaPoolKind.Event
                ? GachaEventTargetDefinitions.Get(eventTargetId)
                : null;
            for (int i = 0; i < count; i++)
            {
                GachaRollOutcome outcome = RollOne(pool, eventTarget);
                GrantOutcome(outcome);
                lastOutcomes.Add(outcome);
                AppendOutcomeLine(result, outcome, i < count - 1);
            }

            SavePoolProgress(pool, eventTarget);
            LastResult = result.ToString();
            ResultSequence++;
            Changed?.Invoke();
        }

        public GachaPoolProgress GetProgress(GachaPoolKind kind)
        {
            return GetProgress(kind, string.Empty);
        }

        public GachaPoolProgress GetProgress(GachaPoolKind kind, string eventTargetId)
        {
            GachaPoolDefinition pool = GachaPoolDefinitions.Get(kind);
            if (kind == GachaPoolKind.Event)
            {
                GachaEventTargetDefinition eventTarget = GachaEventTargetDefinitions.Get(eventTargetId);
                bool hasPity = eventTarget != null && eventTarget.HasPity;
                int pityCount = hasPity ? GetPityCount(eventTarget.Id) : 0;
                return new GachaPoolProgress(pool, GetTotalPulls(kind), pityCount, hasPity, GachaPoolDefinition.HighestGradePityLimit);
            }

            return new GachaPoolProgress(pool, GetTotalPulls(kind), GetPityCount(pool.Id));
        }

        public GachaPoolDefinition GetPoolDefinition(GachaPoolKind kind)
        {
            return GachaPoolDefinitions.Get(kind);
        }

        public bool CanRoll(GachaPoolKind kind, int count)
        {
            count = Mathf.Clamp(count, 1, 10);
            GachaPoolDefinition pool = GachaPoolDefinitions.Get(kind);
            if (wallet == null)
            {
                return false;
            }

            if (pool.UsesHeroTicket)
            {
                return wallet.CanSpendHeroSummonCost(count, pool.RubyCostPerPull);
            }

            if (pool.UsesEquipmentTicket)
            {
                return wallet.CanSpendEquipmentSummonCost(count, pool.RubyCostPerPull);
            }

            return wallet.CanSpendRuby((long)count * pool.RubyCostPerPull);
        }

        public string GetCostText(GachaPoolKind kind, int count)
        {
            count = Mathf.Clamp(count, 1, 10);
            GachaPoolDefinition pool = GachaPoolDefinitions.Get(kind);
            if (pool.UsesHeroTicket)
            {
                long tickets = wallet != null ? wallet.HeroSummonTicket : 0;
                long ticketUse = Math.Min(tickets, count);
                long rubyCost = (count - ticketUse) * pool.RubyCostPerPull;
                return rubyCost > 0 ? "영웅권 " + ticketUse + " + 루비 " + rubyCost.ToString("N0") : "영웅권 " + count;
            }

            if (pool.UsesEquipmentTicket)
            {
                long tickets = wallet != null ? wallet.EquipmentSummonTicket : 0;
                long ticketUse = Math.Min(tickets, count);
                long rubyCost = (count - ticketUse) * pool.RubyCostPerPull;
                return rubyCost > 0 ? "장비권 " + ticketUse + " + 루비 " + rubyCost.ToString("N0") : "장비권 " + count;
            }

            return "루비 " + ((long)count * pool.RubyCostPerPull).ToString("N0");
        }

        public string GetFeaturedRewardText(GachaPoolKind kind)
        {
            return GetFeaturedRewardText(kind, string.Empty);
        }

        public string GetFeaturedRewardText(GachaPoolKind kind, string eventTargetId)
        {
            GachaPoolDefinition pool = GachaPoolDefinitions.Get(kind);
            if (pool.Kind == GachaPoolKind.Event)
            {
                GachaEventTargetDefinition target = GachaEventTargetDefinitions.Get(eventTargetId);
                return target != null
                    ? GachaRateTable.GetRarityLabel(target.Rarity) + " " + target.CategoryLabel + " " + target.DisplayName
                    : pool.FeaturedLabel;
            }

            return pool.FeaturedLabel;
        }

        public string GetRateSummaryText(GachaPoolKind kind)
        {
            return GetRateSummaryText(kind, string.Empty);
        }

        public string GetRateSummaryText(GachaPoolKind kind, string eventTargetId)
        {
            GachaEventTargetDefinition eventTarget = kind == GachaPoolKind.Event
                ? GachaEventTargetDefinitions.Get(eventTargetId)
                : null;
            return GachaRollService.GetRateSummaryText(kind, GetProgress(kind, eventTargetId).Level, eventTarget);
        }

        public static int GetTotalRateWeight()
        {
            return GachaRateTable.RateWeightTotal;
        }

        public static int GetRarityRateWeight(HeroRarity rarity)
        {
            return GachaRateTable.GetRarityRateWeight(rarity, 1);
        }

        public static string GetRateSummaryText()
        {
            return GachaRollService.GetRateSummaryText(GachaPoolKind.Hero, 1, null);
        }

        private GachaRollOutcome RollOne(GachaPoolDefinition pool, GachaEventTargetDefinition eventTarget)
        {
            GachaPoolKind kind = pool.Kind;
            int totalPulls = GetTotalPulls(kind);
            string pityKey = GetPityKey(pool, eventTarget);
            int pityCount = !string.IsNullOrEmpty(pityKey) ? GetPityCount(pityKey) : 0;
            int level = GachaRateTable.GetLevel(totalPulls);
            bool hasPity = !string.IsNullOrEmpty(pityKey);
            bool forceHighestGrade = hasPity && pityCount + 1 >= GachaPoolDefinition.HighestGradePityLimit;

            GachaRollOutcome outcome = GachaRollService.Roll(pool, level, forceHighestGrade, random, eventTarget);
            totalPullsByPool[kind] = totalPulls + 1;

            if (hasPity)
            {
                pityCountByPool[pityKey] = outcome.IsHighestGrade ? 0 : Mathf.Clamp(pityCount + 1, 0, GachaPoolDefinition.HighestGradePityLimit);
            }

            return outcome;
        }

        private void GrantOutcome(GachaRollOutcome outcome)
        {
            switch (outcome.PoolKind)
            {
                case GachaPoolKind.Equipment:
                    equipmentInventory?.AddEquipment(outcome.RewardId, outcome.Amount);
                    break;
                case GachaPoolKind.Rune:
                    battleManager?.AddRuneCount(outcome.RewardId, ToRuneGrade(outcome.Rarity), outcome.Amount);
                    break;
                case GachaPoolKind.Event:
                    if (outcome.CategoryLabel == "장비")
                    {
                        equipmentInventory?.AddEquipment(outcome.RewardId, outcome.Amount);
                    }
                    else
                    {
                        battleManager?.AddHeroShards(outcome.RewardId, outcome.Amount);
                    }

                    break;
                default:
                    battleManager?.AddHeroShards(outcome.RewardId, outcome.Amount);
                    break;
            }
        }

        private bool SpendCost(GachaPoolDefinition pool, int count)
        {
            if (wallet == null)
            {
                return false;
            }

            if (pool.UsesHeroTicket)
            {
                return wallet.SpendHeroSummonCost(count, pool.RubyCostPerPull);
            }

            if (pool.UsesEquipmentTicket)
            {
                return wallet.SpendEquipmentSummonCost(count, pool.RubyCostPerPull);
            }

            return wallet.SpendRuby((long)count * pool.RubyCostPerPull);
        }

        private void LoadProgress()
        {
            totalPullsByPool.Clear();
            pityCountByPool.Clear();

            foreach (GachaPoolDefinition pool in GachaPoolDefinitions.All)
            {
                totalPullsByPool[pool.Kind] = Mathf.Max(0, PlayerPrefs.GetInt(SaveKeys.GachaTotalPulls(pool.Id), 0));
                if (pool.HasHighestGradePity)
                {
                    pityCountByPool[pool.Id] =
                        Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.GachaPityCount(pool.Id), 0), 0, pool.PityLimit);
                }
            }

            foreach (GachaEventTargetDefinition eventTarget in GachaEventTargetDefinitions.All)
            {
                if (!eventTarget.HasPity)
                {
                    continue;
                }

                pityCountByPool[eventTarget.Id] =
                    Mathf.Clamp(
                        PlayerPrefs.GetInt(SaveKeys.GachaPityCount(GachaPoolDefinitions.Get(GachaPoolKind.Event).Id, eventTarget.Id), 0),
                        0,
                        GachaPoolDefinition.HighestGradePityLimit);
            }
        }

        private void SavePoolProgress(GachaPoolDefinition pool, GachaEventTargetDefinition eventTarget)
        {
            if (pool == null)
            {
                return;
            }

            PlayerPrefs.SetInt(SaveKeys.GachaTotalPulls(pool.Id), GetTotalPulls(pool.Kind));
            if (pool.HasHighestGradePity)
            {
                PlayerPrefs.SetInt(SaveKeys.GachaPityCount(pool.Id), GetPityCount(pool.Id));
            }

            if (eventTarget != null && eventTarget.HasPity)
            {
                PlayerPrefs.SetInt(SaveKeys.GachaPityCount(pool.Id, eventTarget.Id), GetPityCount(eventTarget.Id));
            }

            PlayerPrefs.Save();
        }

        private int GetTotalPulls(GachaPoolKind kind)
        {
            return totalPullsByPool.TryGetValue(kind, out int totalPulls) ? Mathf.Max(0, totalPulls) : 0;
        }

        private int GetPityCount(string key)
        {
            return !string.IsNullOrEmpty(key) && pityCountByPool.TryGetValue(key, out int pityCount) ? Mathf.Max(0, pityCount) : 0;
        }

        private static string GetPityKey(GachaPoolDefinition pool, GachaEventTargetDefinition eventTarget)
        {
            if (pool.Kind == GachaPoolKind.Event)
            {
                return eventTarget != null && eventTarget.HasPity ? eventTarget.Id : string.Empty;
            }

            return pool.HasHighestGradePity ? pool.Id : string.Empty;
        }

        private static RuneGrade ToRuneGrade(HeroRarity rarity)
        {
            return (RuneGrade)Mathf.Clamp((int)rarity, 0, (int)RuneGrade.Mythic);
        }

        private static void AppendOutcomeLine(StringBuilder result, GachaRollOutcome outcome, bool appendLineBreak)
        {
            result.Append(outcome.FormatLine());
            if (appendLineBreak)
            {
                result.AppendLine();
            }
        }
    }
}
