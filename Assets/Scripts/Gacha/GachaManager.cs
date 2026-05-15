using System;
using System.Text;
using UnityEngine;

public sealed class GachaManager : MonoBehaviour
{
    private const int RubyCostPerHeroSummon = 150;

    private readonly System.Random random = new System.Random();
    private BattleManager battleManager;
    private CurrencyWallet wallet;

    public event Action Changed;

    public string LastResult { get; private set; } = "뽑기 대기";

    public void Initialize(BattleManager battle, CurrencyWallet currency)
    {
        battleManager = battle;
        wallet = currency;
    }

    public void Roll(int count)
    {
        count = Mathf.Clamp(count, 1, 10);
        if (!wallet.SpendHeroSummonCost(count, RubyCostPerHeroSummon))
        {
            LastResult = "히어로 뽑기 실패: 뽑기권과 루비 부족";
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

    private void RollOne(out HeroDefinition hero, out int shards)
    {
        HeroRarity rarity = RollRarity();
        hero = RollHero(rarity);
        shards = hero.GetSummonShardReward();
    }

    private HeroRarity RollRarity()
    {
        int value = random.Next(0, 10000);
        if (value < 5500)
        {
            return HeroRarity.Uncommon;
        }

        if (value < 8500)
        {
            return HeroRarity.Rare;
        }

        if (value < 9600)
        {
            return HeroRarity.Epic;
        }

        if (value < 9950)
        {
            return HeroRarity.Legendary;
        }

        return HeroRarity.Mythic;
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
}
