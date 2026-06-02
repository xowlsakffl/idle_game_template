using System.Collections.Generic;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;
using IdleGame.UI.Hero;

namespace IdleGame.UI.Hero.Detail
{
    public static partial class HeroDetailStateBuilder
    {
        public static HeroDetailEquipmentDetailViewState BuildEquipmentDetail(HeroDetailEquipmentDetailStateBuildArgs args)
        {
            if (args == null || args.State == null)
            {
                return null;
            }

            EquipmentState state = args.State;
            bool levelCap = state.Level >= state.MaxLevel;
            bool absoluteMaxLevel = state.IsMaxStars && levelCap;
            bool canPayLevelUp = args.Wallet != null && args.Wallet.EquipmentExpItem >= state.LevelUpCost;
            bool canStarUp = !state.IsMaxStars && args.StarMaterialCount >= state.StarUpCost;
            string ownersText = string.IsNullOrEmpty(args.EquippedOwners)
                ? "\n보유 x" + state.Count + " / 남음 " + args.AvailableCount
                : "\n장착중 " + args.EquippedOwners + "\n보유 x" + state.Count + " / 남음 " + args.AvailableCount;

            return new HeroDetailEquipmentDetailViewState
            {
                IconText = "Lv." + state.Level
                    + "\n" + state.Definition.SlotLabel
                    + "\n" + StarUiText.FormatStars(state.Stars),
                IconColor = Color.Lerp(HeroUiText.GetRarityColor(state.Definition.Rarity), Color.white, 0.15f),
                MetaText = state.Definition.SlotLabel
                    + "    " + state.Definition.RarityLabel
                    + ownersText,
                TitleText = state.Definition.DisplayName
                    + "\n<size=28>Lv. " + state.Level + "/" + state.MaxLevel
                    + "    " + state.Stars + "/" + EquipmentDefinition.MaxStars + "</size>",
                StatsText = "공격력"
                    + "\n+" + NumberFormatter.Format(state.AttackBonus)
                    + "\n체력"
                    + "\n+" + NumberFormatter.Format(state.HpBonus),
                SetText = EquipmentUiText.BuildDetailEffectText(state),
                BookText = "장비책 " + NumberFormatter.Format(args.Wallet != null ? args.Wallet.EquipmentExpItem : 0),
                NoticeText = args.NoticeText ?? string.Empty,
                EquipButton = new HeroDetailButtonViewState
                {
                    Interactable = args.EquippedToHero || args.AvailableCount > 0,
                    Text = args.EquippedToHero ? "해제" : args.AvailableCount > 0 ? "장착" : "남은 장비 없음",
                    Color = args.EquippedToHero
                        ? new Color(0.54f, 0.76f, 0.96f, 1f)
                        : args.AvailableCount > 0 ? new Color(0.54f, 0.78f, 0.22f, 1f) : new Color(0.35f, 0.36f, 0.34f, 1f)
                },
                LevelUpButton = new HeroDetailButtonViewState
                {
                    Interactable = true,
                    Text = absoluteMaxLevel
                        ? "레벨업\nMAX"
                        : levelCap
                        ? "레벨업\n승급 필요"
                        : "레벨업\n" + NumberFormatter.Format(state.LevelUpCost),
                    Color = absoluteMaxLevel || levelCap
                        ? new Color(0.34f, 0.35f, 0.36f, 1f)
                        : canPayLevelUp ? new Color(0.54f, 0.78f, 0.22f, 1f) : new Color(0.35f, 0.36f, 0.34f, 1f)
                },
                StarUpButton = new HeroDetailButtonViewState
                {
                    Interactable = true,
                    Text = state.IsMaxStars
                        ? "승급\nMAX"
                        : "승급\n" + FormatCountNumber(args.StarMaterialCount) + "/" + FormatCountNumber(state.StarUpCost),
                    Color = state.IsMaxStars
                        ? new Color(0.34f, 0.35f, 0.36f, 1f)
                        : canStarUp ? new Color(0.88f, 0.62f, 0.16f, 1f) : new Color(0.35f, 0.36f, 0.34f, 1f)
                }
            };
        }

        public static HeroDetailEquipmentDismantleViewState BuildEquipmentDismantle(HeroDetailEquipmentDismantleStateBuildArgs args)
        {
            if (args == null)
            {
                return null;
            }

            string selectedLabel = args.SelectedCount <= 0
                ? string.Empty
                : "    선택 " + args.SelectedCount + "개 / 보상 +" + NumberFormatter.Format(args.SelectedReward);
            bool hasSelection = args.SelectedCount > 0;

            return new HeroDetailEquipmentDismantleViewState
            {
                SummaryText = "필터 " + EquipmentUiText.BuildFilterSummaryLabel(args.SelectedSlots)
                    + "    표시 " + args.VisibleCount
                    + selectedLabel,
                EmptyVisible = args.VisibleCount <= 0,
                NoticeText = args.NoticeText ?? string.Empty,
                DismantleButton = new HeroDetailButtonViewState
                {
                    Interactable = true,
                    Text = hasSelection
                        ? "선택 분해\n" + args.SelectedCount + "개 +" + NumberFormatter.Format(args.SelectedReward)
                        : "선택 분해",
                    Color = hasSelection ? new Color(0.54f, 0.76f, 0.96f, 1f) : new Color(0.35f, 0.38f, 0.44f, 1f)
                }
            };
        }

        public static HeroDetailEquipmentBulkDismantleViewState BuildEquipmentBulkDismantle(HeroDetailEquipmentBulkDismantleStateBuildArgs args)
        {
            if (args == null)
            {
                return null;
            }

            return new HeroDetailEquipmentBulkDismantleViewState
            {
                InfoText = "해당 등급 이하의 전체 장비를 일괄 분해합니다."
                    + "\n장착 중인 장비는 분해하지 않습니다."
                    + "\n대상 " + args.Count + "개 / 장비책+" + NumberFormatter.Format(args.Reward),
                RarityText = HeroUiText.GetRarityLabel(args.SelectedRarity),
                RarityColor = Color.Lerp(HeroUiText.GetRarityColor(args.SelectedRarity), Color.white, 0.16f),
                NoticeText = args.NoticeText ?? string.Empty
            };
        }

        public static HeroDetailEquipmentContentViewState BuildEquipmentContent(HeroDetailEquipmentContentStateBuildArgs args)
        {
            if (args == null)
            {
                return null;
            }

            string slotLabel = args.SlotSelectionActive
                ? "    장착 칸 " + EquipmentUiText.GetSlotLabel(args.SelectedSlot)
                : string.Empty;
            string selectedLabel = args.SelectedState != null
                ? "    선택 " + args.SelectedState.Definition.DisplayName + " 후 슬롯 클릭"
                : string.Empty;

            return new HeroDetailEquipmentContentViewState
            {
                SummaryText = "필터 " + EquipmentUiText.BuildFilterSummaryLabel(args.SelectedSlots)
                    + "    보유 " + args.OwnedCount
                    + "    표시 " + args.VisibleCount
                    + slotLabel
                    + selectedLabel,
                EmptyText = "표시할 장비가 없습니다.",
                EmptyVisible = args.VisibleCount <= 0
            };
        }

        public static HeroDetailEquipmentContentViewState BuildEquipmentContentLocked()
        {
            return new HeroDetailEquipmentContentViewState
            {
                SummaryText = "미보유 영웅은 장비를 장착할 수 없습니다.",
                EmptyText = "뽑기로 조각을 획득하면 장비 장착이 열립니다.",
                EmptyVisible = true
            };
        }

        private static string BuildResourceText(HeroDetailTab activeTab, CurrencyWallet wallet)
        {
            if (activeTab == HeroDetailTab.Equipment)
            {
                return "장비책 " + NumberFormatter.Format(wallet != null ? wallet.EquipmentExpItem : 0);
            }

            if (activeTab == HeroDetailTab.Transcend)
            {
                return "초월석 " + FormatCountNumber(wallet != null ? wallet.HeroTranscendStone : 0);
            }

            return "경험치책  " + NumberFormatter.Format(wallet != null ? wallet.HeroExpItem : 0);
        }

        private static string FormatCountNumber(long value)
        {
            return GameData.ClampCount(value).ToString("#,0");
        }
    }
}
