using IdleGame.Data;

namespace IdleGame.UI.Hero.Detail
{
    public static class HeroDetailUiText
    {
        public static string GetSkillName(HeroState hero)
        {
            switch (hero.Definition.Trait)
            {
                case HeroTrait.Ranged:
                    return "화살비";
                case HeroTrait.Melee:
                    return "연속 베기";
                case HeroTrait.Support:
                    return "축복의 빛";
                case HeroTrait.Defense:
                    return "수호 강타";
                default:
                    return "기본 공격";
            }
        }

        public static string GetSkillDescription(HeroState hero)
        {
            switch (hero.Definition.Trait)
            {
                case HeroTrait.Ranged:
                    return "하늘을 향해 화살을 발사하여 공격력의 42% 피해를 4회 입힌다.";
                case HeroTrait.Melee:
                    return "가까운 적에게 파고들어 공격력의 180% 피해를 입힌다.";
                case HeroTrait.Support:
                    return "전장의 아군을 지원해 5초간 파티 공격력을 12% 높인다.";
                case HeroTrait.Defense:
                    return "방패로 적을 밀어내 공격력의 90%와 체력의 8%만큼 피해를 입힌다.";
                default:
                    return "현재 대상에게 피해를 입힌다.";
            }
        }

        public static string GetStarEffectLine(HeroState hero, int requiredStars, string effectText)
        {
            bool unlocked = hero.Stars >= requiredStars;
            string state = unlocked ? "<color=#90FF58>해금</color>" : "<color=#7C8495>잠김</color>";
            return requiredStars + "성 " + state + "  " + effectText;
        }
    }
}
