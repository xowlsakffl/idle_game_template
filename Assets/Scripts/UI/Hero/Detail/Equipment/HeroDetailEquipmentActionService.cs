using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Detail.Equipment
{
    public sealed class HeroDetailEquipmentActionResult
    {
        public bool Success;
        public bool NeedsSelection;
        public bool SelectSlotFilter;
        public bool ClearSelectedEquipment;
        public bool ClearSelectedEquipmentDetail;
        public bool ClearSlotSelection;
        public bool CloseEquipmentDetailPopup;
        public bool CloseBulkDismantlePrompt;
        public string Message;
    }

    public static partial class HeroDetailEquipmentActionService
    {
    }
}
