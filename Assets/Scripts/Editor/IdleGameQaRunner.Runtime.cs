using System;
using System.Collections.Generic;
using UnityEngine;
using IdleGame.App;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Gacha;
using IdleGame.Progression;
using IdleGame.Save;
using IdleGame.Speed;

namespace IdleGame.Editor
{
    public static partial class IdleGameQaRunner
    {
        private sealed class RuntimeHarness : IDisposable
        {
            private GameObject root;

            public RuntimeHarness()
            {
                root = new GameObject("IdleGameQaRuntime");
                Save = root.AddComponent<SaveManager>();
                Progress = root.AddComponent<StageProgressManager>();
                Wallet = root.AddComponent<CurrencyWallet>();
                Dungeon = root.AddComponent<DungeonProgressManager>();
                Abilities = root.AddComponent<AbilityManager>();
                EquipmentInventory = root.AddComponent<EquipmentInventory>();
                Speed = root.AddComponent<GameSpeedManager>();
                Battle = root.AddComponent<BattleManager>();
                Gacha = root.AddComponent<GachaManager>();

                Progress.Initialize(Save);
                Wallet.Initialize(Save);
                Dungeon.Initialize(Save, Wallet);
                Abilities.Initialize(Wallet, Save);
                EquipmentInventory.Initialize(Save);
                Speed.Initialize(Save);
                Battle.Initialize(Progress, Wallet, Save, Abilities, Speed);
                Battle.InitializeDungeon(Dungeon);
                Gacha.Initialize(Battle, Wallet, EquipmentInventory);
            }

            public SaveManager Save { get; }
            public StageProgressManager Progress { get; }
            public CurrencyWallet Wallet { get; }
            public DungeonProgressManager Dungeon { get; }
            public AbilityManager Abilities { get; }
            public EquipmentInventory EquipmentInventory { get; }
            public GameSpeedManager Speed { get; }
            public BattleManager Battle { get; }
            public GachaManager Gacha { get; }

            public void Dispose()
            {
                if (root != null)
                {
                    UnityEngine.Object.DestroyImmediate(root);
                    root = null;
                }
            }
        }

        private sealed class PlayerPrefsScope : IDisposable
        {
            private readonly List<PrefEntry> snapshot = new List<PrefEntry>();

            public PlayerPrefsScope()
            {
                foreach (PrefDescriptor pref in GetKnownPrefs())
                {
                    snapshot.Add(PrefEntry.Capture(pref));
                }
            }

            public void ClearKnownKeys()
            {
                foreach (PrefDescriptor pref in GetKnownPrefs())
                {
                    PlayerPrefs.DeleteKey(pref.Key);
                }

                PlayerPrefs.Save();
            }

            public void Dispose()
            {
                ClearKnownKeys();
                foreach (PrefEntry entry in snapshot)
                {
                    entry.Restore();
                }

                PlayerPrefs.Save();
            }

            private static IEnumerable<PrefDescriptor> GetKnownPrefs()
            {
                yield return PrefDescriptor.String(SaveKeys.Gold);
                yield return PrefDescriptor.String(SaveKeys.Ruby);
                yield return PrefDescriptor.String(SaveKeys.HeroExpItem);
                yield return PrefDescriptor.String(SaveKeys.EquipmentExpItem);
                yield return PrefDescriptor.String(SaveKeys.TotemEssence);
                yield return PrefDescriptor.String(SaveKeys.RuneDust);
                yield return PrefDescriptor.String(SaveKeys.Wood);
                yield return PrefDescriptor.String(SaveKeys.Brick);
                yield return PrefDescriptor.String(SaveKeys.Iron);
                yield return PrefDescriptor.String(SaveKeys.HeroTranscendStone);
                yield return PrefDescriptor.String(SaveKeys.HeroSummonTicket);
                yield return PrefDescriptor.String(SaveKeys.EquipmentSummonTicket);
                yield return PrefDescriptor.String(SaveKeys.DungeonTicket);
                yield return PrefDescriptor.String(SaveKeys.DungeonFreeEntryDate);
                yield return PrefDescriptor.Int(SaveKeys.DungeonFreeEntriesUsed);
                yield return PrefDescriptor.Int(SaveKeys.AccountLevel);
                yield return PrefDescriptor.String(SaveKeys.AccountExperience);
                yield return PrefDescriptor.Int(SaveKeys.DebugTalentPointBonus);
                yield return PrefDescriptor.String(SaveKeys.HighestStageId);
                yield return PrefDescriptor.String(SaveKeys.CurrentStageId);
                yield return PrefDescriptor.String(SaveKeys.SelectedStageId);
                yield return PrefDescriptor.String(SaveKeys.ProgressMode);
                yield return PrefDescriptor.Int(SaveKeys.ChapterOneBossCleared);
                yield return PrefDescriptor.String(SaveKeys.LastOnlineUtcTicks);
                yield return PrefDescriptor.String(SaveKeys.CombatSpeedMultiplier);
                yield return PrefDescriptor.Int(SaveKeys.HasFourTimesSpeedEntitlement);
                yield return PrefDescriptor.Int(SaveKeys.SkillAutoEnabled);
                yield return PrefDescriptor.Int(SaveKeys.FeverAutoEnabled);
                yield return PrefDescriptor.Int(SaveKeys.HeroTranscendStopOnlySs);
                yield return PrefDescriptor.Int(SaveKeys.HeroFormationPreset);
                yield return PrefDescriptor.Int(SaveKeys.FortressLevel);
                yield return PrefDescriptor.String(SaveKeys.FortressExperience);

                foreach (DungeonKind kind in Enum.GetValues(typeof(DungeonKind)))
                {
                    yield return PrefDescriptor.Int(SaveKeys.DungeonHighestClearLevel(DungeonProgressManager.GetId(kind)));
                }

                foreach (GachaPoolDefinition pool in GachaPoolDefinitions.All)
                {
                    yield return PrefDescriptor.Int(SaveKeys.GachaTotalPulls(pool.Id));
                    yield return PrefDescriptor.Int(SaveKeys.GachaPityCount(pool.Id));
                }

                foreach (GachaEventTargetDefinition target in GachaEventTargetDefinitions.All)
                {
                    yield return PrefDescriptor.Int(SaveKeys.GachaPityCount(GachaPoolDefinitions.Get(GachaPoolKind.Event).Id, target.Id));
                }

                foreach (HeroDefinition hero in GameData.Heroes)
                {
                    yield return PrefDescriptor.Int(SaveKeys.HeroLevel(hero.Id));
                    yield return PrefDescriptor.Int(SaveKeys.HeroShards(hero.Id));
                    yield return PrefDescriptor.Int(SaveKeys.HeroStars(hero.Id));
                    for (int slot = 0; slot < HeroDefinition.MaxTranscendSlots; slot++)
                    {
                        yield return PrefDescriptor.String(SaveKeys.HeroTranscendOption(hero.Id, slot));
                        yield return PrefDescriptor.Int(SaveKeys.HeroTranscendOptionRolled(hero.Id, slot));
                        yield return PrefDescriptor.Int(SaveKeys.HeroTranscendLocked(hero.Id, slot));
                    }

                    foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                    {
                        yield return PrefDescriptor.String(SaveKeys.HeroEquipmentSlot(hero.Id, slot));
                    }
                }

                foreach (EquipmentDefinition equipment in GameData.Equipments)
                {
                    yield return PrefDescriptor.Int(SaveKeys.EquipmentLevel(equipment.Id));
                    yield return PrefDescriptor.Int(SaveKeys.EquipmentStars(equipment.Id));
                    yield return PrefDescriptor.Int(SaveKeys.EquipmentCount(equipment.Id));
                }

                foreach (AbilityDefinition ability in GameData.Abilities)
                {
                    yield return PrefDescriptor.Int(SaveKeys.AbilityLevel(ability.Kind));
                }

                for (int preset = 1; preset <= GameData.MaxHeroPresets; preset++)
                {
                    for (int slot = 0; slot < GameData.MaxPartyHeroes; slot++)
                    {
                        yield return PrefDescriptor.String(SaveKeys.HeroFormationSlot(preset, slot));
                    }

                    yield return PrefDescriptor.String(SaveKeys.HeroFormationTotem(preset));
                    for (int slot = 2; slot <= GameData.MaxRuneSlots; slot++)
                    {
                        yield return PrefDescriptor.String(SaveKeys.HeroFormationTotem(preset, slot));
                    }

                    for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
                    {
                        yield return PrefDescriptor.String(SaveKeys.HeroFormationRune(preset, slot));
                    }
                }

                foreach (TotemDefinition totem in GameData.Totems)
                {
                    yield return PrefDescriptor.Int(SaveKeys.TotemLevel(totem.Id));
                    yield return PrefDescriptor.Int(SaveKeys.TotemGrade(totem.Id));
                    yield return PrefDescriptor.Int(SaveKeys.TotemUnlocked(totem.Id));
                }

                foreach (RuneDefinition rune in GameData.Runes)
                {
                    yield return PrefDescriptor.Int(SaveKeys.RuneLevel(rune.Id));
                    yield return PrefDescriptor.Int(SaveKeys.RuneGrade(rune.Id));
                    yield return PrefDescriptor.Int(SaveKeys.RuneCopies(rune.Id));
                    yield return PrefDescriptor.Int(SaveKeys.RuneUnlocked(rune.Id));
                    foreach (RuneGrade grade in Enum.GetValues(typeof(RuneGrade)))
                    {
                        yield return PrefDescriptor.Int(SaveKeys.RuneCount(rune.Id, grade));
                    }
                }

                foreach (FacilityDefinition facility in GameData.Facilities)
                {
                    yield return PrefDescriptor.Int(SaveKeys.FacilityLevel(facility.Id));
                    yield return PrefDescriptor.String(SaveKeys.FacilityStoredAmount(facility.Id));
                    yield return PrefDescriptor.String(SaveKeys.FacilityLastUpdateUtcTicks(facility.Id));
                    for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
                    {
                        yield return PrefDescriptor.String(SaveKeys.FacilityAssignedHero(facility.Id, slot));
                    }
                }

                foreach (TalentDefinition talent in TalentData.Talents)
                {
                    yield return PrefDescriptor.Int(SaveKeys.TalentLevel(talent.Id));
                }
            }
        }

        private enum PrefKind
        {
            String,
            Int
        }

        private readonly struct PrefDescriptor
        {
            private PrefDescriptor(string key, PrefKind kind)
            {
                Key = key;
                Kind = kind;
            }

            public string Key { get; }
            public PrefKind Kind { get; }

            public static PrefDescriptor String(string key)
            {
                return new PrefDescriptor(key, PrefKind.String);
            }

            public static PrefDescriptor Int(string key)
            {
                return new PrefDescriptor(key, PrefKind.Int);
            }
        }

        private readonly struct PrefEntry
        {
            private readonly string key;
            private readonly PrefKind kind;
            private readonly bool exists;
            private readonly string stringValue;
            private readonly int intValue;

            private PrefEntry(PrefDescriptor pref, bool exists, string stringValue, int intValue)
            {
                key = pref.Key;
                kind = pref.Kind;
                this.exists = exists;
                this.stringValue = stringValue;
                this.intValue = intValue;
            }

            public static PrefEntry Capture(PrefDescriptor pref)
            {
                bool exists = PlayerPrefs.HasKey(pref.Key);
                string stringValue = pref.Kind == PrefKind.String ? PlayerPrefs.GetString(pref.Key, string.Empty) : string.Empty;
                int intValue = pref.Kind == PrefKind.Int ? PlayerPrefs.GetInt(pref.Key, 0) : 0;
                return new PrefEntry(pref, exists, stringValue, intValue);
            }

            public void Restore()
            {
                if (exists)
                {
                    if (kind == PrefKind.String)
                    {
                        PlayerPrefs.SetString(key, stringValue);
                    }
                    else
                    {
                        PlayerPrefs.SetInt(key, intValue);
                    }
                }
            }
        }
    }
}
