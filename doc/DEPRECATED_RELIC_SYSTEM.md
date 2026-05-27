# Deprecated Relic System

Status: deprecated / on hold.

The previous Relic/Seongmul design is no longer part of the active MVP. The hero bottom tab that used to represent Relic has been replaced by Facility Dispatch.

Do not build new runtime work on the old Relic assumptions:

- Attribute relics that directly increase matching heroes' attack/HP are paused.
- `ElementRelicDefinition`, `HeroElement`, and `ElementRelicFragment` are removed from active runtime code.
- Relic-like long-term progression can be revisited later, but it must not overlap with Totem, Rune, Facility Dispatch, or Expedition-style resource systems.

Active replacement:

- Facility Dispatch handles timed resource production.
- Facilities produce Gold, Hero EXP Books, Equipment EXP Books, Totem Essence, Rune Boxes, and Transcend Stones.
- Facility upgrades use hunting materials: Wood, Brick, and Iron.
