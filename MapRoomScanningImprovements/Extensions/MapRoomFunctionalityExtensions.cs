﻿using MapRoomScanningImprovements.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UWE;

namespace MapRoomScanningImprovements.Extensions
{
    static class MapRoomFunctionalityExtensions
    {
        public static IEnumerator ScanInSleepingBatchCellsNotQueuesCoroutine(this MapRoomFunctionality mapRoom, int numOfBatchRings)
        {
            var watch = new Stopwatch();
            watch.Start();

            Logger.Info(string.Format("Starting scan in sleeping/unloaded BatchCells"));

            var largeWorldStreamer = LargeWorldStreamer.main;
            var cellManager = LargeWorldStreamer.main.cellManager;

            var batch2Cells = cellManager.GetBatch2Cells();

            var containingBatch = largeWorldStreamer.GetContainingBatch(mapRoom.transform.position);
            var scanBounds = new Int3.Bounds(containingBatch - numOfBatchRings, containingBatch + numOfBatchRings);

            var orderedScanBatches = scanBounds.ToEnumerable()
                .OrderBy(batch => (mapRoom.transform.position - largeWorldStreamer.GetBatchCenter(batch)).sqrMagnitude);

            using (var serializerProxy = ProtobufSerializerPool.GetProxy())
            {
                foreach (var batch in orderedScanBatches)
                {
                    Logger.Debug(string.Format("Now at batch \"{0}\"", batch));

                    var batchVisible = true;
                    if (!batch2Cells.TryGetValue(batch, out var batchCells))
                    {
                        Logger.Debug(string.Format("Loading cells of batch \"{0}\"", batch));

                        batchCells = BatchCells.GetFromPool(cellManager, largeWorldStreamer, batch);

                        yield return cellManager.LoadBatchCellsThreadedAsync(batchCells, false);

                        batchVisible = false;
                    }

                    for (int level = 0; level <= 3; level++)
                    {
                        Logger.Debug(string.Format("Getting cells level {0} of batch \"{1}\"", level, batchCells.batch));

                        var cellsTier = batchCells.GetCells(level);
                        var orderedCellsTier = cellsTier
                            .Where(cell => cell != null)
                            .OrderBy(cell => (mapRoom.transform.position - cell.GetCenter()).sqrMagnitude);

                        foreach (var entityCell in orderedCellsTier)
                        {
                            Logger.Debug(string.Format("Getting cell {0} of batch \"{1}\"", entityCell.CellId, batchCells.batch));

                            if (entityCell != null && !entityCell.IsProcessing())
                            {
                                SerialData serialData;
                                if (batchVisible)
                                {
                                    serialData = new SerialData();
                                    serialData.CopyFrom(entityCell.GetSerialData());
                                }
                                else
                                {
                                    serialData = entityCell.GetSerialData();
                                }

                                if (serialData.Length > 0)
                                {
                                    Logger.Debug(string.Format("Entity cell \"{0}\" is not awake and has serialData", entityCell.ToString()));

                                    UnityEngine.GameObject liveRoot;

                                    using (MemoryStream stream = new MemoryStream(serialData.Data.Array, serialData.Data.Offset, serialData.Data.Length, false))
                                    {
                                        if (serializerProxy.Value.TryDeserializeStreamHeader(stream))
                                        {
                                            CoroutineTask<UnityEngine.GameObject> task = serializerProxy.Value.DeserializeObjectTreeAsync(stream, true, entityCell.AllowSpawnRestrictions, 0);

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

                                        UnityEngine.Object.Destroy(liveRoot);
                                    }
                                    else
                                    {
                                        Logger.Debug(string.Format("Entity cell \"{0}\" can't have liveRoot", entityCell.ToString()));
                                    }
                                }

                                SerialData waiterData;
                                if (batchVisible)
                                {
                                    waiterData = new SerialData();
                                    waiterData.CopyFrom(entityCell.GetWaiterData());
                                }
                                else
                                {
                                    waiterData = entityCell.GetWaiterData();
                                }

                                if (waiterData.Length > 0)
                                {
                                    Logger.Debug(string.Format("Entity cell \"{0}\" is not awake and has waiterData", entityCell.ToString()));

                                    UnityEngine.GameObject waiterRoot;

                                    using (MemoryStream stream = new MemoryStream(waiterData.Data.Array, waiterData.Data.Offset, waiterData.Data.Length, false))
                                    {
                                        while (stream.Position < waiterData.Length)
                                        {
                                            CoroutineTask<UnityEngine.GameObject> task = serializerProxy.Value.DeserializeObjectTreeAsync(stream, true, entityCell.AllowSpawnRestrictions, 0);

                                            yield return task;
                                            waiterRoot = task.GetResult();
                                            if (waiterRoot)
                                            {
                                                Logger.Debug(string.Format("Entity cell \"{0}\" can have waiterRoot {1}", entityCell.ToString(), waiterRoot.ToString()));

                                                var resourceTrackers = waiterRoot.GetComponentsInChildren<ResourceTracker>(true);
                                                foreach (var resourceTracker in resourceTrackers)
                                                {
                                                    resourceTracker.Start();
                                                    Logger.Debug(string.Format("Entity cell \"{0}\" invoked \"Start\" for {1}", entityCell.ToString(), resourceTracker.gameObject.ToString()));
                                                }

                                                UnityEngine.Object.Destroy(waiterRoot);
                                            }
                                        }
                                    }
                                }

                                SerialData legacyData;
                                if (batchVisible)
                                {
                                    legacyData = new SerialData();
                                    legacyData.CopyFrom(entityCell.GetLegacyData());
                                }
                                else
                                {
                                    legacyData = entityCell.GetLegacyData();
                                }

                                if (legacyData.Length > 0)
                                {
                                    Logger.Debug(string.Format("Entity cell \"{0}\" has legacyData", entityCell.ToString()));

                                    UnityEngine.GameObject legacyRoot;

                                    using (MemoryStream stream = new MemoryStream(legacyData.Data.Array, legacyData.Data.Offset, legacyData.Data.Length, false))
                                    {
                                        if (serializerProxy.Value.TryDeserializeStreamHeader(stream))
                                        {
                                            CoroutineTask<UnityEngine.GameObject> task = serializerProxy.Value.DeserializeObjectTreeAsync(stream, true, entityCell.AllowSpawnRestrictions, 0);

                                            yield return task;
                                            legacyRoot = task.GetResult();
                                        }
                                        else
                                        {
                                            legacyRoot = null;
                                        }
                                    }

                                    if (legacyRoot)
                                    {
                                        Logger.Debug(string.Format("Entity cell \"{0}\" can have legacyRoot {1}", entityCell.ToString(), legacyRoot.ToString()));

                                        var resourceTrackers = legacyRoot.GetComponentsInChildren<ResourceTracker>(true);
                                        foreach (var resourceTracker in resourceTrackers)
                                        {
                                            resourceTracker.Start();
                                            Logger.Debug(string.Format("Entity cell \"{0}\" invoked \"Start\" for {1}", entityCell.ToString(), resourceTracker.gameObject.ToString()));
                                        }

                                        UnityEngine.Object.Destroy(legacyRoot);
                                    }
                                }
                            }
                        }
                    }

                    if (!batchVisible)
                    {
                        Logger.Debug(string.Format("Unloading cells of batch \"{0}\"", batch));
                        BatchCells.ReturnToPool(batchCells);
                    };

                    yield return null;
                }
            }
            watch.Stop();

            Logger.Info(string.Format("Finishing scan in sleeping/unloaded BatchCells in {0} ms", watch.ElapsedMilliseconds));

            yield break;
        }
    }
}
