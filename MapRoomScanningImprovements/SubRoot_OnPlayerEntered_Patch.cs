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
        private static List<SubRoot> coroutinesSubRoots = new List<SubRoot>();

        static void Postfix(SubRoot __instance)
        {
            var mapRoom = __instance.GetComponentInChildren<MapRoomFunctionality>();
            if (mapRoom != null && !coroutinesSubRoots.Contains(__instance))
            {
                coroutinesSubRoots.Add(__instance);

                mapRoom.StartCoroutine(mapRoom.ScanInSleepingBatchCellsNotQueuesCoroutine(0));
                mapRoom.StartCoroutine(mapRoom.ScanInSleepingBatchCellsNotQueuesCoroutine(1));
                mapRoom.StartCoroutine(mapRoom.ScanInSleepingBatchCellsNotQueuesCoroutine(2));
                mapRoom.StartCoroutine(mapRoom.ScanInSleepingBatchCellsNotQueuesCoroutine(3));
            }
        }
    }
}
