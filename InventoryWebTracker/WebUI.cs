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
            <span id='slot34'>
                <img src='/icons/item0.png'>
                <span class='overlay-number' id='amount34'></span>
            </span>
            <img id='slot35' src='/icons/item1.png'>
            <img id='slot39' src='/icons/item5.png'>
            <img id='slot41' src='/icons/item7.png'>
            <img id='slot42' src='/icons/item8.png'>
            <img id='slot43' src='/icons/bulblet.png'>
            <img id='slot44' src='/icons/fireres.png'>
            <img id='slot45' src='/icons/waterres.png'>
            <span id='slot47'>
                <img src='/icons/coolant.png'>
                <span class='overlay-number' id='amount47'></span>
            </span>
            <span id='slot46'>
                <img src='/icons/powercell.png'>
                <span class='overlay-number' id='amount46'></span>
            </span>
            <span id='slot48'>
                <img src='/icons/redchipslot.png'>
                <span class='overlay-number' id='amount48'></span>
            </span>
            <span id='slot49'>
                <img src='/icons/greenchipslot.png'>
                <span class='overlay-number' id='amount49'></span>
            </span>
            <span id='slot50'>
                <img src='/icons/bluechipslot.png'>
                <span class='overlay-number' id='amount50'></span>
            </span>
            <style>
                body {
                    background-color: #FFF5D3;
                    display: grid;
                    grid-template-columns: repeat(auto-fill, 46px);
                    grid-auto-rows: 46px;
                }

                body > img {
                    border-radius: 8px;
                    padding: 2px;
                    width: 36px;
                    height: 36px;
                }

                body > span {
                    border-radius: 8px;
                    padding: 2px;
                    display: inline-flex;
                    position: relative;
                    width: 36px;
                    height: 36px;
                }

                .overlay-number {
                    visibility: hidden;
                    position: absolute;
                    width: 8px;
                    height: 8px;
                    bottom: 0;
                    right: 0;
                    display: inline-block;
                    border-radius: 2px;
                    background: black;
                    text-align: center;
                    font-size: 8px;
                    font-family: sans-serif;
                    color: white;
                }
            </style>
            <script>
                window.addEventListener('load', () => {
                    let loc = window.location.href.replace(/^http:\/\//, 'ws://') + '/inventory'
                    let conn = new WebSocket(loc)
                    conn.addEventListener('message', msg => {
                        let slots = JSON.parse(msg.data)
                        for (let i = 0; i < slots.length; i++) {
                            let icon = document.getElementById('slot' + i)
                            let num = document.getElementById('amount' + i)
                            if (icon != null) {
                                if (slots[i] > 0) {
                                    icon.style.opacity = 1
                                    icon.style.border = '2px solid black'
                                } else {
                                    icon.style.opacity = 0.3
                                    icon.style.border = '2px solid rgba(0, 0, 0, 0)'
                                }
                            }
                            if (num != null) {
                                if (slots[i] > 0) {
                                    num.style.visibility = 'visible'
                                    num.innerText = String(slots[i])
                                } else {
                                    num.style.visibility = 'hidden'
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
    }
}