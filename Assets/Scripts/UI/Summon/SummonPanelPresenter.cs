using UnityEngine.UI;
using IdleGame.Economy;
using IdleGame.Gacha;

namespace IdleGame.UI.Summon
{
    public sealed class SummonPanelPresenterArgs
    {
        public GachaManager GachaManager;
        public EquipmentInventory EquipmentInventory;
        public Text ResultText;
        public bool RefreshPanel;
        public int EquipmentSummaryMaxLines = 6;
    }

    public static class SummonPanelPresenter
    {
        public static void Refresh(SummonPanelPresenterArgs args)
        {
            if (args == null || !args.RefreshPanel || args.ResultText == null)
            {
                return;
            }

            string result = args.GachaManager == null ? string.Empty : args.GachaManager.LastResult;
            string equipmentSummary = args.EquipmentInventory == null
                ? string.Empty
                : args.EquipmentInventory.GetOwnedSummary(args.EquipmentSummaryMaxLines);

            args.ResultText.text = result
                + "\n\n보유 장비"
                + "\n" + equipmentSummary;
        }
    }
}
