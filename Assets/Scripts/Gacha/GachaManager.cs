using System;
using System.Text;
using UnityEngine;

public sealed class GachaManager : MonoBehaviour
{
    private readonly System.Random random = new System.Random();
    private BattleManager battleManager;

    public event Action Changed;

    public string LastResult { get; private set; } = "뽑기 대기";

    public void Initialize(BattleManager battle)
    {
        battleManager = battle;
    }

    public void Roll(int count)
    {
        count = Mathf.Clamp(count, 1, 10);
        var result = new StringBuilder();

        for (int i = 0; i < count; i++)
        {
            RollOne(out string rarity, out HeroDefinition hero, out int shards);
            battleManager.AddHeroShards(hero.Id, shards);
            result.Append(rarity).Append(" ").Append(hero.DisplayName).Append(" +").Append(shards);

            if (i < count - 1)
            {
                result.AppendLine();
            }
        }

        LastResult = result.ToString();
        Changed?.Invoke();
    }

    private void RollOne(out string rarity, out HeroDefinition hero, out int shards)
    {
        int value = random.Next(0, 100);
        if (value < 70)
        {
            rarity = "Common";
            shards = 1;
        }
        else if (value < 95)
        {
            rarity = "Rare";
            shards = 3;
        }
        else
        {
            rarity = "Epic";
            shards = 10;
        }

        hero = GameData.Heroes[random.Next(0, GameData.Heroes.Count)];
    }
}
