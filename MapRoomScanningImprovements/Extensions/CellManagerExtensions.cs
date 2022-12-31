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
        public static Dictionary<Int3, BatchCells> GetBatch2Cells(this CellManager cellManager)
        {
            return cellManager.batch2cells;
        }
    }
}
