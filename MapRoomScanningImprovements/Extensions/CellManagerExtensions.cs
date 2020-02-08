using MapRoomScanningImprovements.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MapRoomScanningImprovements.Extensions
{
    public static class CellManagerExtensions
    {
        public static IEnumerator<EntityCell> GetScanCells(this CellManager cellManager, UnityEngine.Vector3 position, int level)
        {
            var batch2CellsField = typeof(CellManager).GetField("batch2cells", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var GetCellsMethod = typeof(BatchCells).GetMethod("GetCells", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var largeWorldStreamer = LargeWorldStreamer.main;
            var batch2Cells = batch2CellsField.GetValue(cellManager) as Dictionary<Int3, BatchCells>;

            var containingBatch = largeWorldStreamer.GetContainingBatch(position);
            var scanBounds = new Int3.Bounds(containingBatch - 3, containingBatch + 3);

            var orderedScanBatches = scanBounds.ToEnumerable()
                .OrderBy(batch => (position - largeWorldStreamer.GetBatchCenter(batch)).sqrMagnitude);

            foreach (var batch in orderedScanBatches)
            {
                Logger.Debug(string.Format("Now at batch \"{0}\"", batch));

                var batchVisible = true;
                if (!batch2Cells.TryGetValue(batch, out var batchCells))
                {
                    Logger.Debug(string.Format("Loading cells of batch \"{0}\"", batch));

                    batchCells = cellManager.InitializeBatchCells(batch);
                    
                    cellManager.LoadBatchCellsThreaded(batchCells, false);

                    batchVisible = false;
                }

                Logger.Debug(string.Format("Getting cells of batch \"{0}\"", batchCells.batch));

                var cellsTier = GetCellsMethod.Invoke(batchCells, new object[] { level }) as Array3<EntityCell>;
                var orderedCellsTier = cellsTier
                    .Where(cell => cell != null)
                    .OrderBy(cell => (position - cell.GetCenter()).sqrMagnitude);

                foreach (var entityCell in orderedCellsTier)
                {
                    Logger.Debug(string.Format("Getting cell {0} of batch \"{1}\"", entityCell.CellId, batchCells.batch));

                    if (entityCell != null)
                    {
                        yield return entityCell;
                    }
                }

                if (!batchVisible)
                {
                    Logger.Debug(string.Format("Unloading cells of batch \"{0}\"", batch));
                    cellManager.UnloadBatchCells(batch);
                };
            }
        }
    }
}
