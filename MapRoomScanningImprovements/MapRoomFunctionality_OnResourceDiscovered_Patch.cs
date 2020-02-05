using Harmony;
using MapRoomScanningImprovements.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapRoomScanningImprovements
{
    [HarmonyPatch(typeof(MapRoomFunctionality))]
    [HarmonyPatch("OnResourceDiscovered")]
    class MapRoomFunctionality_OnResourceDiscovered_Patch
    {
        static bool Prefix(MapRoomFunctionality __instance, List<ResourceTracker.ResourceInfo> ___resourceNodes, ResourceTracker.ResourceInfo info)
        {
            return MapRoomFunctionality_OnResourceDiscovered_Patch.AddResourceNodeIfWithinScanRange(__instance, ___resourceNodes, info);
        }

        static private bool AddResourceNodeIfWithinScanRange(MapRoomFunctionality mapRoom, List<ResourceTracker.ResourceInfo> resourceNodes, ResourceTracker.ResourceInfo info)
        {
            float scanRange = mapRoom.GetScanRange();
            float sqrScanRange = scanRange * scanRange;

            if (mapRoom.typeToScan == info.techType && (mapRoom.wireFrameWorld.position - info.position).sqrMagnitude <= sqrScanRange)
            {
                Logger.Debug(string.Format("Techtype \"{0}\" is within scan range.", info.techType));
                resourceNodes.Add(info);
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
