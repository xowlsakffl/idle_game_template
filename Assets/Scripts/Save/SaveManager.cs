using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SaveManager : MonoBehaviour
{
    private const float FlushIntervalSeconds = 5f;

    private bool flushPending;
    private float nextFlushTime;

    private void Awake()
    {
        nextFlushTime = Time.unscaledTime + FlushIntervalSeconds;
    }

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

    public double LoadDouble(string key, double fallback)
    {
        string raw = PlayerPrefs.GetString(key, fallback.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return double.TryParse(
            raw,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out double value)
            ? value
            : fallback;
    }

    public void SaveDouble(string key, double value)
    {
        PlayerPrefs.SetString(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public GameNumber LoadGameNumber(string key, GameNumber fallback)
    {
        string raw = PlayerPrefs.GetString(key, fallback.ToSaveString());
        return GameNumber.TryParse(raw, out GameNumber value) ? value : fallback;
    }

    public void SaveGameNumber(string key, GameNumber value)
    {
        PlayerPrefs.SetString(key, value.ToSaveString());
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
            HeroState hero = new HeroState(definition, level, shards, stars);
            string[] loadedTranscendOptionIds = new string[HeroDefinition.MaxTranscendSlots];
            bool hasRolledTranscendMarker = false;
            bool hasNonLegacyDefaultTranscendOption = false;
            for (int slot = 0; slot < HeroDefinition.MaxTranscendSlots; slot++)
            {
                string optionId = PlayerPrefs.GetString(SaveKeys.HeroTranscendOption(definition.Id, slot), string.Empty);
                loadedTranscendOptionIds[slot] = optionId;
                hasRolledTranscendMarker |= PlayerPrefs.GetInt(SaveKeys.HeroTranscendOptionRolled(definition.Id, slot), 0) == 1;
                hasNonLegacyDefaultTranscendOption |= !string.IsNullOrEmpty(optionId)
                    && !(slot == 0 && optionId == "COMMON_ACCOUNT_EXP_F");
            }

            bool onlyLegacyDefaultOption = !hasRolledTranscendMarker
                && !hasNonLegacyDefaultTranscendOption
                && loadedTranscendOptionIds.Length > 0
                && loadedTranscendOptionIds[0] == "COMMON_ACCOUNT_EXP_F";
            if (onlyLegacyDefaultOption)
            {
                loadedTranscendOptionIds[0] = string.Empty;
                PlayerPrefs.DeleteKey(SaveKeys.HeroTranscendOption(definition.Id, 0));
            }

            for (int slot = 0; slot < HeroDefinition.MaxTranscendSlots; slot++)
            {
                hero.SetTranscendOptionId(slot, loadedTranscendOptionIds[slot]);
            }

            heroes.Add(hero);
        }

        return heroes;
    }

    public void SaveHero(HeroState hero)
    {
        PlayerPrefs.SetInt(SaveKeys.HeroLevel(hero.Definition.Id), hero.Level);
        PlayerPrefs.SetInt(SaveKeys.HeroShards(hero.Definition.Id), hero.Shards);
        PlayerPrefs.SetInt(SaveKeys.HeroStars(hero.Definition.Id), hero.Stars);
        for (int slot = 0; slot < HeroDefinition.MaxTranscendSlots; slot++)
        {
            string optionId = hero.GetTranscendOptionId(slot);
            PlayerPrefs.SetString(SaveKeys.HeroTranscendOption(hero.Definition.Id, slot), optionId);
            if (!string.IsNullOrEmpty(optionId))
            {
                PlayerPrefs.SetInt(SaveKeys.HeroTranscendOptionRolled(hero.Definition.Id, slot), 1);
            }
        }
    }

    public void SaveHeroTranscendOption(HeroState hero, int slot)
    {
        if (hero == null || slot < 0 || slot >= HeroDefinition.MaxTranscendSlots)
        {
            return;
        }

        string optionId = hero.GetTranscendOptionId(slot);
        PlayerPrefs.SetString(SaveKeys.HeroTranscendOption(hero.Definition.Id, slot), optionId);
        if (!string.IsNullOrEmpty(optionId))
        {
            PlayerPrefs.SetInt(SaveKeys.HeroTranscendOptionRolled(hero.Definition.Id, slot), 1);
        }
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
        flushPending = true;
        if (nextFlushTime <= 0f)
        {
            nextFlushTime = Time.unscaledTime + FlushIntervalSeconds;
        }

        TryFlush();
    }

    public void FlushImmediate()
    {
        PlayerPrefs.Save();
        flushPending = false;
        nextFlushTime = Time.unscaledTime + FlushIntervalSeconds;
    }

    public void ResetAll()
    {
        flushPending = false;
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    private void Update()
    {
        TryFlush();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && flushPending)
        {
            FlushImmediate();
        }
    }

    private void OnApplicationQuit()
    {
        if (flushPending)
        {
            FlushImmediate();
        }
    }

    private void TryFlush()
    {
        if (!flushPending)
        {
            return;
        }

        if (Time.unscaledTime < nextFlushTime)
        {
            return;
        }

        FlushImmediate();
    }
}
