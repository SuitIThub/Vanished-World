using System;
using System.Collections.Generic;
using static Entity.IModule;

namespace Entity.Inventory
{
    public class Core : IModule
    {
        [Serializable]
        public class Save : IModuleSave
        {
            public string moduleType { get; }

            public Save(Core input)
            {
                moduleType = input.ModuleType;
            }
        }

        public string id { get => parent.id; }

        public string ModuleType { get; private set; }

        public Entity.Core parent { get; private set; }
        public Dictionary<string, ModuleMethod> methods { get; private set; }

        private Dictionary<string, Item> inventoryID;
        private Dictionary<string, List<Item>> inventoryName;
        private Dictionary<int, List<Item>> inventoryUniqueID;

        public delegate void StatusChanged(Item item, params ReturnCode[] codes);

        public StatusChanged statusChanged;

        public IEnumerable<Item> items { get => inventoryID.Values; }

        public Core()
        {
            IntegrateMethods();

            ModuleType = "dummy";
            parent = null;

            inventoryID = new Dictionary<string, Item>();
            inventoryName = new Dictionary<string, List<Item>>();
            inventoryUniqueID = new Dictionary<int, List<Item>>();
        }

        public Core(Entity.Core parent, KVStorage json, KVStorage replaceData = null)
        {
            IntegrateMethods();

            this.parent = parent;

            string moduleType = "dummy";
            json.getElement("module", ref moduleType, replaceData);
            this.ModuleType = moduleType;

            inventoryID = new Dictionary<string, Item>();
            inventoryName = new Dictionary<string, List<Item>>();
            inventoryUniqueID = new Dictionary<int, List<Item>>();
        }

        public Core(string type, Entity.Core parent)
        {
            IntegrateMethods();

            ModuleType = type;
            this.parent = parent;

            inventoryID = new Dictionary<string, Item>();
            inventoryName = new Dictionary<string, List<Item>>();
            inventoryUniqueID = new Dictionary<int, List<Item>>();
        }

        public Core(Core core)
        {
            IntegrateMethods();

            ModuleType = core.ModuleType;
            parent = core.parent;
        }

        /// <summary>
        /// Adds an <see cref="Item"/> into the Inventory of <see cref="parent"/>
        /// </summary>
        /// <remarks>
        /// it tries, if possible and not limited by <paramref name="stackIfPossible"/>, to stack the item onto already added items
        /// </remarks>
        /// <param name="item">the <see cref="Item"/> to be added</param>
        /// <param name="showMessage">determines if a message should be prompted when the item gets added</param>
        /// <param name="stackIfPossible">determines if the item should be stack onto others if possible</param>
        /// <returns>
        ///     <list type="bullet|number|table">
        ///         <item>
        ///             <term><see cref="true"/> <see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
        ///             <description>Successfully loaded save-data into module</description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="false"/> <see cref="ReturnCode.codeEnum.ITEM_PARTIAL_MERGE"/> (402)</term>
        ///             <description><paramref name="item"/> has been partly merged and the rest has been added</description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="false"/> <see cref="ReturnCode.codeEnum.ITEM_NO_ADD"/> (403)</term>
        ///             <description><paramref name="item"/> has already been added to the inventory</description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="true"/> <see cref="ReturnCode.codeEnum.ITEM_FULL_MERGE"/> (405)</term>
        ///             <description><paramref name="item"/> has been fully merged into an existing item in inventory</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public virtual ReturnCode Add(Item item, bool showMessage = true, bool stackIfPossible = true, bool insertIfNoFullStack = true)
        {
            ReturnCode output = ReturnCode.SUCCESS;

            int amountCollect = 0;
            if (stackIfPossible && inventoryUniqueID.ContainsKey(item.UniqueID))
            {
                Item[] items = GetUniqueID(item.UniqueID);
                foreach(Item i in items)
                {
                    ReturnCode code = i.TryStack(item);

                    if (code.boolean && code != 405)
                    {
                        statusChanged?.Invoke(i, code);
                        if (code == 402)
                        {
                            output = code;
                            code.message.ToInt(out int stackAmount);
                            amountCollect += stackAmount;
                        }
                    }
                    else if (code == 405)
                    {
                        code.message.ToInt(out int stackAmount);

                        if (showMessage)
                        {
                            string itemName = stackAmount > 1 ? item.displayNamePlural : item.displayName;
                            CentreBrain.messageSystem.sendMessage(
                                parent.gameObject,
                                $"Collected {itemName} (x{stackAmount})",
                                item.icon,
                                true,
                                5
                            );
                        }

                        if ((parent.id == CentreBrain.data.player.id ||
                                (item.storedInventory != null &&
                                    item.storedInventory.id == CentreBrain.data.player.id)) &&
                            CentreBrain.inventoryManager.isActive)
                        {
                            CentreBrain.inventoryManager.pingUpdate();
                        }

                        return code;
                    }
                }

                int amount = 0;
                item.Info().getElement("amount", ref amount);
                if (amount == 0)
                {
                    if (showMessage)
                    {
                        string itemName = amountCollect > 1 ? item.displayNamePlural : item.displayName;
                        CentreBrain.messageSystem.sendMessage(
                            parent.gameObject,
                            $"Collected {itemName} (x{amountCollect})",
                            item.icon,
                            true,
                            5
                        );
                    }

                    if ((parent.id == CentreBrain.data.player.id ||
                            (item.storedInventory != null &&
                                item.storedInventory.id == CentreBrain.data.player.id)) &&
                        CentreBrain.inventoryManager.isActive)
                    {
                        CentreBrain.inventoryManager.pingUpdate();
                    }

                    return ReturnCode.Code(405);
                }

                if (output == 402)
                {
                    if (!insertIfNoFullStack)
                    {
                        if (showMessage)
                        {
                            string itemName = amountCollect > 1 ? item.displayNamePlural : item.displayName;
                            CentreBrain.messageSystem.sendMessage(
                                parent.gameObject,
                                $"Collected {itemName} (x{amountCollect})",
                                item.icon,
                                true,
                                5
                            );
                        }

                        if ((parent.id == CentreBrain.data.player.id ||
                                (item.storedInventory != null &&
                                    item.storedInventory.id == CentreBrain.data.player.id)) &&
                            CentreBrain.inventoryManager.isActive)
                        {
                            CentreBrain.inventoryManager.pingUpdate();
                        }

                        return output;
                    }
                }
            }

            if (inventoryID.ContainsKey(item.id))
                return ReturnCode.Code(403, "Item already added to inventory");

            int itemAmount = -1;
            item.Info().getElement("amount", ref itemAmount);

            statusChanged?.Invoke(item, ReturnCode.Code(405, "" + itemAmount));

            amountCollect += itemAmount;

            if (showMessage)
            {
                string itemName = amountCollect > 1 ? item.displayNamePlural : item.displayName;

                string amountText = itemAmount != -1 ? $"(x{amountCollect})" : "";
                CentreBrain.messageSystem.sendMessage(
                    parent.gameObject,
                    $"Collected {itemName} {amountText}",
                    item.icon,
                    true,
                    5
                );
            }

            item.storedInventory = this;

            inventoryID.Add(item.id, item);

            if (!inventoryName.ContainsKey(item.name))
                inventoryName.Add(item.name, new List<Item>());
            inventoryName[item.name].Add(item);

            if (!inventoryUniqueID.ContainsKey(item.UniqueID))
                inventoryUniqueID.Add(item.UniqueID, new List<Item>());
            inventoryUniqueID[item.UniqueID].Add(item);

            if ((parent.id == CentreBrain.data.player.id ||
                    (item.storedInventory != null &&
                        item.storedInventory.id == CentreBrain.data.player.id)) &&
                CentreBrain.inventoryManager.isActive)
            {
                CentreBrain.inventoryManager.pingUpdate();
            }

            return output;
        }

        /// <inheritdoc cref="Add(Item, bool, bool)"/>
        /// <param name="items">List of items to be added</param>
        public virtual void Add(IEnumerable<Item> items, bool showMessage = true, bool stackIfPossible = true, bool insertIfNoFullStack = true)
        {
            foreach (Item item in items)
                Add(item, showMessage, stackIfPossible, insertIfNoFullStack);
        }

        /// <summary>
        /// tries to find an <see cref="Item"/> in Inventory by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">the id of the item</param>
        /// <returns>the <see cref="Item"/> if found, if not returns 'null'</returns>
        public virtual Item GetID(string id)
        {
            if (inventoryID.ContainsKey(id))
                return inventoryID[id];
            return null;
        }

        /// <summary>
        /// tries to find all <see cref="Item"/>s possessing the <paramref name="name"/>
        /// </summary>
        /// <param name="name">the name of the item</param>
        /// <returns>a List of <see cref="Item"/> possessing <paramref name="name"/></returns>
        public virtual Item[] GetName(string name)
        {
            if (inventoryName.ContainsKey(name))
                return inventoryName[name].ToArray();
            return new Item[0];
        }

        /// <summary>
        /// tries to find all <see cref="Item"/>s possessing the <see cref="Item.UniqueID"/> of <paramref name="id"/>
        /// </summary>
        /// <param name="id">the <see cref="Item.UniqueID"/></param>
        /// <returns>a List of <see cref="Item"/> possessing <paramref name="id"/> as <see cref="Item.UniqueID"/></returns>
        public virtual Item[] GetUniqueID(int id)
        {
            if (inventoryUniqueID.ContainsKey(id))
                return inventoryUniqueID[id].ToArray();
            return new Item[0];
        }

        /// <summary>
        /// tries to find an <see cref="Item"/> in Inventory by its <paramref name="id"/> and the removes it from the inventory
        /// </summary>
        /// <param name="id">the id of the item</param>
        /// <returns>the <see cref="Item"/> if found, if not returns 'null'</returns>
        public virtual Item PullID(string id, bool showMessage = true, bool changeStoredInventory = true)
        {
            if (inventoryID.ContainsKey(id))
            {
                Item item = inventoryID[id];
                Remove(item, showMessage, changeStoredInventory);
                return item;
            }
            return null;
        }

        /// <summary>
        /// tries to find all <see cref="Item"/>s possessing the <paramref name="name"/> and removes them from the inventory
        /// </summary>
        /// <param name="name">the name of the item</param>
        /// <returns>a List of <see cref="Item"/> possessing <paramref name="name"/></returns>
        public virtual Item[] PullName(string name, bool showMessage = true, bool changeStoredInventory = true)
        {
            if (inventoryName.ContainsKey(name))
            {
                Item[] items = inventoryName[name].ToArray();

                foreach (Item item in items)
                    Remove(item, showMessage, changeStoredInventory);
                return items;
            }
            return new Item[0];
        }

        /// <summary>
        /// removes an <see cref="Item"/> from the Inventory
        /// </summary>
        /// <param name="item">the <see cref="Item"/> to be removed</param>
        public virtual void Remove(Item item, bool showMessage = true, bool changeStoredInventory = true)
        {
            statusChanged?.Invoke(item, ReturnCode.Code(406));

            if (changeStoredInventory)
                item.storedInventory = null;

            inventoryID.Remove(item.id);

            inventoryName[item.name].Remove(item);
            if (inventoryName[item.name].Count == 0)
                inventoryName.Remove(item.name);

            inventoryUniqueID[item.UniqueID].Remove(item);
            if (inventoryUniqueID[item.UniqueID].Count == 0)
                inventoryUniqueID.Remove(item.UniqueID);

            CentreBrain.messageSystem.sendMessage(
                    parent.gameObject,
                    item.displayName + " removed.",
                    item.icon,
                    true,
                    5
                );
        }

        /// <summary>
        /// tries to find an <see cref="Item"/> in Inventory by its <paramref name="id"/> and the removes it from the inventory
        /// </summary>
        /// <param name="id">the id of the item</param>
        public virtual void Remove(string id, bool showMessage = true, bool changeStoredInventory = true)
        {
            if (inventoryID.ContainsKey(id))
                Remove(inventoryID[id], showMessage, changeStoredInventory);
        }

        public virtual IModule Copy()
        {
            return new Core(this);
        }

        public virtual IModuleSave SaveModule()
        {
            return new Save(this);
        }

        public virtual ReturnCode LoadModule(IModuleSave save)
        {
            if (ModuleType != save.moduleType)
                return ReturnCode.Code(102, $"type:{save.moduleType} is not type:core");

            //todo: implement loading

            return ReturnCode.SUCCESS;
        }

        public virtual void Destroy()
        {
            return;
        }

        public virtual void IntegrateMethods()
        {
            methods = new Dictionary<string, ModuleMethod>();
        }

        /// <summary>
        /// used to add new delegate-methods to Module
        /// </summary>
        /// <param name="type">the key for finding the delegate-method</param>
        /// <param name="method">the method to be added</param>
        public virtual void addMethod(string type, ModuleMethod method)
        {
            if (methods.ContainsKey(type))
                methods[type] = method;
            else
                methods.Add(type, method);
        }
    }
}
