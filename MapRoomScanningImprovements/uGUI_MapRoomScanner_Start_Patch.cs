using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace MapRoomScanningImprovements
{
    [HarmonyPatch(typeof(uGUI_MapRoomScanner))]
    [HarmonyPatch(nameof(uGUI_MapRoomScanner.Start))]
    partial class uGUI_MapRoomScanner_Start_Patch
    {
        static void Postfix(uGUI_MapRoomScanner __instance)
        {
            var upgrading = new MapRoomScannerUpgrading(__instance);
            MapRoomScannerUpgrading.AddInstance(__instance, upgrading);
        }
    }
}
