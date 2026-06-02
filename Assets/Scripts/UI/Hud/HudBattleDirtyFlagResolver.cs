using IdleGame.Battle;

namespace IdleGame.UI.Hud
{
    public static class HudBattleDirtyFlagResolver
    {
        public static HudDirtyFlags Resolve(BattleChangeFlags battleFlags)
        {
            if (battleFlags == BattleChangeFlags.None)
            {
                return HudDirtyFlags.None;
            }

            if ((battleFlags & BattleChangeFlags.All) == BattleChangeFlags.All)
            {
                return HudDirtyFlags.Header
                    | HudDirtyFlags.Battle
                    | HudDirtyFlags.Hero
                    | HudDirtyFlags.HeroDetail
                    | HudDirtyFlags.Fortress
                    | HudDirtyFlags.Facility
                    | HudDirtyFlags.Support
                    | HudDirtyFlags.Debug
                    | HudDirtyFlags.Navigation;
            }

            HudDirtyFlags hudFlags = HudDirtyFlags.None;

            if (HasAny(battleFlags, BattleChangeFlags.Combat | BattleChangeFlags.BattleLog))
            {
                hudFlags |= HudDirtyFlags.Battle | HudDirtyFlags.Support | HudDirtyFlags.Debug | HudDirtyFlags.Navigation;
            }

            if (HasAny(battleFlags, BattleChangeFlags.HeroProgression))
            {
                hudFlags |= HudDirtyFlags.Header
                    | HudDirtyFlags.Battle
                    | HudDirtyFlags.Hero
                    | HudDirtyFlags.HeroDetail
                    | HudDirtyFlags.Facility
                    | HudDirtyFlags.Debug
                    | HudDirtyFlags.Navigation;
            }

            if (HasAny(battleFlags, BattleChangeFlags.Formation))
            {
                hudFlags |= HudDirtyFlags.Header
                    | HudDirtyFlags.Battle
                    | HudDirtyFlags.Hero
                    | HudDirtyFlags.HeroDetail
                    | HudDirtyFlags.Support
                    | HudDirtyFlags.Debug
                    | HudDirtyFlags.Navigation;
            }

            if (HasAny(battleFlags, BattleChangeFlags.Fortress))
            {
                hudFlags |= HudDirtyFlags.Header
                    | HudDirtyFlags.Battle
                    | HudDirtyFlags.Fortress
                    | HudDirtyFlags.Debug
                    | HudDirtyFlags.Navigation;
            }

            if (HasAny(battleFlags, BattleChangeFlags.Facility))
            {
                hudFlags |= HudDirtyFlags.Facility | HudDirtyFlags.Debug;
            }

            if (HasAny(battleFlags, BattleChangeFlags.TotemRune))
            {
                hudFlags |= HudDirtyFlags.Header
                    | HudDirtyFlags.Battle
                    | HudDirtyFlags.Hero
                    | HudDirtyFlags.HeroDetail
                    | HudDirtyFlags.Support
                    | HudDirtyFlags.Debug
                    | HudDirtyFlags.Navigation;
            }

            if (HasAny(battleFlags, BattleChangeFlags.AutoControl))
            {
                hudFlags |= HudDirtyFlags.Battle | HudDirtyFlags.Debug;
            }

            return hudFlags;
        }

        private static bool HasAny(BattleChangeFlags source, BattleChangeFlags flags)
        {
            return (source & flags) != 0;
        }
    }
}
