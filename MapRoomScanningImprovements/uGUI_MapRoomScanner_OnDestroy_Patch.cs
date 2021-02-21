using HarmonyLib;
using MapRoomScanningImprovements.Extensions;
using MapRoomScanningImprovements.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MapRoomScanningImprovements
{
    [HarmonyPatch(typeof(uGUI_MapRoomScanner))]
    [HarmonyPatch("OnDestroy")]
    class uGUI_MapRoomScanner_OnDestroy_Patch
    {
        static void Postfix(uGUI_MapRoomScanner __instance)
        {
            MapRoomScannerUpgrading.RemoveInstance(__instance);
        }
    }
}
