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
        
        public static void Start(this ResourceTracker resourceTracker)
        {
            StartMethod.Invoke(resourceTracker, new object[] { });
        }
    }
}
