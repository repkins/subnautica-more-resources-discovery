using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MapRoomScanningImprovements.Extensions
{
    static class EntityCellExtensions
    {
        private static MethodInfo IsProcessingMethod = typeof(EntityCell).GetMethod("IsProcessing", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsProcessing(this EntityCell entityCell)
        {
            return (bool)IsProcessingMethod.Invoke(entityCell, new object[] { });
        }
    }
}
