using HarmonyLib;
using MapRoomScanningImprovements.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapRoomScanningImprovements
{
    [HarmonyPatch(typeof(MapRoomFunctionality))]
    [HarmonyPatch(nameof(MapRoomFunctionality.OnResourceDiscovered))]
    class MapRoomFunctionality_OnResourceDiscovered_Patch
    {
        static bool Prefix(MapRoomFunctionality __instance, ResourceTrackerDatabase.ResourceInfo info)
        {
            return AddResourceNodeIfWithinScanRange(__instance, info);
        }

        static private bool AddResourceNodeIfWithinScanRange(MapRoomFunctionality mapRoom, ResourceTrackerDatabase.ResourceInfo info)
        {
            float scanRange = mapRoom.GetScanRange();
            float sqrScanRange = scanRange * scanRange;

            if (mapRoom.typeToScan == info.techType && (mapRoom.wireFrameWorld.position - info.position).sqrMagnitude <= sqrScanRange)
            {
                Logger.Debug(string.Format("Techtype \"{0}\" is within scan range.", info.techType));
                mapRoom.resourceNodes.Add(info);
            }
            else
            {
                if (mapRoom.typeToScan == info.techType)
                {
                    Logger.Debug(string.Format("Techtype \"{0}\" is out of scan range.", info.techType));
                }
            }

            return false;
        }
    }
}
