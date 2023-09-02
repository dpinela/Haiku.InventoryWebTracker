using Routing = EmbedIO.Routing;
using WAPI = EmbedIO.WebApi;
using static EmbedIO.HttpContextExtensions;
using UE = UnityEngine;
using Collections = System.Collections.Generic;
using Tasks = System.Threading.Tasks;
using Text = System.Text;

namespace Haiku.InventoryWebTracker
{
    internal class WebUI : WAPI.WebApiController
    {
        [Routing.Route(EmbedIO.HttpVerbs.Get, "/")]
        public async Tasks.Task GetUI()
        {
            await HttpContext.SendStringAsync(UI, "text/html; charset=utf-8", Text.Encoding.UTF8);
        }

        [Routing.Route(EmbedIO.HttpVerbs.Get, "/icons/{iconName}")]
        public async Tasks.Task GetIcon(string iconName)
        {
            if (!gameIcons.TryGetValue(iconName, out var icon))
            {
                throw EmbedIO.HttpException.NotFound();
            }
            HttpContext.Response.ContentType = "image/png";
            var png = await InventoryWebTrackerPlugin.Instance.ToPNGAsync(icon);
            using var stream = HttpContext.OpenResponseStream();
            await stream.WriteAsync(png, 0, png.Length);
        }

        private const string UI = @"<!doctype html>
        <html>
        <body>
            <p>Hello!</p>
            <img src='/icons/chip.png'>
        </body>
        </html>";

        private static readonly Collections.Dictionary<string, Func<UE.Sprite>> gameIcons = new()
        {
            {"chip.png", () => GameManager.instance.chip[3].image}
        };
    }
}