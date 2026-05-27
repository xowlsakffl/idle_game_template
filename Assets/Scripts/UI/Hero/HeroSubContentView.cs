using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Hero.TotemRune;
using IdleGame.UI.Hero.Trait;

namespace IdleGame.UI.Hero
{
    public sealed class HeroSubContentViewRefs
    {
        public GameObject TraitContent;
        public Text TraitSummaryText;
        public Text TraitDetailText;
        public Button TraitLevelUpButton;
        public GameObject TotemContent;
        public Text TotemSummaryText;
        public Text TotemDetailText;
        public Button TotemEquipButton;
        public Button TotemLevelUpButton;
        public GameObject RuneContent;
        public Text RuneSummaryText;
        public Text RuneDetailText;
        public Button RuneEquipButton;
        public Button RuneLevelUpButton;
    }

    public sealed class HeroSubContentViewBuildArgs
    {
        public Transform Parent;
        public IReadOnlyList<TotemDefinition> Totems;
        public IReadOnlyList<RuneDefinition> Runes;
        public Action<string> OnTalentSelected;
        public Action OnTraitLevelUp;
        public Func<bool> CanTraitLevelUp;
        public Action<string> OnSelectTotem;
        public Action<string> OnTotemAction;
        public Action OnEquipSelectedTotem;
        public Action OnLevelUpTotem;
        public Func<bool> CanLevelUpTotem;
        public Action<string> OnSelectRune;
        public Action<string> OnRuneAction;
        public Action OnEquipSelectedRune;
        public Action OnLevelUpRune;
        public Func<bool> CanLevelUpRune;
        public Dictionary<string, Button> TalentButtons;
        public Dictionary<string, Text> TalentButtonTexts;
        public Dictionary<string, Button> TotemButtons;
        public Dictionary<string, Text> TotemButtonTexts;
        public Dictionary<string, Button> TotemActionButtons;
        public Dictionary<string, Button> RuneButtons;
        public Dictionary<string, Text> RuneButtonTexts;
        public Dictionary<string, Button> RuneActionButtons;
    }

    public static class HeroSubContentView
    {
        public static HeroSubContentViewRefs Build(HeroSubContentViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new HeroSubContentViewRefs();
            }

            HeroTraitViewRefs traitRefs = HeroTraitView.Build(new HeroTraitViewBuildArgs
            {
                Parent = args.Parent,
                OnTalentSelected = args.OnTalentSelected,
                OnLevelUp = args.OnTraitLevelUp,
                CanLevelUp = args.CanTraitLevelUp,
                TalentButtons = args.TalentButtons,
                TalentButtonTexts = args.TalentButtonTexts
            });

            HeroTotemViewRefs totemRefs = HeroTotemView.Build(new HeroTotemViewBuildArgs
            {
                Parent = args.Parent,
                Totems = args.Totems,
                OnSelectTotem = args.OnSelectTotem,
                OnTotemAction = args.OnTotemAction,
                OnEquipSelected = args.OnEquipSelectedTotem,
                OnLevelUp = args.OnLevelUpTotem,
                CanLevelUp = args.CanLevelUpTotem,
                TotemButtons = args.TotemButtons,
                TotemButtonTexts = args.TotemButtonTexts,
                TotemActionButtons = args.TotemActionButtons
            });

            HeroRuneViewRefs runeRefs = HeroRuneView.Build(new HeroRuneViewBuildArgs
            {
                Parent = args.Parent,
                Runes = args.Runes,
                OnSelectRune = args.OnSelectRune,
                OnRuneAction = args.OnRuneAction,
                OnEquipSelected = args.OnEquipSelectedRune,
                OnLevelUp = args.OnLevelUpRune,
                CanLevelUp = args.CanLevelUpRune,
                RuneButtons = args.RuneButtons,
                RuneButtonTexts = args.RuneButtonTexts,
                RuneActionButtons = args.RuneActionButtons
            });

            return new HeroSubContentViewRefs
            {
                TraitContent = traitRefs.Content,
                TraitSummaryText = traitRefs.SummaryText,
                TraitDetailText = traitRefs.DetailText,
                TraitLevelUpButton = traitRefs.LevelUpButton,
                TotemContent = totemRefs.Content,
                TotemSummaryText = totemRefs.SummaryText,
                TotemDetailText = totemRefs.DetailText,
                TotemEquipButton = totemRefs.EquipButton,
                TotemLevelUpButton = totemRefs.LevelUpButton,
                RuneContent = runeRefs.Content,
                RuneSummaryText = runeRefs.SummaryText,
                RuneDetailText = runeRefs.DetailText,
                RuneEquipButton = runeRefs.EquipButton,
                RuneLevelUpButton = runeRefs.LevelUpButton
            };
        }
    }
}
