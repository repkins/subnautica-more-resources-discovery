using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MapRoomScanningImprovements.Extensions
{
    public static class CellManagerExtensions
    {
        public static IEnumerator<EntityCell> GetLoadedCells(this CellManager cellManager, UnityEngine.Vector3 position)
        {
            var batchToCellsField = typeof(CellManager).GetField("batch2cells", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var GetCellsMethod = typeof(BatchCells).GetMethod("GetCells", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var largeWorldStreamer = LargeWorldStreamer.main;

            var batchToCells = batchToCellsField.GetValue(cellManager) as Dictionary<Int3, BatchCells>;
            var orderedBatchCells = batchToCells
                .OrderBy(pair => (position - largeWorldStreamer.GetBatchCenter(pair.Key)).sqrMagnitude)
                .Select(pair => pair.Value);

            for (int i = 0; i < 4; i++)
            {
                foreach (var batchCells in orderedBatchCells)
                {
                    var cellsTier = GetCellsMethod.Invoke(batchCells, new object[] { i }) as Array3<EntityCell>;
                    var orderedCellsTier = cellsTier
                        .Where(cell => cell != null)
                        .OrderBy(cell => (position - cell.GetCenter()).sqrMagnitude);

                    foreach (var entityCell in orderedCellsTier)
                    {
                        if (entityCell != null)
                        {
                            yield return entityCell;
                        }
                    }
                }
            }
        }
    }
}
