using Harmony;
using MapRoomScanningImprovements.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapRoomScanningImprovements
{
    [HarmonyPatch(typeof(SubRoot))]
    [HarmonyPatch("OnPlayerEntered")]
    class SubRoot_OnPlayerEntered_Patch
    {
        static void Postfix(SubRoot __instance)
        {
            var mapRoom = __instance.GetComponentInChildren<MapRoomFunctionality>();
            if (mapRoom != null)
            {
                mapRoom.StartCoroutine(mapRoom.ScanInSleepingBatchCellsNotQueuesCoroutine());
            }
        }
    }
}
