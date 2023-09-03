global using System;
using SyncCollections = System.Collections.Concurrent;
using Collections = System.Collections.Generic;
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

        private Settings? modSettings;
        private EmbedIO.WebServer? uiServer;

        private Inventory? lastState;
        private float timeSinceLastState = 0;

        private const string url = "http://localhost:8086";
        private const float pollInterval = .1f;

        public void Start()
        {
            HaikuResources.Init();
            plugin = this;
            modSettings = new(Config);
        }

        public void LogError(String err) => Logger.LogError(err);

        public void Update()
        {
            if (modSettings!.Enabled.Value && uiServer == null)
            {
                uiServer = new EmbedIO.WebServer(opt => opt
                    .WithUrlPrefix(url)
                    .WithMode(EmbedIO.HttpListenerMode.EmbedIO)
                )
                    .WithModule(new UpdateSocket("/inventory"))
                    .WithWebApi("/", m => m.WithController<WebUI>());
                uiServer.RunAsync();
            }
            else if (!modSettings!.Enabled.Value && uiServer != null)
            {
                uiServer.Dispose();
                uiServer = null;
            }

            while (iconFetchQueue.TryDequeue(out var task))
            {
                task.RunSynchronously();
            }

            timeSinceLastState += UE.Time.deltaTime;
            if (timeSinceLastState < pollInterval)
            {
                return;
            }
            timeSinceLastState = 0;

            try
            {
                var inv = Inventory.Current();
                if (!inv.Equals(lastState))
                {
                    foreach (var q in inventoryQueues)
                    {
                        q(inv);
                    }
                    lastState = inv;
                }
            }
            catch (Exception)
            {
                // do nothing; we just couldn't read the inventory yet
            }
        }

        public void OnApplicationQuit()
        {
            // If we don't do this, the game will lock up when we try to quit it with a client connected.
            if (uiServer != null)
            {
                uiServer.Dispose();
                uiServer = null;
            }
        }

        private SyncCollections.ConcurrentQueue<Tasks.Task<byte[]>> iconFetchQueue = new();
        private Collections.List<Action<Inventory>> inventoryQueues = new();

        internal void Connect(Action<Inventory> client)
        {
            lock (inventoryQueues)
            {
                inventoryQueues.Add(client);
            }
        }

        internal void Disconnect(Action<Inventory> client)
        {
            lock (inventoryQueues)
            {
                inventoryQueues.Remove(client);
            }
        }

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
