using System;

namespace IdleGame.UI.Hud
{
    [Flags]
    public enum HudDirtyFlags
    {
        None = 0,
        Header = 1 << 0,
        Battle = 1 << 1,
        Growth = 1 << 2,
        Hero = 1 << 3,
        HeroDetail = 1 << 4,
        Fortress = 1 << 5,
        Facility = 1 << 6,
        Stage = 1 << 7,
        Summon = 1 << 8,
        Support = 1 << 9,
        Debug = 1 << 10,
        Navigation = 1 << 11,
        All = Header
            | Battle
            | Growth
            | Hero
            | HeroDetail
            | Fortress
            | Facility
            | Stage
            | Summon
            | Support
            | Debug
            | Navigation
    }
}
