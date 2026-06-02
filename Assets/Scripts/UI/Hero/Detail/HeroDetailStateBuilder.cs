using System.Collections.Generic;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;
using IdleGame.UI.Hero;

namespace IdleGame.UI.Hero.Detail
{
    public sealed class HeroDetailBasicStateBuildArgs
    {
        public HeroState Hero;
        public HeroDetailTab ActiveTab;
        public CurrencyWallet Wallet;
        public double CombatPower;
        public double OwnedAttackBonusPercent;
        public string NoticeText;
    }

    public sealed class HeroDetailActionStateBuildArgs
    {
        public HeroState Hero;
        public bool InFormation;
        public CurrencyWallet Wallet;
    }

    public sealed class HeroDetailEquipmentDetailStateBuildArgs
    {
        public EquipmentState State;
        public CurrencyWallet Wallet;
        public bool EquippedToHero;
        public int AvailableCount;
        public string EquippedOwners;
        public int StarMaterialCount;
        public string NoticeText;
    }

    public sealed class HeroDetailEquipmentDismantleStateBuildArgs
    {
        public ICollection<EquipmentSlot> SelectedSlots;
        public int VisibleCount;
        public int SelectedCount;
        public int SelectedReward;
        public string NoticeText;
    }

    public sealed class HeroDetailEquipmentBulkDismantleStateBuildArgs
    {
        public HeroRarity SelectedRarity;
        public int Count;
        public int Reward;
        public string NoticeText;
    }

    public sealed class HeroDetailEquipmentContentStateBuildArgs
    {
        public ICollection<EquipmentSlot> SelectedSlots;
        public bool SlotSelectionActive;
        public EquipmentSlot SelectedSlot;
        public EquipmentState SelectedState;
        public int OwnedCount;
        public int VisibleCount;
    }

    public sealed class HeroDetailEquipmentContentViewState
    {
        public string SummaryText;
        public string EmptyText;
        public bool EmptyVisible;
    }

    public static partial class HeroDetailStateBuilder
    {
        public static HeroDetailBasicViewState BuildBasicInfo(HeroDetailBasicStateBuildArgs args)
        {
            if (args == null || args.Hero == null)
            {
                return null;
            }

            HeroState hero = args.Hero;
            return new HeroDetailBasicViewState
            {
                TitleText = "i  상세 정보",
                TraitText = HeroUiText.GetTraitLabel(hero.Definition.Trait) + "\n" + hero.Definition.RarityLabel,
                StarsText = StarUiText.FormatStars(hero.Stars) + "  " + hero.Stars + "/" + HeroDefinition.MaxStars + "성",
                CharacterText = hero.Definition.DisplayName
                    + "\n<size=30>" + hero.Definition.Role + "</size>"
                    + "\n<size=28>" + hero.Definition.PassiveLabel + "</size>",
                CharacterColor = Color.Lerp(HeroUiText.GetRarityColor(hero.Definition.Rarity), Color.white, 0.18f),
                LevelText = "Lv. " + hero.Level + "/" + hero.MaxLevel
                    + "    15성 최대 " + HeroDefinition.MaxLevelAtMaxStars,
                PowerText = "전투력 " + NumberFormatter.Format(args.CombatPower),
                ResourceText = BuildResourceText(args.ActiveTab, args.Wallet),
                SkillText = HeroDetailUiText.GetSkillName(hero)
                    + "\n" + HeroDetailUiText.GetSkillDescription(hero),
                StatsText = "공격력 " + NumberFormatter.Format(hero.AttackPower)
                    + "        체력  " + NumberFormatter.Format(hero.MaxHp)
                    + "\n공속  " + hero.AttackSpeed.ToString("0.##")
                    + "        이속  " + hero.MoveSpeed.ToString("0.#"),
                StarEffectsText = HeroDetailUiText.GetStarEffectLine(hero, 5, "패시브 효과 50% 강화")
                    + "\n" + HeroDetailUiText.GetStarEffectLine(hero, 10, "공격력/체력/공속/이속 +10%"),
                OwnedEffectText = hero.IsOwned
                    ? "[보유 효과]  공격력+" + args.OwnedAttackBonusPercent.ToString("0.##") + "%"
                    : "[미보유]  뽑기로 조각을 획득하면 배치 가능",
                NoticeText = args.NoticeText ?? string.Empty
            };
        }

        public static HeroDetailActionViewState BuildActionButtons(HeroDetailActionStateBuildArgs args)
        {
            if (args == null || args.Hero == null)
            {
                return null;
            }

            HeroState hero = args.Hero;
            bool isOwned = hero.IsOwned;
            bool maxLevel = hero.Level >= hero.MaxLevel;
            bool canPayLevelUp = isOwned && args.Wallet != null && args.Wallet.HeroExpItem >= hero.LevelUpCost;
            bool maxStars = hero.IsMaxStars;
            bool canStarUp = isOwned && hero.CanStarUp;

            return new HeroDetailActionViewState
            {
                Formation = new HeroDetailButtonViewState
                {
                    Interactable = isOwned,
                    Text = !isOwned ? "미보유" : args.InFormation ? "제외" : "배치",
                    Color = !isOwned
                        ? new Color(0.35f, 0.36f, 0.38f, 1f)
                        : args.InFormation
                        ? new Color(0.54f, 0.76f, 0.96f, 1f)
                        : new Color(0.54f, 0.78f, 0.22f, 1f)
                },
                LevelUp = new HeroDetailButtonViewState
                {
                    Interactable = isOwned,
                    Text = !isOwned
                        ? "레벨업\n미보유"
                        : maxLevel
                        ? "레벨업\nMAX"
                        : "레벨업\n" + NumberFormatter.Format(hero.LevelUpCost),
                    Color = !isOwned || maxLevel
                        ? new Color(0.26f, 0.27f, 0.29f, 1f)
                        : canPayLevelUp
                        ? new Color(0.54f, 0.78f, 0.22f, 1f)
                        : new Color(0.35f, 0.36f, 0.34f, 1f)
                },
                StarUp = new HeroDetailButtonViewState
                {
                    Interactable = isOwned,
                    Text = !isOwned
                        ? "승급\n미보유"
                        : maxStars
                        ? "승급\nMAX"
                        : "승급\n" + FormatCountNumber(hero.Shards) + "/" + FormatCountNumber(hero.StarUpCost),
                    Color = !isOwned || maxStars
                        ? new Color(0.26f, 0.27f, 0.29f, 1f)
                        : canStarUp
                        ? new Color(0.54f, 0.72f, 0.96f, 1f)
                        : new Color(0.35f, 0.36f, 0.34f, 1f)
                }
            };
        }

    }
}
