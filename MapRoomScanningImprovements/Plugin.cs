using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapRoomScanningImprovements
{
    [BepInPlugin("subnautica.repkins.map-room-scanning-improvements", "More Resources Discovery", "1.1.1.0")]
    public class Plugin: BaseUnityPlugin
    {
        public void Awake()
        {
            MainPatcher.Patch();
        }
    }
}
