using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SaveManager : MonoBehaviour
{
    public string LoadString(string key, string fallback)
    {
        return PlayerPrefs.GetString(key, fallback);
    }

    public void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value ?? string.Empty);
    }

    public bool LoadBool(string key, bool fallback)
    {
        return PlayerPrefs.GetInt(key, fallback ? 1 : 0) == 1;
    }

    public void SaveBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    public long LoadLong(string key, long fallback)
    {
        string raw = PlayerPrefs.GetString(key, fallback.ToString());
        return long.TryParse(raw, out long value) ? value : fallback;
    }

    public void SaveLong(string key, long value)
    {
        PlayerPrefs.SetString(key, value.ToString());
    }

    public T LoadEnum<T>(string key, T fallback) where T : struct
    {
        string raw = PlayerPrefs.GetString(key, fallback.ToString());
        return Enum.TryParse(raw, out T value) ? value : fallback;
    }

    public List<HeroState> LoadHeroes()
    {
        var heroes = new List<HeroState>();
        foreach (HeroDefinition definition in GameData.Heroes)
        {
            int level = PlayerPrefs.GetInt(SaveKeys.HeroLevel(definition.Id), 1);
            int shards = PlayerPrefs.GetInt(SaveKeys.HeroShards(definition.Id), 0);
            int stars = PlayerPrefs.GetInt(SaveKeys.HeroStars(definition.Id), 0);
            heroes.Add(new HeroState(definition, level, shards, stars));
        }

        return heroes;
    }

    public void SaveHero(HeroState hero)
    {
        PlayerPrefs.SetInt(SaveKeys.HeroLevel(hero.Definition.Id), hero.Level);
        PlayerPrefs.SetInt(SaveKeys.HeroShards(hero.Definition.Id), hero.Shards);
        PlayerPrefs.SetInt(SaveKeys.HeroStars(hero.Definition.Id), hero.Stars);
    }

    public DateTime? LoadLastOnlineUtc()
    {
        long ticks = LoadLong(SaveKeys.LastOnlineUtcTicks, 0);
        if (ticks <= 0)
        {
            return null;
        }

        try
        {
            return new DateTime(ticks, DateTimeKind.Utc);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    public void SaveLastOnlineUtc(DateTime utc)
    {
        SaveLong(SaveKeys.LastOnlineUtcTicks, utc.ToUniversalTime().Ticks);
    }

    public void Flush()
    {
        PlayerPrefs.Save();
    }

    public void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
