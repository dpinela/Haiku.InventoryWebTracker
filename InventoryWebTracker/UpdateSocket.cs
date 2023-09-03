using WS = EmbedIO.WebSockets;
using SyncCollections = System.Collections.Concurrent;
using Tasks = System.Threading.Tasks;

namespace Haiku.InventoryWebTracker
{
    internal class UpdateSocket : WS.WebSocketModule
    {
        public UpdateSocket(string path) : base(path, true) {}

        protected override async Tasks.Task OnClientConnectedAsync(WS.IWebSocketContext ctx)
        {
            using var stateQueue = new SyncCollections.BlockingCollection<Inventory>(
                new SyncCollections.ConcurrentQueue<Inventory>()
            );
            Action<Inventory> add = stateQueue.Add;
            InventoryWebTrackerPlugin.Instance.Connect(add);
            try
            {
                while (true)
                {
                    var state = await Tasks.Task.Run(stateQueue.Take);
                    await SendAsync(ctx, state.ToString());
                }
            }
            finally
            {
                InventoryWebTrackerPlugin.Instance.Disconnect(add);
            }
        }

        protected override Tasks.Task OnMessageReceivedAsync(WS.IWebSocketContext ctx, byte[] msg, WS.IWebSocketReceiveResult res) => Tasks.Task.CompletedTask;
    }
}