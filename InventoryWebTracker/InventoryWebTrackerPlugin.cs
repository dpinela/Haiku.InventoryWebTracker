global using System;
using SyncCollections = System.Collections.Concurrent;
using Tasks = System.Threading.Tasks;
using Bep = BepInEx;
using UE = UnityEngine;
using static EmbedIO.WebApi.WebApiModuleExtensions;
using static EmbedIO.WebServerOptionsExtensions;
using static EmbedIO.WebModuleContainerExtensions;
namespace Haiku.InventoryWebTracker
{
    [Bep.BepInPlugin("haiku.inventorywebtracker", "Haiku Inventory Web Tracker", "1.0.0.0")]
    [Bep.BepInDependency("haiku.mapi", "1.0")]
    public class InventoryWebTrackerPlugin : Bep.BaseUnityPlugin
    {
        private static InventoryWebTrackerPlugin? plugin;
        internal static InventoryWebTrackerPlugin Instance => plugin!;

        public void Start()
        {
            plugin = this;
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

            if (iconFetchQueue.TryDequeue(out var task))
            {
                task.RunSynchronously();
            }
        }

        private const string url = "http://localhost:8086";

        private Settings? modSettings;
        private EmbedIO.WebServer? uiServer;

        private SyncCollections.ConcurrentQueue<Tasks.Task<byte[]>> iconFetchQueue = new();

        internal Tasks.Task<byte[]> ToPNGAsync(Func<UE.Sprite> getter)
        {
            var task = new Tasks.Task<byte[]>(() => ToPNG(getter()));
            iconFetchQueue.Enqueue(task);
            return task;
        }

        private byte[] ToPNG(UE.Sprite sprite)
        {
            var act = UE.RenderTexture.active;
            var tmp = UE.RenderTexture.GetTemporary(sprite.texture.width, sprite.texture.height, 0, UE.RenderTextureFormat.ARGB32);
            try
            {
                UE.RenderTexture.active = tmp;
                UE.Graphics.Blit(sprite.texture, tmp);
                var srcRect = sprite.textureRect;
                var buf = new UE.Texture2D((int)srcRect.width, (int)srcRect.height, UE.TextureFormat.ARGB32, false);
                buf.ReadPixels(srcRect, 0, 0, false);
                var png = UE.ImageConversion.EncodeToPNG(buf);
                UE.Object.Destroy(buf);
                return png;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                throw;
            }
            finally
            {
                UE.RenderTexture.active = act;
                UE.RenderTexture.ReleaseTemporary(tmp);
            }
        }
    }
}
