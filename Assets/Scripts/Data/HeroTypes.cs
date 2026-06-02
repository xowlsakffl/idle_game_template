using System;

namespace IdleGame.Data
{
    [Serializable]
    public enum HeroRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    [Serializable]
    public enum HeroTrait
    {
        Melee,
        Ranged,
        Support,
        Defense
    }

    [Serializable]
    public enum HeroPassiveStat
    {
        AttackPower,
        MaxHp,
        AttackSpeed,
        MoveSpeed
    }

    [Serializable]
    public enum HeroTranscendGrade
    {
        F,
        E,
        D,
        C,
        B,
        A,
        S,
        SS
    }

    [Serializable]
    public enum HeroTranscendOptionScope
    {
        Common,
        Exclusive
    }

}
