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
            <img src='/icons/chip1.png'>
            <img src='/icons/chip3.png'>
            <img src='/icons/chip6.png'>
            <img src='/icons/chip11.png'>
            <img src='/icons/chip16.png'>
            <img src='/icons/chip20.png'>
            <img src='/icons/chip25.png'>
            <img src='/icons/ability0.png'>
            <img src='/icons/ability1.png'>
            <img src='/icons/ability2.png'>
            <img src='/icons/ability3.png'>
            <img src='/icons/ability4.png'>
            <img src='/icons/ability5.png'>
            <img src='/icons/item1.png'>
            <img src='/icons/item5.png'>
            <img src='/icons/item7.png'>
            <img src='/icons/item8.png'>
            <img src='/icons/bulblet.png'>
            <img src='/icons/fireres.png'>
            <img src='/icons/waterres.png'>
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
                    InventoryWebTrackerPlugin.Instance.LogError(key);
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