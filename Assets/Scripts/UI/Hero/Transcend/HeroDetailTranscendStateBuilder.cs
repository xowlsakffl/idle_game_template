using System.Collections.Generic;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;
using IdleGame.UI.Hero;
using IdleGame.UI.Hero.Detail;

namespace IdleGame.UI.Hero.Transcend
{
    public sealed class HeroDetailTranscendStateBuildArgs
    {
        public HeroState Hero;
        public int SelectedSlotIndex;
        public bool[] LockedSlots;
        public CurrencyWallet Wallet;
        public bool StopOnlySs;
        public bool AutoRolling;
    }

    public sealed class HeroDetailTranscendSlotViewState
    {
        public int SlotIndex;
        public string Text;
        public Color ButtonColor;
        public bool LockVisible;
        public string LockText;
        public Color LockColor;
    }

    public sealed class HeroDetailTranscendViewState
    {
        public string SummaryText;
        public readonly List<HeroDetailTranscendSlotViewState> Slots = new List<HeroDetailTranscendSlotViewState>();
        public HeroDetailButtonViewState StopButton;
        public HeroDetailButtonViewState ChangeButton;
        public HeroDetailButtonViewState AutoButton;
    }

    public static class HeroDetailTranscendStateBuilder
    {
        public static HeroDetailTranscendViewState Build(HeroDetailTranscendStateBuildArgs args)
        {
            if (args == null || args.Hero == null)
            {
                return null;
            }

            HeroState hero = args.Hero;
            int selectedSlot = Mathf.Clamp(args.SelectedSlotIndex, 0, HeroDefinition.MaxTranscendSlots - 1);
            int selectedRequiredStars = HeroDefinition.GetTranscendRequiredStars(selectedSlot);
            bool selectedUnlocked = hero.IsTranscendSlotUnlocked(selectedSlot);
            int unlockedSlotCount = HeroTranscendRules.CountUnlockedSlots(hero);
            int lockedSlotCount = HeroTranscendRules.CountLockedSlots(hero, args.LockedSlots);
            int changeableSlotCount = HeroTranscendRules.CountChangeableSlots(hero, args.LockedSlots);

            var state = new HeroDetailTranscendViewState();
            for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
            {
                bool unlocked = hero.IsTranscendSlotUnlocked(i);
                bool selected = selectedSlot == i;
                bool locked = unlocked && HeroTranscendRules.IsLocked(args.LockedSlots, i);
                state.Slots.Add(BuildSlot(hero, i, selected, unlocked, locked));
            }

            int rollCost = HeroTranscendRules.GetRollCost(hero, args.LockedSlots);
            bool canRoll = changeableSlotCount > 0 && args.Wallet != null && args.Wallet.HeroTranscendStone >= rollCost;
            state.SummaryText = "초월 슬롯 " + (selectedSlot + 1)
                + " / " + HeroDefinition.MaxTranscendSlots
                + "    " + (selectedUnlocked ? "선택됨" : selectedRequiredStars + "성 필요")
                + "\n열림 " + unlockedSlotCount + "칸 / 잠김 " + lockedSlotCount + "칸 / 변경 " + changeableSlotCount + "칸";
            state.StopButton = new HeroDetailButtonViewState
            {
                Interactable = true,
                Text = (args.StopOnlySs ? "[x] " : "[ ] ") + "자동 변경시 SS만 정지",
                Color = args.StopOnlySs
                    ? new Color(0.46f, 0.62f, 0.30f, 1f)
                    : new Color(0.26f, 0.32f, 0.43f, 1f)
            };
            state.ChangeButton = new HeroDetailButtonViewState
            {
                Interactable = true,
                Text = "변경\n" + rollCost,
                Color = canRoll ? new Color(0.28f, 0.72f, 0.92f, 1f) : new Color(0.35f, 0.36f, 0.38f, 1f)
            };
            state.AutoButton = new HeroDetailButtonViewState
            {
                Interactable = true,
                Text = args.AutoRolling
                    ? "자동 변경\n중지"
                    : args.StopOnlySs ? "자동 변경\nSS 정지" : "자동 변경\nS 이상 정지",
                Color = args.AutoRolling
                    ? new Color(0.86f, 0.52f, 0.16f, 1f)
                    : canRoll ? new Color(0.70f, 0.24f, 0.82f, 1f) : new Color(0.35f, 0.36f, 0.38f, 1f)
            };

            return state;
        }

        private static HeroDetailTranscendSlotViewState BuildSlot(HeroState hero, int slotIndex, bool selected, bool unlocked, bool locked)
        {
            int requiredStars = HeroDefinition.GetTranscendRequiredStars(slotIndex);
            string optionId = hero.GetTranscendOptionId(slotIndex);
            HeroTranscendOptionDefinition option = string.IsNullOrEmpty(optionId)
                ? null
                : GameData.GetHeroTranscendOption(optionId);

            string text;
            if (unlocked && option != null)
            {
                text = "<size=34><color=" + HeroUiText.GetTranscendGradeHex(option.Grade) + ">" + option.Grade + "</color></size>"
                    + "  [" + option.ScopeLabel + "] " + option.Description
                    + "\n<size=22>슬롯 " + (slotIndex + 1) + "  해금 " + requiredStars + "성  가중치 " + option.ProbabilityWeight.ToString("0.####") + "</size>";
            }
            else if (unlocked)
            {
                text = "<size=34>옵션 없음</size>\n<size=22>변경을 눌러 옵션을 부여하세요.</size>";
            }
            else
            {
                text = "<size=30>잠김</size>  " + StarUiText.FormatStars(requiredStars)
                    + "\n<size=22>" + requiredStars + "성부터 추가 초월 가능</size>";
            }

            if (unlocked && locked)
            {
                text = "[잠금] " + text;
            }

            Color buttonColor = unlocked
                ? selected
                    ? Color.Lerp(HeroUiText.GetTranscendGradeColor(option != null ? option.Grade : HeroTranscendGrade.F), new Color(1f, 0.92f, 0.42f, 1f), 0.34f)
                    : HeroUiText.GetTranscendGradeColor(option != null ? option.Grade : HeroTranscendGrade.F)
                : new Color(0.19f, 0.22f, 0.30f, 1f);
            if (locked)
            {
                buttonColor = new Color(0.18f, 0.20f, 0.24f, 1f);
            }

            return new HeroDetailTranscendSlotViewState
            {
                SlotIndex = slotIndex,
                Text = text,
                ButtonColor = buttonColor,
                LockVisible = unlocked,
                LockText = locked ? "잠금" : "열림",
                LockColor = locked
                    ? new Color(0.72f, 0.46f, 0.16f, 1f)
                    : new Color(0.20f, 0.25f, 0.36f, 1f)
            };
        }

    }
}
