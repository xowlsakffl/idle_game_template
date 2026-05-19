using System;
using UnityEngine;

public enum TalentEffectKind
{
    AttackPercent,
    HpPercent,
    CriticalDamagePercent,
    FinalDamagePercent,
    SkillDamagePercent,
    DamageReductionPercent,
    MoveSpeedPercent,
    GoldGainPercent,
    HeroExpGainPercent,
    AccountExpGainPercent
}

[Serializable]
public sealed class TalentDefinition
{
    public TalentDefinition(
        string id,
        string displayName,
        string icon,
        string branchName,
        int branchIndex,
        int tier,
        int maxLevel,
        int costPerLevel,
        double valuePerLevel,
        TalentEffectKind effectKind)
    {
        Id = id;
        DisplayName = displayName;
        Icon = icon;
        BranchName = branchName;
        BranchIndex = Mathf.Max(0, branchIndex);
        Tier = Mathf.Max(0, tier);
        MaxLevel = Mathf.Max(1, maxLevel);
        CostPerLevel = Mathf.Max(1, costPerLevel);
        ValuePerLevel = Math.Max(0d, valuePerLevel);
        EffectKind = effectKind;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string Icon { get; }
    public string BranchName { get; }
    public int BranchIndex { get; }
    public int Tier { get; }
    public int MaxLevel { get; }
    public int CostPerLevel { get; }
    public double ValuePerLevel { get; }
    public TalentEffectKind EffectKind { get; }

    public double GetValue(int level)
    {
        return Math.Max(0, Math.Min(level, MaxLevel)) * ValuePerLevel;
    }

    public string FormatValue(int level)
    {
        return GetEffectLabel() + " +" + GetValue(level).ToString("0.#") + "%";
    }

    public string GetEffectLabel()
    {
        switch (EffectKind)
        {
            case TalentEffectKind.AttackPercent:
                return "공격력";
            case TalentEffectKind.HpPercent:
                return "체력";
            case TalentEffectKind.CriticalDamagePercent:
                return "치명타 데미지";
            case TalentEffectKind.FinalDamagePercent:
                return "최종 데미지";
            case TalentEffectKind.SkillDamagePercent:
                return "스킬 데미지";
            case TalentEffectKind.DamageReductionPercent:
                return "받는 피해 감소";
            case TalentEffectKind.MoveSpeedPercent:
                return "이동속도";
            case TalentEffectKind.GoldGainPercent:
                return "골드 획득량";
            case TalentEffectKind.HeroExpGainPercent:
                return "경험치책 획득량";
            case TalentEffectKind.AccountExpGainPercent:
                return "계정 경험치 획득량";
            default:
                return "효과";
        }
    }
}
