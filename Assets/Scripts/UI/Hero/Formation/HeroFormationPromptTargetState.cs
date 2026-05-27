using IdleGame.Data;
using IdleGame.UI.Hero;
using IdleGame.UI.Navigation;

namespace IdleGame.UI.Hero.Formation
{
    public enum HeroFormationPromptTargetKind
    {
        None,
        Tab,
        Preset,
        HeroPage
    }

    public sealed class HeroFormationPromptTargetState
    {
        public HeroFormationPromptTargetKind Kind { get; private set; }
        public int Preset { get; private set; }
        public HudTab Tab { get; private set; } = HudTab.Growth;
        public bool ContentOpen { get; private set; } = true;
        public HeroPageTab HeroPageTab { get; private set; } = HeroPageTab.Formation;

        public void Reset(HudTab activeTab, bool contentOpen, HeroPageTab activeHeroPageTab)
        {
            Kind = HeroFormationPromptTargetKind.None;
            Preset = 0;
            Tab = activeTab;
            ContentOpen = contentOpen;
            HeroPageTab = activeHeroPageTab;
        }

        public void SetTab(HudTab targetTab, bool targetContentOpen)
        {
            Kind = HeroFormationPromptTargetKind.Tab;
            Preset = 0;
            Tab = targetTab;
            ContentOpen = targetContentOpen;
            HeroPageTab = HeroPageTab.Formation;
        }

        public void SetPreset(int preset)
        {
            Kind = HeroFormationPromptTargetKind.Preset;
            Preset = ClampPreset(preset);
            ContentOpen = true;
            HeroPageTab = HeroPageTab.Formation;
        }

        public void SetHeroPage(HeroPageTab targetHeroPageTab)
        {
            Kind = HeroFormationPromptTargetKind.HeroPage;
            Preset = 0;
            HeroPageTab = targetHeroPageTab;
        }

        private static int ClampPreset(int preset)
        {
            if (preset < 1)
            {
                return 1;
            }

            return preset > GameData.MaxHeroPresets ? GameData.MaxHeroPresets : preset;
        }
    }
}
