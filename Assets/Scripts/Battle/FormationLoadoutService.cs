using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Save;
using UnityEngine;

namespace IdleGame.Battle
{
    internal readonly struct FormationLoadoutChangeResult
    {
        public FormationLoadoutChangeResult(bool changed, int preset, int slot, string battleLog)
        {
            Changed = changed;
            Preset = preset;
            Slot = slot;
            BattleLog = battleLog ?? string.Empty;
        }

        public bool Changed { get; }
        public int Preset { get; }
        public int Slot { get; }
        public string BattleLog { get; }
    }

    internal static partial class FormationLoadoutService
    {
        private const string UnequippedRuneId = "__NONE__";



    }
}
