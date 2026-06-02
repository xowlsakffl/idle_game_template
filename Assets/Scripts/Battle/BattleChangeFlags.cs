using System;

namespace IdleGame.Battle
{
    [Flags]
    public enum BattleChangeFlags
    {
        None = 0,
        Combat = 1 << 0,
        HeroProgression = 1 << 1,
        Formation = 1 << 2,
        Fortress = 1 << 3,
        Facility = 1 << 4,
        TotemRune = 1 << 5,
        AutoControl = 1 << 6,
        BattleLog = 1 << 7,
        All = Combat
            | HeroProgression
            | Formation
            | Fortress
            | Facility
            | TotemRune
            | AutoControl
            | BattleLog
    }
}
