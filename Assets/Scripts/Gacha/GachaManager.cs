using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class GachaManager : MonoBehaviour
{
    private const int RubyCostPerHeroSummon = 150;
    private const int RubyCostPerEquipmentSummon = 100;
    private const int RateWeightTotal = 10000;
    private static readonly GachaRarityRate[] RarityRates =
    {
        new GachaRarityRate(HeroRarity.Common, 4500),
        new GachaRarityRate(HeroRarity.Uncommon, 3000),
        new GachaRarityRate(HeroRarity.Rare, 1500),
        new GachaRarityRate(HeroRarity.Epic, 700),
        new GachaRarityRate(HeroRarity.Legendary, 250),
        new GachaRarityRate(HeroRarity.Mythic, 50)
    };

    private readonly System.Random random = new System.Random();
    private BattleManager battleManager;
    private CurrencyWallet wallet;
    private EquipmentInventory equipmentInventory;

    public event Action Changed;

    public string LastResult { get; private set; } = "뽑기 대기";
    public static IReadOnlyList<GachaRarityRate> Rates => RarityRates;

    public void Initialize(BattleManager battle, CurrencyWallet currency, EquipmentInventory equipment)
    {
        battleManager = battle;
        wallet = currency;
        equipmentInventory = equipment;
    }

    public void Roll(int count)
    {
        RollHeroes(count);
    }

    public void RollHeroes(int count)
    {
        count = Mathf.Clamp(count, 1, 10);
        if (!wallet.SpendHeroSummonCost(count, RubyCostPerHeroSummon))
        {
            LastResult = "영웅 뽑기 실패: 뽑기권과 루비 부족";
            Changed?.Invoke();
            return;
        }

        var result = new StringBuilder();

        for (int i = 0; i < count; i++)
        {
            RollOne(out HeroDefinition hero, out int shards);
            battleManager.AddHeroShards(hero.Id, shards);
            result.Append(hero.RarityLabel)
                .Append(" ")
                .Append(hero.DisplayName)
                .Append(" +")
                .Append(shards)
                .Append(" 조각");

            if (i < count - 1)
            {
                result.AppendLine();
            }
        }

        LastResult = result.ToString();
        Changed?.Invoke();
    }

    public void RollEquipment(int count)
    {
        count = Mathf.Clamp(count, 1, 10);
        if (!wallet.SpendEquipmentSummonCost(count, RubyCostPerEquipmentSummon))
        {
            LastResult = "장비 뽑기 실패: 장비 뽑기권과 루비 부족";
            Changed?.Invoke();
            return;
        }

        var result = new StringBuilder();

        for (int i = 0; i < count; i++)
        {
            EquipmentDefinition equipment = RollEquipmentDefinition(RollRarity());
            EquipmentState state = equipmentInventory.AddEquipment(equipment.Id, 1);
            result.Append(equipment.RarityLabel)
                .Append(" ")
                .Append(equipment.SlotLabel)
                .Append(" ")
                .Append(equipment.DisplayName)
                .Append(" +1개")
                .Append(" / 보유 x")
                .Append(state != null ? state.Count : equipmentInventory.GetOwnedCount(equipment.Id));

            if (i < count - 1)
            {
                result.AppendLine();
            }
        }

        LastResult = result.ToString();
        Changed?.Invoke();
    }

    private void RollOne(out HeroDefinition hero, out int shards)
    {
        HeroRarity rarity = RollRarity();
        hero = RollHero(rarity);
        shards = hero.GetSummonShardReward();
    }

    private HeroRarity RollRarity()
    {
        int value = random.Next(0, RateWeightTotal);
        int cumulativeWeight = 0;
        for (int i = 0; i < RarityRates.Length; i++)
        {
            cumulativeWeight += RarityRates[i].Weight;
            if (value < cumulativeWeight)
            {
                return RarityRates[i].Rarity;
            }

        }

        return RarityRates[RarityRates.Length - 1].Rarity;
    }

    public static int GetTotalRateWeight()
    {
        int total = 0;
        foreach (GachaRarityRate rate in RarityRates)
        {
            total += rate.Weight;
        }

        return total;
    }

    public static int GetRarityRateWeight(HeroRarity rarity)
    {
        foreach (GachaRarityRate rate in RarityRates)
        {
            if (rate.Rarity == rarity)
            {
                return rate.Weight;
            }
        }

        return 0;
    }

    public static string GetRateSummaryText()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < RarityRates.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(" / ");
            }

            builder.Append(GetRarityLabel(RarityRates[i].Rarity))
                .Append(" ")
                .Append(RarityRates[i].PercentText);
        }

        return builder.ToString();
    }

    private static string GetRarityLabel(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Common:
                return "커먼";
            case HeroRarity.Uncommon:
                return "언커먼";
            case HeroRarity.Rare:
                return "레어";
            case HeroRarity.Epic:
                return "에픽";
            case HeroRarity.Legendary:
                return "전설";
            case HeroRarity.Mythic:
                return "신화";
            default:
                return "미정";
        }
    }

    private HeroDefinition RollHero(HeroRarity rarity)
    {
        int matchCount = 0;
        foreach (HeroDefinition hero in GameData.Heroes)
        {
            if (hero.Rarity == rarity)
            {
                matchCount += 1;
            }
        }

        if (matchCount == 0)
        {
            return GameData.Heroes[random.Next(0, GameData.Heroes.Count)];
        }

        int selected = random.Next(0, matchCount);
        foreach (HeroDefinition hero in GameData.Heroes)
        {
            if (hero.Rarity != rarity)
            {
                continue;
            }

            if (selected == 0)
            {
                return hero;
            }

            selected -= 1;
        }

        return GameData.Heroes[0];
    }

    private EquipmentDefinition RollEquipmentDefinition(HeroRarity rarity)
    {
        int matchCount = 0;
        foreach (EquipmentDefinition equipment in GameData.Equipments)
        {
            if (equipment.Rarity == rarity)
            {
                matchCount += 1;
            }
        }

        if (matchCount == 0)
        {
            return GameData.Equipments[random.Next(0, GameData.Equipments.Count)];
        }

        int selected = random.Next(0, matchCount);
        foreach (EquipmentDefinition equipment in GameData.Equipments)
        {
            if (equipment.Rarity != rarity)
            {
                continue;
            }

            if (selected == 0)
            {
                return equipment;
            }

            selected -= 1;
        }

        return GameData.Equipments[0];
    }
}

public struct GachaRarityRate
{
    public GachaRarityRate(HeroRarity rarity, int weight)
    {
        Rarity = rarity;
        Weight = Mathf.Clamp(weight, 0, 10000);
    }

    public HeroRarity Rarity { get; }
    public int Weight { get; }
    public float Percent => Weight / 100f;
    public string PercentText => Percent.ToString("0.##") + "%";
}
