global using System;
using Bep = BepInEx;

namespace Haiku.InventoryWebTracker
{
    [Bep.BepInPlugin("haiku.inventorywebtracker", "Haiku Inventory Web Tracker", "1.0.0.0")]
    [Bep.BepInDependency("haiku.mapi", "1.0")]
    public class InventoryWebTrackerPlugin : Bep.BaseUnityPlugin
    {
        public void Start()
        {
            modSettings = new(Config);
        }

        private Settings? modSettings;
    }
}
