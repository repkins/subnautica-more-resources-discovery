using System.Collections.Generic;

namespace MapRoomScanningImprovements
{ 
    class MapRoomScannerUpgrading
    {
        private static Dictionary<uGUI_MapRoomScanner, MapRoomScannerUpgrading> Instances = new Dictionary<uGUI_MapRoomScanner, MapRoomScannerUpgrading>();

        private uGUI_MapRoomScanner uGuiMapRoomScanner;

        public MapRoomScannerUpgrading(uGUI_MapRoomScanner uGuiMapRoomScanner)
        {
            this.uGuiMapRoomScanner = uGuiMapRoomScanner;

            uGuiMapRoomScanner.mapRoom.storageContainer.container.onAddItem += OnAddItem;
            uGuiMapRoomScanner.mapRoom.storageContainer.container.onRemoveItem += OnRemoveItem;
        }

        private void OnAddItem(InventoryItem inventoryItem)
        {
            if (inventoryItem.item.GetTechType() == TechType.MapRoomUpgradeScanRange)
            {
                uGuiMapRoomScanner.UpdateAvailableTechTypes();
            }
        }

        private void OnRemoveItem(InventoryItem inventoryItem)
        {
            if (inventoryItem.item.GetTechType() == TechType.MapRoomUpgradeScanRange)
            {
                uGuiMapRoomScanner.UpdateAvailableTechTypes();
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
