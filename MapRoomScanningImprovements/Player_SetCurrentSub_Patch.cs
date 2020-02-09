using Harmony;
using MapRoomScanningImprovements.Extensions;
using MapRoomScanningImprovements.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapRoomScanningImprovements
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("SetCurrentSub")]
    class Player_SetCurrentSub_Patch
    {
        private static List<SubRoot> coroutinesSubRoots = new List<SubRoot>();

        private static float waitSeconds = 0.016f;
        private static int numOfBatchRings = 3;

        static void Postfix(Player __instance, SubRoot sub)
        {
            if (sub)
            {
                Logger.Debug(string.Format("SetCurrentSub Postfix called"));

                if (!coroutinesSubRoots.Contains(sub))
                {
                    coroutinesSubRoots.Add(sub);
                }

                __instance.StartCoroutine(StartScanning(sub));
            }
        }

        private static IEnumerator StartScanning(SubRoot subRoot)
        {
            while (UnityEngine.Time.timeScale <= 0)
            {
                yield return null;
            }

            var mapRoom = subRoot.GetComponentInChildren<MapRoomFunctionality>();
            if (mapRoom != null)
            {
                mapRoom.StartCoroutine(Coroutine.waitFor(mapRoom.ScanInSleepingBatchCellsNotQueuesCoroutine(numOfBatchRings, 0), waitSeconds));
                mapRoom.StartCoroutine(Coroutine.waitFor(mapRoom.ScanInSleepingBatchCellsNotQueuesCoroutine(numOfBatchRings, 1), waitSeconds));
                mapRoom.StartCoroutine(Coroutine.waitFor(mapRoom.ScanInSleepingBatchCellsNotQueuesCoroutine(numOfBatchRings, 2), waitSeconds));
                mapRoom.StartCoroutine(Coroutine.waitFor(mapRoom.ScanInSleepingBatchCellsNotQueuesCoroutine(numOfBatchRings, 3), waitSeconds));
            }
        }
    }
}
