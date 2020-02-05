using Harmony;
using MapRoomScanningImprovements.Extensions;
using MapRoomScanningImprovements.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UWE;

namespace MapRoomScanningImprovements
{
    [HarmonyPatch(typeof(MapRoomFunctionality))]
    [HarmonyPatch("Start")]
    class MapRoomFunctionality_StartScanning_Patch
    {
        private static Dictionary<MapRoomFunctionality, bool> scanActives = new Dictionary<MapRoomFunctionality, bool>();

        static void Postfix(MapRoomFunctionality __instance, bool ___scanActive)
        {
            // if (!scanActives.ContainsKey(__instance)) {
            //     scanActives.Add(__instance, ___scanActive);
            // }
            // else
            // {
            //    scanActives[__instance] = ___scanActive;
            // }

            // if (___scanActive)
            // {
                Logger.Debug(string.Format("Starting scan in sleeping BatchCells"));
                __instance.StartCoroutine(ScanInSleepingBatchCellsNotQueuesCoroutine(__instance));
            // }
        }

        private static IEnumerator ScanInSleepingBatchCellsNotQueuesCoroutine(MapRoomFunctionality mapRoom)
        {
            CellManager cellManager = LargeWorldStreamer.main.cellManager;

            while (UnityEngine.Time.timeScale <= 0)
            {
                yield return null;
            }

            using (var serializerProxy = ProtobufSerializerPool.GetProxy())
            {
                var loadedCells = cellManager.GetLoadedCells(mapRoom.transform.position);
                while (loadedCells.MoveNext())
                {
                    var entityCell = loadedCells.Current;
                    var serialData = entityCell.GetSerialData();

                    if (entityCell.liveRoot == null && serialData.Length > 0)
                    {
                        Logger.Debug(string.Format("Entity cell \"{0}\" is not awake and has serialData", entityCell.ToString()));

                        UnityEngine.GameObject liveRoot;

                        using (MemoryStream stream = new MemoryStream(serialData.Data, false))
                        {
                            bool headerDeserialized = serializerProxy.Value.TryDeserializeStreamHeader(stream);
                            if (headerDeserialized)
                            {
                                CoroutineTask<UnityEngine.GameObject> task = serializerProxy.Value.DeserializeObjectTreeAsync(stream, true, 0);

                                yield return task;
                                liveRoot = task.GetResult();
                            } 
                            else
                            {
                                liveRoot = null;
                            }
                        }

                        if (liveRoot)
                        {
                            Logger.Debug(string.Format("Entity cell \"{0}\" can have liveRoot {1}", entityCell.ToString(), liveRoot.ToString()));

                            var resourceTrackers = liveRoot.GetComponentsInChildren<ResourceTracker>(true);
                            foreach (var resourceTracker in resourceTrackers)
                            {
                                resourceTracker.Start();
                                Logger.Debug(string.Format("Entity cell \"{0}\" invoked \"Start\" for {1}", entityCell.ToString(), resourceTracker.gameObject.ToString()));
                            }
                        }
                        else
                        {
                            Logger.Debug(string.Format("Entity cell \"{0}\" can't have liveRoot", entityCell.ToString()));
                        }

                        // var scanActive = scanActives[mapRoom];
                        // if (!scanActive)
                        // {
                        //     break;
                        // }
                    }
                }
            }

            yield break;
        }
    }
}
