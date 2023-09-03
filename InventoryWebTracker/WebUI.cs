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

        private const string UI = @"
        <!doctype html>
        <html>
        <body>
            <img id='slot1' src='/icons/chip1.png'>
            <img id='slot3' src='/icons/chip3.png'>
            <img id='slot6' src='/icons/chip6.png'>
            <img id='slot11' src='/icons/chip11.png'>
            <img id='slot16' src='/icons/chip16.png'>
            <img id='slot20' src='/icons/chip20.png'>
            <img id='slot25' src='/icons/chip25.png'>
            <img id='slot28' src='/icons/ability0.png'>
            <img id='slot29' src='/icons/ability1.png'>
            <img id='slot30' src='/icons/ability2.png'>
            <img id='slot31' src='/icons/ability3.png'>
            <img id='slot32' src='/icons/ability4.png'>
            <img id='slot33' src='/icons/ability5.png'>
            <img id='slot34' src='/icons/item0.png'>
            <img id='slot35' src='/icons/item1.png'>
            <img id='slot39' src='/icons/item5.png'>
            <img id='slot41' src='/icons/item7.png'>
            <img id='slot42' src='/icons/item8.png'>
            <img id='slot43' src='/icons/bulblet.png'>
            <img id='slot44' src='/icons/fireres.png'>
            <img id='slot45' src='/icons/waterres.png'>
            <img id='slot47' src='/icons/coolant.png'>
            <img id='slot46' src='/icons/powercell.png'>
            <img id='slot48' src='/icons/redchipslot.png'>
            <img id='slot49' src='/icons/greenchipslot.png'>
            <img id='slot50' src='/icons/bluechipslot.png'>
            <script>
                window.addEventListener('load', () => {
                    let loc = window.location.href.replace(/^http:\/\//, 'ws://') + '/inventory'
                    let conn = new WebSocket(loc)
                    conn.addEventListener('message', msg => {
                        let slots = JSON.parse(msg.data)
                        for (let i = 0; i < slots.length; i++) {
                            let icon = document.getElementById('slot' + i)
                            if (icon != null) {
                                if (slots[i] > 0) {
                                    icon.style.opacity = 1;
                                } else {
                                    icon.style.opacity = 0.3;
                                }
                            }
                        }
                    })
                })
            </script>
        </body>
        </html>";

        private static readonly Collections.Dictionary<string, Func<UE.Sprite>> gameIcons = BuildIconTable();

        private static Collections.Dictionary<string, Func<UE.Sprite>> BuildIconTable()
        {
            try
            {
            var tbl = new Collections.Dictionary<string, Func<UE.Sprite>>();
            for (var i = 0; i < 9; i++)
            {
                var j = i;
                tbl[$"item{j}.png"] = () => InventoryManager.instance.items[j].image;
            }
            
            for (var i = 0; i < HaikuResources.RefUnlockTutorial.abilities.Length; i++)
            {
                var j = i;
                tbl[$"ability{j}.png"] = () => HaikuResources.RefUnlockTutorial.abilities[j].image;
            }
            for (var i = 0; i < GameManager.instance.chip.Length; i++)
            {
                var j = i;
                tbl[$"chip{j}.png"] = () => GameManager.instance.chip[j].image;
            }
            var slotColors = 0;
            foreach (var slot in GameManager.instance.chipSlot)
            {
                var key = $"{slot.chipSlotColor}chipslot.png";
                if (!tbl.ContainsKey(key))
                {
                    var item = slot.chipSlotColor switch
                    {
                        "red" => HaikuResources.RefPickupRedChipSlot,
                        "green" => HaikuResources.RefPickupGreenChipSlot,
                        "blue" => HaikuResources.RefPickupBlueChipSlot,
                        _ => throw new InvalidOperationException($"invalid chip slot color: {slot.chipSlotColor}")
                    };
                    tbl[key] = () => item.chipSlotImage;
                    slotColors++;
                    if (slotColors == 3)
                    {
                        break;
                    }
                }
            }
            tbl["bulblet.png"] = () => HaikuResources.ItemDesc().lightBulb.image.sprite;
            tbl["powercell.png"] = () => HaikuResources.RefPowerCell.GetComponentInChildren<UE.SpriteRenderer>(true).sprite;
            tbl["coolant.png"] = () => HaikuResources.RefPickupCoolant.coolantImage;
            tbl["fireres.png"] = () => HaikuResources.ItemDesc().fireRes.image.sprite;
            tbl["waterres.png"] = () => HaikuResources.ItemDesc().waterRes.image.sprite;

            return tbl;
            }
            catch (Exception err)
            {
                InventoryWebTrackerPlugin.Instance.LogError(err.ToString());
                return new();
            }
        }
    }
}