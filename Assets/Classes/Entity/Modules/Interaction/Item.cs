using System;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;

namespace Entity.Interaction
{
    public class Item : Core, IModuleSubVariant
    {
        [Serializable]
        public new class Save : Core.Save
        {
            public Save(Item input) : base(input)
            {

            }
        }

        public Item() : base()
        {

        }

        public Item(Entity.Core parent, KVStorage json, KVStorage replaceData = null) : base(parent, json, replaceData)
        {
            if (!CentreBrain.data.interactions.ContainsKey("drop"))
            {
                KVStorage interactData = new KVStorage("center", CentreBrain.data.player.gameObject);
                CentreBrain.data.interactions.Add("drop",
                    InteractionExecuter.createInteractionFromPreset("drop", interactData, replaceData)
                );
            }
            this._interactions.Add("drop");
        }

        public Item(string type, Entity.Core parent) : base(type, parent)
        {

        }

        public ReturnCode RecieverPickUp(InteractionData<Entity.Core> data)
        {
            if (((Entity.Item)parent).storedInventory != null)
                ((Entity.Item)parent).storedInventory.Remove(((Entity.Item)parent));

            Entity.Core entity = data.sender;
            if (entity.Inventory(out Inventory.Core inventory))
            {
                inventory.Add((Entity.Item)parent, true);
                parent.DestroyGO();
                return ReturnCode.SUCCESS;
            }
            return ReturnCode.Code(301, $"Inventory in entity {entity.id} missing.", true);
        } 

        public ReturnCode RecieverDrop(InteractionData<Entity.Core> data)
        {
            Entity.Core sender = data.sender;
            Entity.Core origin = data.origin;
            Entity.Item item = null;
            if (!data.getElement("item", ref item))
                return ReturnCode.FAILED;

            Entity.Core focus = null;
            if (!data.getElement("focus", ref focus))
            {
                Entity.Item focusItem = null;
                data.getElement("focus", ref focusItem);
                focus = focusItem;
            }

            if (focus != null && item.id == parent.id)
                return ReturnCode.Code(301, $"Item {parent.id} can't be dropped onto itself.");

            if (item.storedInventory == null)
                return ReturnCode.Code(301, $"Item {parent.id} isn't stored in an Inventory so it can't be dropped.");


            List<Entity.Core> entities = null;
            data.getElement("entities", ref entities);

            if (entities.Count == 1 && entities[0].GetType() == typeof(Entity.Item) && entities[0].id != item.id)
            {
                Entity.Item itemGround = entities[0] as Entity.Item;
                ReturnCode code = itemGround.TryStack(item);

                if (code == 405)
                {
                    CentreBrain.messageSystem.sendMessage(itemGround.gameObject, $"x{code.message} added", 5);

                    item.storedInventory.Remove(item);
                    data.cancelInteraction();
                    return ReturnCode.SUCCESS;
                }
                else if (code == 402)
                {
                    CentreBrain.messageSystem.sendMessage(itemGround.gameObject, $"x{code.message} added", 5);
                    CentreBrain.messageSystem.sendMessage(
                        item.storedInventory.parent.gameObject,
                        $"x{code.message} removed",
                        item.icon,
                        true,
                        5
                    );

                    if (CentreBrain.inventoryManager.isActive)
                        CentreBrain.inventoryManager.pingUpdate();
                    return ReturnCode.SUCCESS;
                }
            }
            else if (entities.Count >= 1 && focus != null && focus.GetType() == typeof(Entity.Item))
            {
                Entity.Item itemGround = focus as Entity.Item;
                ReturnCode code = itemGround.TryStack(item);

                if (code == 405)
                {
                    CentreBrain.messageSystem.sendMessage(itemGround.gameObject, $"x{code.message} added", 5);

                    item.storedInventory.Remove(item);
                    data.cancelInteraction();
                    return ReturnCode.SUCCESS;
                }
                else if (code == 402)
                {
                    CentreBrain.messageSystem.sendMessage(itemGround.gameObject, $"x{code.message} added", 5);
                    CentreBrain.messageSystem.sendMessage(
                        item.storedInventory.parent.gameObject, 
                        $"x{code.message} removed", 
                        item.icon,
                        true,
                        5
                    );

                    if (CentreBrain.inventoryManager.isActive)
                        CentreBrain.inventoryManager.pingUpdate();
                    return ReturnCode.SUCCESS;
                }
            }

            Vector2 position = Vector2.zero;
            if (data.getElement("position", ref position))
            {
                ReturnCode code = item.DropItem(position);
                if (code.boolean)
                {
                    item.storedInventory.Remove(item);
                    data.cancelInteraction();
                }
                return code;
            }
            return ReturnCode.FAILED;
        }

        public override IModule Copy()
        {
            return new Item(ModuleType, parent);
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

            AddReciever("pick_up", RecieverPickUp);
            AddReciever("drop"   , RecieverDrop);
        }
    }
}
