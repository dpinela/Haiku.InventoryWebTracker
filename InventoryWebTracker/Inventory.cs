using Collections = System.Collections.Generic;
using static System.Linq.Enumerable;

namespace Haiku.InventoryWebTracker
{
    internal class Inventory
    {
        private readonly Collections.List<ushort> slots = new();

        public static Inventory Current()
        {
            var inv = new Inventory();
            foreach (var c in GameManager.instance.chip)
            {
                inv.Add(c.collected);
            }
            inv.Add(GameManager.instance.canWallJump);
            inv.Add(GameManager.instance.canRoll);
            inv.Add(GameManager.instance.canBomb);
            inv.Add(GameManager.instance.canTeleport);
            inv.Add(GameManager.instance.canDoubleJump);
            inv.Add(GameManager.instance.canGrapple);
            for (var i = 0; i < 9; i++)
            {
                inv.Add(InventoryManager.instance.ItemQuantity(i));
            }
            inv.Add(GameManager.instance.lightBulb);
            inv.Add(GameManager.instance.fireRes);
            inv.Add(GameManager.instance.waterRes);
            int numPC = 0;
            foreach (var pc in GameManager.instance.powerCells)
            {
                if (pc.collected) numPC++;
            }
            inv.Add(numPC);
            inv.Add(GameManager.instance.coolingPoints);
            int redSlots = 0;
            int greenSlots = 0;
            int blueSlots = 0;
            foreach (var slot in GameManager.instance.chipSlot)
            {
                if (!slot.collected) continue;

                switch (slot.chipSlotColor)
                {
                    case "red":
                        redSlots++;
                        break;
                    case "green":
                        greenSlots++;
                        break;
                    case "blue":
                        blueSlots++;
                        break;
                }
            }
            inv.Add(redSlots);
            inv.Add(greenSlots);
            inv.Add(blueSlots);
            return inv;
        }

        public override string ToString() =>
            "[" + string.Join(",", slots) + "]";

        public bool Equals(Inventory? other) =>
            other != null && slots.SequenceEqual(other.slots);

        private void Add(bool b)
        {
            Add(b ? 1 : 0);
        }

        private void Add(int x)
        {
            slots.Add((ushort)x);
        }
    }
}