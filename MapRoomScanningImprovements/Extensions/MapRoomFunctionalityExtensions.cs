using MapRoomScanningImprovements.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UWE;

namespace MapRoomScanningImprovements.Extensions
{
    static class MapRoomFunctionalityExtensions
    {
        private static LargeWorldStreamer largeWorldStreamer = LargeWorldStreamer.main;
        private static CellManager cellManager = LargeWorldStreamer.main.cellManager;

        private static float waitSeconds = 0.016f;
        private static int numOfBatchRings = 3;

        private static WorkerThread workerThread = ThreadUtils.StartWorkerThread("I/O", "ScannerThread", System.Threading.ThreadPriority.BelowNormal, -2, 32);

        public static IEnumerator ScanInSleepingBatchCellsNotQueuesCoroutine(this MapRoomFunctionality mapRoom, int level)
        {
            while (UnityEngine.Time.timeScale <= 0)
            {
                yield return null;
            }
            Logger.Debug(string.Format("Starting scan in sleeping BatchCells of level {0}", level));

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

                        batchCells = cellManager.InitializeBatchCells(batch);

                        var loadBatchCellsTask = new LoadBatchCellsTask(cellManager, batchCells);
                        UWE.Utils.EnqueueWrap(workerThread, loadBatchCellsTask);
                        yield return new AsyncAwaiter(loadBatchCellsTask);

                        batchVisible = false;
                    }

                    Logger.Debug(string.Format("Getting cells of batch \"{0}\"", batchCells.batch));

                    var cellsTier = batchCells.GetCells(level);
                    var orderedCellsTier = cellsTier
                        .Where(cell => cell != null)
                        .OrderBy(cell => (mapRoom.transform.position - cell.GetCenter()).sqrMagnitude);

                    foreach (var entityCell in orderedCellsTier)
                    {
                        Logger.Debug(string.Format("Getting cell {0} of batch \"{1}\"", entityCell.CellId, batchCells.batch));

                        if (entityCell != null && !entityCell.IsProcessing())
                        {
                            var serialData = entityCell.GetSerialData();
                            if (entityCell.liveRoot == null && serialData.Length > 0)
                            {
                                Logger.Debug(string.Format("Entity cell \"{0}\" is not awake and has serialData", entityCell.ToString()));

                                UnityEngine.GameObject liveRoot;

                                using (MemoryStream stream = new MemoryStream(serialData.Data.Array, serialData.Data.Offset, serialData.Data.Length, false))
                                {
                                    bool headerDeserialized = serializerProxy.Value.TryDeserializeStreamHeader(stream);
                                    if (headerDeserialized)
                                    {
                                        CoroutineTask<UnityEngine.GameObject> task = serializerProxy.Value.DeserializeObjectTreeAsync(stream, true, 0);

                                        yield return waitFor(task, waitSeconds);
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
                            }

                            var waiterData = entityCell.GetWaiterData();
                            if (waiterData.Length > 0)
                            {
                                Logger.Debug(string.Format("Entity cell \"{0}\" is not awake and has waiterData", entityCell.ToString()));

                                UnityEngine.GameObject waiterRoot;

                                using (MemoryStream stream = new MemoryStream(waiterData.Data.Array, waiterData.Data.Offset, waiterData.Data.Length, false))
                                {
                                    while (stream.Position < waiterData.Length)
                                    {
                                        CoroutineTask<UnityEngine.GameObject> task = serializerProxy.Value.DeserializeObjectTreeAsync(stream, true, 0);

                                        yield return waitFor(task, waitSeconds);
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
                                        }
                                    }
                                }
                            }

                            var legacyData = entityCell.GetLegacyData();
                            if (legacyData.Length > 0)
                            {
                                Logger.Debug(string.Format("Entity cell \"{0}\" has legacyData", entityCell.ToString()));

                                UnityEngine.GameObject legacyRoot;

                                using (MemoryStream stream = new MemoryStream(legacyData.Data.Array, legacyData.Data.Offset, legacyData.Data.Length, false))
                                {
                                    bool headerDeserialized = serializerProxy.Value.TryDeserializeStreamHeader(stream);
                                    if (headerDeserialized)
                                    {
                                        CoroutineTask<UnityEngine.GameObject> task = serializerProxy.Value.DeserializeObjectTreeAsync(stream, true, 0);

                                        yield return waitFor(task, waitSeconds);
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
                                }
                            }
                        }
                    }

                    if (!batchVisible)
                    {
                        Logger.Debug(string.Format("Unloading cells of batch \"{0}\"", batch));
                        cellManager.UnloadBatchCells(batch);
                    };
                }
            }
            Logger.Debug(string.Format("Finishing scan in sleeping BatchCells of level {0}", level));

            yield break;
        }

        private static IEnumerator waitFor(IEnumerator enumetator, float seconds)
        {
            if (enumetator == null)
            {
                yield break;
            }

            float cyclesWithinFrame = 1;

            Stack<IEnumerator> stack = new Stack<IEnumerator>();
            stack.Push(enumetator);
            while (stack.Count > 0)
            {
                if (UnityEngine.Time.timeScale <= 0)
                {
                    yield return null;
                    continue;
                }

                if (cyclesWithinFrame < 1)
                {
                    yield return null;
                    cyclesWithinFrame += UnityEngine.Time.unscaledDeltaTime / seconds;
                    Logger.Debug(string.Format("Skip: Cycles {0}", cyclesWithinFrame));
                    continue;
                }

                IEnumerator currentEnumetator = stack.Peek();

                if (currentEnumetator.MoveNext())
                {
                    var currentValue = currentEnumetator.Current;
                    if (currentValue is IEnumerator innerEnumerator)
                    {
                        stack.Push(innerEnumerator);
                    }
                    else
                    {
                        if (cyclesWithinFrame < 2)
                        {
                            cyclesWithinFrame--;

                            yield return null;
                            cyclesWithinFrame += UnityEngine.Time.unscaledDeltaTime / seconds;
                            Logger.Debug(string.Format("Next: Cycles {0}", cyclesWithinFrame));
                        } 
                        else
                        {
                            cyclesWithinFrame--;
                        }
                    }
                }
                else
                {
                    stack.Pop();
                }
            }
        }
    
        private sealed class LoadBatchCellsTask : IWorkerTask, IAsyncOperation
        {
            public LoadBatchCellsTask(CellManager cellManager, BatchCells batchCells)
            {
                this.cellManager = cellManager;
                this.batchCells = batchCells;
            }

            public void Execute()
            {
                try
                {
                    this.cellManager.LoadBatchCellsThreaded(this.batchCells, false);
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
                finally
                {
                    this.isDone = true;
                }
            }

            // Token: 0x17000376 RID: 886
            // (get) Token: 0x06003752 RID: 14162 RVA: 0x00124C70 File Offset: 0x00122E70
            public bool isDone { get; private set; }

            // Token: 0x06003753 RID: 14163 RVA: 0x00124C78 File Offset: 0x00122E78
            public override string ToString()
            {
                return string.Format("LoadBatchCellsTask {0}", this.batchCells.batch);
            }

            // Token: 0x04003534 RID: 13620
            private readonly CellManager cellManager;

            // Token: 0x04003535 RID: 13621
            private readonly BatchCells batchCells;
        }
    }
}
