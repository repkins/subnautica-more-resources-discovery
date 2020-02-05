using Harmony;
using MapRoomScanningImprovements.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MapRoomScanningImprovements
{
    public class MainPatcher
    {
        public static void Patch()
        {
            var harmony = HarmonyInstance.Create("subnautica.repkins.map-room-scanning-improvements");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.Info("Successfully patched");
        }
    }
}
