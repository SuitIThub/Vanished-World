using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Inventory
{
    public class Display : Core, IModuleSubVariant
    {
        [Serializable]
        public new class Save : Core.Save
        {
            public Save(Display input) : base(input)
            {

            }
        }

        public class Slot
        {
            public Vector2 pos;
            public List<Entity.Item> items;
            public int amount { get => items.Count; }

            public Slot(Vector2 pos, List<Item> items)
            {
                this.pos = pos;
                this.items = items;
            }
        }

        private Slot[] displaySlots;
        private Dictionary<Entity.Item, Slot> itemSlots;
        private int sortingOffset;

        public Display() : base()
        {

        }

        public Display(Entity.Core parent, KVStorage json, KVStorage replaceData = null) : base(parent, json, replaceData)
        {
            displaySlots = new Slot[0];
            itemSlots = new Dictionary<Item, Slot>();

            List<object> posList = null;
            if (json.getElement("positions", ref posList))
            {
                List<Slot> slots = new List<Slot>();
                foreach(object o in posList)
                {
                    Vector2 pos = Vector2.zero;
                    if (o.getElement(ref pos, replaceData))
                    {
                        slots.Add(new Slot(pos, new List<Item>()));
                    }
                }
                displaySlots = slots.ToArray();
            }

            sortingOffset = 0;
            json.getElement("sortingOffset", ref sortingOffset);
        }

        public Display(string type, Entity.Core parent) : base(type, parent)
        {

        }

        public override ReturnCode Add(Item item, bool showMessage = false, bool stackIfPossible = true, bool insertIfNoFullStack = true)
        {
            ReturnCode code = base.Add(item, showMessage, stackIfPossible, insertIfNoFullStack);

            if (code == 0)
            {
                Slot leastAmount = null;
                foreach(Slot slot in displaySlots)
                {
                    if (leastAmount == null || leastAmount.amount > slot.amount && !slot.items.Contains(item))
                        leastAmount = slot;
                }
                if (leastAmount != null)
                {
                    item.DropItem(leastAmount.pos + parent.pos, false, sortingOffset);
                    leastAmount.items.Add(item);
                    itemSlots.Add(item, leastAmount);
                }
            }

            return code;
        }

        public override void Remove(Item item, bool showMessage = false, bool changeStoredInventory = true)
        {
            base.Remove(item, showMessage, changeStoredInventory);

            if (itemSlots.ContainsKey(item))
            {
                Slot s = itemSlots[item];
                s.items.Remove(item);
                itemSlots.Remove(item);
            }
        }

        public override IModule Copy()
        {
            return new Display(ModuleType, parent);
        }

        public override IModuleSave SaveModule()
        {
            return new Save(this);
        }

        public override ReturnCode LoadModule(IModuleSave save)
        {
            if (ModuleType != save.moduleType)
                return ReturnCode.Code(102, $"type:{save.moduleType} is not type:core");

            //todo: implement loading

            return ReturnCode.SUCCESS;
        }

        public override void Destroy()
        {
            base.Destroy();

            return;
        }

        public override void IntegrateMethods()
        {
            base.IntegrateMethods();
        }
    }
}