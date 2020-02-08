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
        private static FieldInfo batch2CellsField = typeof(CellManager).GetField("batch2cells", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo GetCellsMethod = typeof(BatchCells).GetMethod("GetCells", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public static Dictionary<Int3, BatchCells> GetBatch2Cells(this CellManager cellManager)
        {
            return batch2CellsField.GetValue(cellManager) as Dictionary<Int3, BatchCells>;
        }

        public static Array3<EntityCell> GetCells(this BatchCells batchCells, int level)
        {
            return GetCellsMethod.Invoke(batchCells, new object[] { level }) as Array3<EntityCell>;
        }
    }
}
