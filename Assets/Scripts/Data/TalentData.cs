using System.Collections.Generic;

public static class TalentData
{
    private const int DefaultMaxLevel = 100;
    private const int DefaultCost = 1;

    private static readonly TalentDefinition[] talents =
    {
        new TalentDefinition("ATK_CORE", "검술 단련", "⚔", "공격", 0, 0, DefaultMaxLevel, DefaultCost, 1.0d, TalentEffectKind.AttackPercent),
        new TalentDefinition("CRIT_EDGE", "약점 간파", "◆", "공격", 0, 1, DefaultMaxLevel, DefaultCost, 1.5d, TalentEffectKind.CriticalDamagePercent),
        new TalentDefinition("FINAL_STRIKE", "결정타", "!", "공격", 0, 2, DefaultMaxLevel, DefaultCost, 0.5d, TalentEffectKind.FinalDamagePercent),
        new TalentDefinition("SKILL_FORCE", "스킬 증폭", "✦", "공격", 0, 3, DefaultMaxLevel, DefaultCost, 0.8d, TalentEffectKind.SkillDamagePercent),

        new TalentDefinition("HP_CORE", "생명 강화", "♥", "생존", 1, 0, DefaultMaxLevel, DefaultCost, 1.5d, TalentEffectKind.HpPercent),
        new TalentDefinition("GUARD_CORE", "수호 자세", "▣", "생존", 1, 1, DefaultMaxLevel, DefaultCost, 0.2d, TalentEffectKind.DamageReductionPercent),
        new TalentDefinition("MOVE_CORE", "전장 기동", "↗", "생존", 1, 2, DefaultMaxLevel, DefaultCost, 0.5d, TalentEffectKind.MoveSpeedPercent),
        new TalentDefinition("HP_ADVANCE", "불굴의 체력", "✚", "생존", 1, 3, DefaultMaxLevel, DefaultCost, 1.0d, TalentEffectKind.HpPercent),

        new TalentDefinition("GOLD_GAIN", "전리품 감각", "G", "성장", 2, 0, DefaultMaxLevel, DefaultCost, 1.0d, TalentEffectKind.GoldGainPercent),
        new TalentDefinition("BOOK_GAIN", "수련 기록", "EXP", "성장", 2, 1, DefaultMaxLevel, DefaultCost, 1.0d, TalentEffectKind.HeroExpGainPercent),
        new TalentDefinition("ACCOUNT_STUDY", "계정 숙련", "AP", "성장", 2, 2, DefaultMaxLevel, DefaultCost, 1.0d, TalentEffectKind.AccountExpGainPercent),
        new TalentDefinition("GOLD_ADVANCE", "희귀 전리품", "★", "성장", 2, 3, DefaultMaxLevel, DefaultCost, 0.7d, TalentEffectKind.GoldGainPercent)
    };

    private static readonly Dictionary<string, TalentDefinition> talentsById = BuildTalentMap();

    public static IReadOnlyList<TalentDefinition> Talents => talents;

    public static TalentDefinition GetTalent(string id)
    {
        return !string.IsNullOrEmpty(id) && talentsById.TryGetValue(id, out TalentDefinition talent)
            ? talent
            : talents[0];
    }

    public static TalentDefinition GetPreviousTalent(TalentDefinition talent)
    {
        if (talent == null || talent.Tier <= 0)
        {
            return null;
        }

        for (int i = 0; i < talents.Length; i++)
        {
            TalentDefinition candidate = talents[i];
            if (candidate.BranchIndex == talent.BranchIndex && candidate.Tier == talent.Tier - 1)
            {
                return candidate;
            }
        }

        return null;
    }

    private static Dictionary<string, TalentDefinition> BuildTalentMap()
    {
        var map = new Dictionary<string, TalentDefinition>();
        for (int i = 0; i < talents.Length; i++)
        {
            map[talents[i].Id] = talents[i];
        }

        return map;
    }
}
