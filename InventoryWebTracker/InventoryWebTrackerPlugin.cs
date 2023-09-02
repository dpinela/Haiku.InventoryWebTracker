global using System;
using Bep = BepInEx;
using static EmbedIO.WebApi.WebApiModuleExtensions;
using static EmbedIO.WebServerOptionsExtensions;
using static EmbedIO.WebModuleContainerExtensions;
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

        public void Update()
        {
            if (modSettings!.Enabled.Value && uiServer == null)
            {
                uiServer = new EmbedIO.WebServer(opt => opt
                    .WithUrlPrefix(url)
                    .WithMode(EmbedIO.HttpListenerMode.EmbedIO)
                ).WithWebApi("/", m => m.WithController<WebUI>());
                uiServer.RunAsync();
            }
            else if (!modSettings!.Enabled.Value && uiServer != null)
            {
                uiServer.Dispose();
                uiServer = null;
            }
        }

        private const string url = "http://localhost:8086";

        private Settings? modSettings;
        private EmbedIO.WebServer? uiServer;
    }
}
