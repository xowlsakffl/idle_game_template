using System;
using UnityEngine;

public sealed class AccountProgressManager : MonoBehaviour
{
    private const int MaxAccountLevel = 9999;
    private SaveManager saveManager;

    public event Action Changed;

    public int Level { get; private set; } = 1;
    public int DebugTalentPointBonus { get; private set; }
    public GameNumber Experience { get; private set; } = GameNumber.Zero;
    public GameNumber NextLevelExperience => GetRequiredExperienceForLevel(Level);
    public int TotalTalentPointsEarned => Mathf.Max(0, Level - 1 + DebugTalentPointBonus);
    public int SpentTalentPoints => CalculateSpentTalentPoints();
    public int AvailableTalentPoints => Mathf.Max(0, TotalTalentPointsEarned - SpentTalentPoints);

    public void Initialize(SaveManager save)
    {
        saveManager = save;
        Level = Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.AccountLevel, 1), 1, MaxAccountLevel);
        DebugTalentPointBonus = Mathf.Max(0, PlayerPrefs.GetInt(SaveKeys.DebugTalentPointBonus, 0));
        Experience = saveManager.LoadGameNumber(SaveKeys.AccountExperience, GameNumber.Zero);
        NormalizeExperience();
        NotifyChanged();
    }

    public void AddExperience(GameNumber amount)
    {
        if (amount <= GameNumber.Zero)
        {
            return;
        }

        Experience += amount;
        NormalizeExperience();
        Save();
        NotifyChanged();
    }

    public void DebugAddTalentPoints(int points)
    {
        int pointGain = Mathf.Max(0, points);
        if (pointGain <= 0)
        {
            return;
        }

        DebugTalentPointBonus = Mathf.Max(0, DebugTalentPointBonus + pointGain);
        Save();
        NotifyChanged();
    }

    public void DebugAddLevels(int levels)
    {
        int levelGain = Mathf.Max(0, levels);
        if (levelGain <= 0 || Level >= MaxAccountLevel)
        {
            return;
        }

        Level = Mathf.Clamp(Level + levelGain, 1, MaxAccountLevel);
        Experience = GameNumber.Zero;
        Save();
        NotifyChanged();
    }

    public bool TryLevelUpTalent(string talentId)
    {
        TalentDefinition talent = TalentData.GetTalent(talentId);
        if (talent == null)
        {
            return false;
        }

        int level = GetTalentLevel(talent.Id);
        if (level >= talent.MaxLevel || !IsTalentUnlocked(talent) || AvailableTalentPoints < talent.CostPerLevel)
        {
            return false;
        }

        PlayerPrefs.SetInt(SaveKeys.TalentLevel(talent.Id), level + 1);
        saveManager.Flush();
        NotifyChanged();
        return true;
    }

    public int GetTalentLevel(string talentId)
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.TalentLevel(talentId), 0), 0, TalentData.GetTalent(talentId).MaxLevel);
    }

    public bool IsTalentUnlocked(TalentDefinition talent)
    {
        if (talent == null)
        {
            return false;
        }

        if (talent.PrerequisiteIds.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < talent.PrerequisiteIds.Count; i++)
        {
            if (!TalentData.TryGetTalent(talent.PrerequisiteIds[i], out TalentDefinition prerequisite))
            {
                continue;
            }

            if (GetTalentLevel(prerequisite.Id) >= prerequisite.MaxLevel)
            {
                return true;
            }
        }

        return false;
    }

    public double GetEffectPercent(TalentEffectKind kind)
    {
        double total = 0d;
        foreach (TalentDefinition talent in TalentData.Talents)
        {
            if (talent.EffectKind == kind)
            {
                total += talent.GetValue(GetTalentLevel(talent.Id));
            }
        }

        return total;
    }

    public double GetMultiplier(TalentEffectKind kind)
    {
        return Math.Max(0d, 1d + GetEffectPercent(kind) / 100d);
    }

    public double DamageTakenMultiplier => Math.Max(0.25d, 1d - Math.Min(75d, GetEffectPercent(TalentEffectKind.DamageReductionPercent)) / 100d);

    public GameNumber GetRequiredExperienceForLevel(int level)
    {
        double required = 40d * Math.Pow(Mathf.Max(1, level), 1.22d);
        return GameData.ClampNumber(GameNumber.Ceiling(GameNumber.FromDouble(required)));
    }

    private void NormalizeExperience()
    {
        int guard = 0;
        while (Level < MaxAccountLevel && Experience >= GetRequiredExperienceForLevel(Level) && guard < 1000)
        {
            Experience -= GetRequiredExperienceForLevel(Level);
            Level += 1;
            guard += 1;
        }

        if (Level >= MaxAccountLevel)
        {
            Level = MaxAccountLevel;
            Experience = GameNumber.Zero;
        }
    }

    private int CalculateSpentTalentPoints()
    {
        int spent = 0;
        foreach (TalentDefinition talent in TalentData.Talents)
        {
            spent += GetTalentLevel(talent.Id) * talent.CostPerLevel;
        }

        return spent;
    }

    private void Save()
    {
        PlayerPrefs.SetInt(SaveKeys.AccountLevel, Level);
        PlayerPrefs.SetInt(SaveKeys.DebugTalentPointBonus, DebugTalentPointBonus);
        saveManager.SaveGameNumber(SaveKeys.AccountExperience, Experience);
        saveManager.Flush();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
