using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MapRoomScanningImprovements.Extensions
{
    static class ResourceTrackerExtensions
    {
        private static MethodInfo StartMethod = typeof(ResourceTracker).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo OnDestroyMethod = typeof(ResourceTracker).GetMethod("OnDestroy", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public static void Start(this ResourceTracker resourceTracker)
        {
            StartMethod.Invoke(resourceTracker, new object[] { });
        }
        public static void OnDestroy(this ResourceTracker resourceTracker)
        {
            OnDestroyMethod.Invoke(resourceTracker, new object[] { });
        }
    }
}
