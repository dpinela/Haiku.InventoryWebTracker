using BepConfig = BepInEx.Configuration;

namespace Haiku.InventoryWebTracker
{
    internal class Settings
    {
        public BepConfig.ConfigEntry<bool> Enabled;

        public Settings(BepConfig.ConfigFile config)
        {
            Enabled = config.Bind("", "Enabled", false);
        }
    }
}