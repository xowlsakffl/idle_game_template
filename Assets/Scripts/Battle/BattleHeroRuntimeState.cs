using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal sealed class BattleHeroRuntimeState
    {
        public BattleHeroRuntimeState(HeroState hero, Vector2 position, int slotIndex, float maxHp)
        {
            Hero = hero;
            Position = position;
            SlotIndex = slotIndex;
            MaxHp = Mathf.Max(1f, maxHp);
            Hp = MaxHp;
        }

        public HeroState Hero { get; }
        public Vector2 Position { get; set; }
        public int SlotIndex { get; set; }
        public float MaxHp { get; set; }
        public float Hp { get; set; }
        public float ReviveRemaining { get; set; }
        public bool IsAlive => Hp > 0f && ReviveRemaining <= 0f;
    }
}
