using HarmonyLib;
using MapRoomScanningImprovements.Extensions;
using MapRoomScanningImprovements.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWE;

namespace MapRoomScanningImprovements
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch(nameof(Player.SetCurrentSub))]
    class Player_SetCurrentSub_Patch
    {
        private static List<SubRoot> coroutinesSubRoots = new List<SubRoot>();

        static void Postfix(Player __instance, SubRoot sub)
        {
            if (sub)
            {
                Logger.Debug(string.Format("SetCurrentSub Postfix called"));

                if (!coroutinesSubRoots.Contains(sub))
                {
                    coroutinesSubRoots.Add(sub);

                    __instance.StartCoroutine(StartScanning(sub));
                }
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
                var maxFrameMs = Config.Instance.maxFrameMS;
                var numOfBatchRings = Config.Instance.numOfBatchRings;

                var pooledScanStateMachine = CoroutineUtils.PumpCoroutine(mapRoom.ScanInSleepingBatchCellsNotQueuesCoroutine(numOfBatchRings), "ScanForResources", maxFrameMs);

                mapRoom.StartCoroutine(pooledScanStateMachine);
            }
        }
    }
}
