using MapRoomScanningImprovements.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MapRoomScanningImprovements.Extensions
{
    static class MapRoomFunctionalityExtensions
    {
        public static IEnumerator ScanInSleepingBatchCellsNotQueuesCoroutine(this MapRoomFunctionality mapRoom, int level)
        {
            while (UnityEngine.Time.timeScale <= 0)
            {
                yield return null;
            }

            CellManager cellManager = LargeWorldStreamer.main.cellManager;
            float waitSeconds = 0.016f;

            Logger.Debug(string.Format("Starting scan in sleeping BatchCells of level {0}", level));
            using (var serializerProxy = ProtobufSerializerPool.GetProxy())
            {
                var loadedCells = cellManager.GetLoadedCells(mapRoom.transform.position, level);
                while (loadedCells.MoveNext())
                {
                    var entityCell = loadedCells.Current;
                    if (!entityCell.IsProcessing())
                    {
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
                    }                    
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
    }
}
