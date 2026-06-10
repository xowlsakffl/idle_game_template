namespace IdleGame.UI.Hud
{
    public static class HudWalletDirtyFlagResolver
    {
        public static HudDirtyFlags Resolve(HudWalletSnapshot previous, HudWalletSnapshot current)
        {
            if (!current.IsValid)
            {
                return HudDirtyFlags.None;
            }

            if (!previous.IsValid)
            {
                return HudDirtyFlags.Header
                    | HudDirtyFlags.Growth
                    | HudDirtyFlags.Hero
                    | HudDirtyFlags.HeroDetail
                    | HudDirtyFlags.Facility
                    | HudDirtyFlags.Stage
                    | HudDirtyFlags.Summon
                    | HudDirtyFlags.Debug
                    | HudDirtyFlags.Navigation;
            }

            HudDirtyFlags flags = HudDirtyFlags.None;
            bool anyChanged = false;

            if (previous.Gold != current.Gold)
            {
                flags |= HudDirtyFlags.Header | HudDirtyFlags.Growth | HudDirtyFlags.Navigation;
                anyChanged = true;
            }

            if (previous.Ruby != current.Ruby)
            {
                flags |= HudDirtyFlags.Header | HudDirtyFlags.Summon | HudDirtyFlags.Navigation;
                anyChanged = true;
            }

            if (previous.HeroExpItem != current.HeroExpItem)
            {
                flags |= HudDirtyFlags.Hero | HudDirtyFlags.HeroDetail | HudDirtyFlags.Navigation;
                anyChanged = true;
            }

            if (previous.EquipmentExpItem != current.EquipmentExpItem)
            {
                flags |= HudDirtyFlags.HeroDetail;
                anyChanged = true;
            }

            if (previous.TotemEssence != current.TotemEssence)
            {
                flags |= HudDirtyFlags.Hero;
                anyChanged = true;
            }

            if (previous.Wood != current.Wood || previous.Brick != current.Brick || previous.Iron != current.Iron)
            {
                flags |= HudDirtyFlags.Facility;
                anyChanged = true;
            }

            if (previous.HeroTranscendStone != current.HeroTranscendStone)
            {
                flags |= HudDirtyFlags.HeroDetail;
                anyChanged = true;
            }

            if (previous.HeroSummonTicket != current.HeroSummonTicket
                || previous.EquipmentSummonTicket != current.EquipmentSummonTicket)
            {
                flags |= HudDirtyFlags.Summon | HudDirtyFlags.Navigation;
                anyChanged = true;
            }

            if (previous.DungeonTicket != current.DungeonTicket)
            {
                flags |= HudDirtyFlags.Stage | HudDirtyFlags.Navigation;
                anyChanged = true;
            }

            if (anyChanged)
            {
                flags |= HudDirtyFlags.Stage | HudDirtyFlags.Debug;
            }

            return flags;
        }
    }
}
