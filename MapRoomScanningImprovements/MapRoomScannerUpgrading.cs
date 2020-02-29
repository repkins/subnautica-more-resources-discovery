using System.Collections.Generic;
using System.Reflection;

namespace MapRoomScanningImprovements
{ 
    class MapRoomScannerUpgrading
    {
        private static Dictionary<uGUI_MapRoomScanner, MapRoomScannerUpgrading> Instances = new Dictionary<uGUI_MapRoomScanner, MapRoomScannerUpgrading>();
        private static MethodInfo UpdateAvailableTechTypesMethod = typeof(uGUI_MapRoomScanner).GetMethod("UpdateAvailableTechTypes", BindingFlags.Instance | BindingFlags.NonPublic);

        private uGUI_MapRoomScanner uGuiMapRoomScanner;

        public MapRoomScannerUpgrading(uGUI_MapRoomScanner uGuiMapRoomScanner)
        {
            this.uGuiMapRoomScanner = uGuiMapRoomScanner;

            uGuiMapRoomScanner.mapRoom.storageContainer.container.onAddItem += this.OnAddItem;
            uGuiMapRoomScanner.mapRoom.storageContainer.container.onRemoveItem += this.OnRemoveItem;
        }

        private void OnAddItem(InventoryItem inventoryItem)
        {
            if (inventoryItem.item.GetTechType() == TechType.MapRoomUpgradeScanRange)
            {
                UpdateAvailableTechTypesMethod.Invoke(uGuiMapRoomScanner, new object[] { });
            }
        }

        private void OnRemoveItem(InventoryItem inventoryItem)
        {
            if (inventoryItem.item.GetTechType() == TechType.MapRoomUpgradeScanRange)
            {
                UpdateAvailableTechTypesMethod.Invoke(uGuiMapRoomScanner, new object[] { });
            }
        }

        public static void AddInstance(uGUI_MapRoomScanner uGuiMapRoomScanner, MapRoomScannerUpgrading upgrading)
        {
            Instances.Add(uGuiMapRoomScanner, upgrading);
        }

        public static void RemoveInstance(uGUI_MapRoomScanner uGuiMapRoomScanner)
        {
            Instances.Remove(uGuiMapRoomScanner);
        }

        ~MapRoomScannerUpgrading()
        {
            uGuiMapRoomScanner.mapRoom.storageContainer.container.onAddItem -= this.OnAddItem;
            uGuiMapRoomScanner.mapRoom.storageContainer.container.onRemoveItem -= this.OnRemoveItem;
        }
    }
}
