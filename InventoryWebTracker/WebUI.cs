using Routing = EmbedIO.Routing;
using WAPI = EmbedIO.WebApi;
using static EmbedIO.HttpContextExtensions;
using Tasks = System.Threading.Tasks;
using Text = System.Text;

namespace Haiku.InventoryWebTracker
{
    internal class WebUI : WAPI.WebApiController
    {
        [Routing.Route(EmbedIO.HttpVerbs.Get, "/")]
        public async Tasks.Task GetUI()
        {
            using var stream = HttpContext.OpenResponseStream();
            var b = Text.Encoding.UTF8.GetBytes(UI);
            await stream.WriteAsync(b, 0, b.Length);
        }

        private const string UI = @"<!doctype html>
        <html>
        <body>
            <p>Hello!</p>
        </body>
        </html>";
    }
}