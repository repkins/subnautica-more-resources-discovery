using HarmonyLib;
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
            var harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "subnautica.repkins.map-room-scanning-improvements");
            Logger.Info("Successfully patched");

            Config.Load();
            Logger.Info("Config successfully loaded");
        }
    }
}
