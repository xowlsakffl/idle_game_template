using System.Collections.Generic;

namespace IdleGame.Data
{
    public static partial class TalentData
    {
        private const int DefaultMaxLevel = 100;
        private const int DefaultCost = 1;

        private static readonly TalentSpec[][] talentSpecs =
        {
            new[] { Spec("ATK_CORE", "검술 단련", "ATK", TalentEffectKind.AttackPercent, 0.05d) },
            new[]
            {
                Spec("GOLD_GAIN", "전리품 감각", "GOLD", TalentEffectKind.GoldGainPercent, 0.08d),
                Spec("HP_CORE", "생명 강화", "HP", TalentEffectKind.HpPercent, 0.08d),
                Spec("CRIT_EDGE", "약점 간파", "CRIT", TalentEffectKind.CriticalDamagePercent, 0.06d)
            },
            new[]
            {
                Spec("BOOK_GAIN", "수련 기록", "BOOK", TalentEffectKind.HeroExpGainPercent, 0.08d),
                Spec("GUARD_CORE", "수호 자세", "DEF", TalentEffectKind.DamageReductionPercent, 0.015d),
                Spec("FINAL_STRIKE", "결정타", "FIN", TalentEffectKind.FinalDamagePercent, 0.04d)
            },
            new[] { Spec("BATTLE_FLOW", "전투 흐름", "SKL", TalentEffectKind.SkillDamagePercent, 0.05d) },
            new[]
            {
                Spec("MOVE_CORE", "전장 기동", "SPD", TalentEffectKind.MoveSpeedPercent, 0.025d),
                Spec("HP_ADVANCE", "불굴의 체력", "HP+", TalentEffectKind.HpPercent, 0.09d)
            },
            new[]
            {
                Spec("ACCOUNT_STUDY", "계정 숙련", "ACC", TalentEffectKind.AccountExpGainPercent, 0.07d),
                Spec("GOLD_ADVANCE", "희귀 전리품", "G+", TalentEffectKind.GoldGainPercent, 0.07d)
            },
            new[] { Spec("SKILL_FORCE", "스킬 증폭", "SKL+", TalentEffectKind.SkillDamagePercent, 0.06d) },
            new[]
            {
                Spec("CRIT_FORCE", "치명 집중", "CRIT+", TalentEffectKind.CriticalDamagePercent, 0.08d),
                Spec("DAMAGE_GUARD", "피해 제어", "DR", TalentEffectKind.DamageReductionPercent, 0.012d),
                Spec("ATTACK_DRILL", "전열 훈련", "ATK+", TalentEffectKind.AttackPercent, 0.06d)
            },
            new[]
            {
                Spec("FIELD_MOBILITY", "사냥 동선", "MOV", TalentEffectKind.MoveSpeedPercent, 0.025d),
                Spec("BOOK_MASTERY", "교본 연구", "EXP", TalentEffectKind.HeroExpGainPercent, 0.08d),
                Spec("FINAL_CORE", "마무리 감각", "FIN+", TalentEffectKind.FinalDamagePercent, 0.05d)
            },
            new[]
            {
                Spec("HP_MASTERY", "강인한 육체", "HP++", TalentEffectKind.HpPercent, 0.10d),
                Spec("GOLD_MASTERY", "보상 증폭", "GOLD+", TalentEffectKind.GoldGainPercent, 0.08d)
            },
            new[] { Spec("ACCOUNT_FLOW", "계정 가속", "ACC+", TalentEffectKind.AccountExpGainPercent, 0.08d) },
            new[]
            {
                Spec("SKILL_MASTERY", "스킬 숙련", "SKL", TalentEffectKind.SkillDamagePercent, 0.08d),
                Spec("GUARD_MASTERY", "방어 숙련", "DEF+", TalentEffectKind.DamageReductionPercent, 0.015d)
            },
            new[] { Spec("ATTACK_MASTERY", "공격 숙련", "ATK", TalentEffectKind.AttackPercent, 0.07d) },
            new[]
            {
                Spec("CRIT_MASTERY", "정밀 타격", "CRIT", TalentEffectKind.CriticalDamagePercent, 0.09d),
                Spec("HP_EXPERT", "생존 본능", "HP", TalentEffectKind.HpPercent, 0.11d),
                Spec("GOLD_EXPERT", "전리품 숙련", "G", TalentEffectKind.GoldGainPercent, 0.09d)
            },
            new[]
            {
                Spec("MOVE_EXPERT", "전장 지배", "SPD", TalentEffectKind.MoveSpeedPercent, 0.03d),
                Spec("BOOK_EXPERT", "훈련 효율", "BOOK", TalentEffectKind.HeroExpGainPercent, 0.09d),
                Spec("FINAL_EXPERT", "끝장내기", "FIN", TalentEffectKind.FinalDamagePercent, 0.06d)
            },
            new[]
            {
                Spec("SKILL_EXPERT", "스킬 과부하", "SKL+", TalentEffectKind.SkillDamagePercent, 0.09d),
                Spec("GUARD_EXPERT", "피해 흡수", "DR", TalentEffectKind.DamageReductionPercent, 0.015d),
                Spec("ACCOUNT_EXPERT", "계정 연구", "ACC", TalentEffectKind.AccountExpGainPercent, 0.09d)
            },
            new[] { Spec("GRAND_CORE", "영웅 지휘", "ALL", TalentEffectKind.AttackPercent, 0.09d) },
            new[]
            {
                Spec("GRAND_SURVIVAL", "장기 생존", "HP+", TalentEffectKind.HpPercent, 0.12d),
                Spec("GRAND_LOOT", "장기 보상", "G+", TalentEffectKind.GoldGainPercent, 0.10d)
            },
            new[] { Spec("MYTH_CORE", "신화 전투술", "MAX", TalentEffectKind.FinalDamagePercent, 0.07d) }
        };

        private static readonly List<TalentDefinition> emptyTalents = new List<TalentDefinition>();
        private static readonly TalentDefinition[] talents = BuildTalents();
        private static readonly Dictionary<string, TalentDefinition> talentsById = BuildTalentMap();
        private static readonly Dictionary<int, List<TalentDefinition>> talentsByDepth = BuildTalentDepthMap();

        public static IReadOnlyList<TalentDefinition> Talents => talents;
        public static int DepthCount => talentSpecs.Length;
        public static int MaxDepth => DepthCount - 1;

        public static TalentDefinition GetTalent(string id)
        {
            return !string.IsNullOrEmpty(id) && talentsById.TryGetValue(id, out TalentDefinition talent)
                ? talent
                : talents[0];
        }

        public static bool TryGetTalent(string id, out TalentDefinition talent)
        {
            if (string.IsNullOrEmpty(id))
            {
                talent = null;
                return false;
            }

            return talentsById.TryGetValue(id, out talent);
        }

        public static IReadOnlyList<TalentDefinition> GetTalentsInDepth(int depth)
        {
            return talentsByDepth.TryGetValue(depth, out List<TalentDefinition> depthTalents)
                ? depthTalents
                : emptyTalents;
        }

        public static IReadOnlyList<TalentDefinition> GetPrerequisiteTalents(TalentDefinition talent)
        {
            var prerequisites = new List<TalentDefinition>();
            if (talent == null)
            {
                return prerequisites;
            }

            for (int i = 0; i < talent.PrerequisiteIds.Count; i++)
            {
                if (TryGetTalent(talent.PrerequisiteIds[i], out TalentDefinition prerequisite))
                {
                    prerequisites.Add(prerequisite);
                }
            }

            return prerequisites;
        }

        public static TalentDefinition GetPreviousTalent(TalentDefinition talent)
        {
            IReadOnlyList<TalentDefinition> prerequisites = GetPrerequisiteTalents(talent);
            return prerequisites.Count > 0 ? prerequisites[0] : null;
        }

    }
}
