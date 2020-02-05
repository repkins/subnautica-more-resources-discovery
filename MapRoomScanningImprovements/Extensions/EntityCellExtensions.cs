using MapRoomScanningImprovements.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MapRoomScanningImprovements.Extensions
{
    static class EntityCellExtensions
    {
        private static MethodInfo AwakeAsyncMethod = typeof(EntityCell).GetMethod("AwakeAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo SleepAsyncMethod = typeof(EntityCell).GetMethod("SleepAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerator AwakeAsync(this EntityCell entityCell, ProtobufSerializer serializer)
        {
            return AwakeAsyncMethod.Invoke(entityCell, new object[] { serializer }) as IEnumerator;
        }

        public static IEnumerator SleepAsync(this EntityCell entityCell, ProtobufSerializer serializer)
        {
            return SleepAsyncMethod.Invoke(entityCell, new object[] { serializer }) as IEnumerator;
        }
    }
}
