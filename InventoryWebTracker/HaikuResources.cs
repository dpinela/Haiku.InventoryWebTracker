// Pulled directly from Haiku.Rando with some unnecessary parts cut down
// and moved to this mod's namespace.
/*
MIT License

Copyright (c) 2022 Jarlyk

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/
#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Haiku.InventoryWebTracker
{
    public static class HaikuResources
    {
        public static PickupItem RefPickupCoolant { get; private set; }

        public static PickupItem RefPickupRedChipSlot { get; private set; }

        public static PickupItem RefPickupBlueChipSlot { get; private set; }

        public static PickupItem RefPickupGreenChipSlot { get; private set; }

        public static PowerCell RefPowerCell { get; private set; }

        public static UnlockTutorial RefUnlockTutorial { get; private set; }

        public static void Init()
        {
            RefPickupCoolant = LoadRef<PickupItem>("PickupCoolantTrigger 1");
            RefPickupRedChipSlot = LoadRef<PickupItem>("PickupRedChipSlotTrigger 1");
            RefPickupBlueChipSlot = LoadRef<PickupItem>("PickupBlueChipSlotTrigger 1");
            RefPickupGreenChipSlot = LoadRef<PickupItem>("PickupGreenChipSlotTrigger 1");
            RefPowerCell = LoadRef<PowerCell>("PowerCell 1");
            RefUnlockTutorial = LoadRef<UnlockTutorial>("PickupPREFAB 1");
        }

        public static PickupItem GetRefChipSlot(int chipSlotId)
        {
            var slot = GameManager.instance.chipSlot[chipSlotId];
            if (slot.chipSlotColor == "red") return RefPickupRedChipSlot;
            if (slot.chipSlotColor == "green") return RefPickupGreenChipSlot;
            if (slot.chipSlotColor == "blue") return RefPickupBlueChipSlot;
            throw new ArgumentException($"Invalid color type '{slot.chipSlotColor}' for id {chipSlotId}");
        }

        private static T LoadRef<T>(string resourcePath)
        {
            var prefab = Resources.Load<GameObject>(resourcePath);
            var refInstance = Object.Instantiate(prefab);
            refInstance.SetActive(false);
            Object.DontDestroyOnLoad(refInstance);
            return refInstance.GetComponent<T>();
        }

        private static GameObject _itemDescObject;

        public static ItemDescriptionManager ItemDesc()
        {
            if (!_itemDescObject)
            {
                _itemDescObject = GetDontDestroyOnLoadObjects()
                                  .Select(x => x.GetComponentInChildren<ItemDescriptionManager>(true))
                                  .First(d => d).gameObject;
            }

            return _itemDescObject.GetComponent<ItemDescriptionManager>();
        }

        public static GameObject[] GetDontDestroyOnLoadObjects()
        {
            GameObject temp = null;
            try
            {
                temp = new GameObject();
                Object.DontDestroyOnLoad( temp );
                UnityEngine.SceneManagement.Scene dontDestroyOnLoad = temp.scene;
                Object.DestroyImmediate( temp );
                temp = null;
     
                return dontDestroyOnLoad.GetRootGameObjects();
            }
            finally
            {
                if( temp != null )
                    Object.DestroyImmediate( temp );
            }
        }
    }
}
