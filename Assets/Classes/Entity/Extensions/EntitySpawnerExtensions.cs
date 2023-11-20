using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

namespace Entity
{
    public static class EntitySpawnerExtensions
    {
        /// <summary>
        /// Creates an <see cref="Entity.Item"/> and drops it on the specified <paramref name="pos"/>
        /// </summary>
        /// <param name="path">The Resource-Path pointing to a json-file containing the data for spawning the <see cref="Item"/>s</param>
        /// <param name="pos">The Position where the Item should be dropped</param>
        /// <param name="entity">The finished <see cref="Item"/>s</param>
        /// <param name="inheritData">Data of type <see cref="KVStorage"/> that replaces and overwrites Data from the json from <paramref name="path"/></param>
        /// <seealso cref="spawnItem"/>
        /// <returns>
        ///     A Tupel of <see cref="ReturnCode"/> where Item1 represents the spawning and Item2 the dropping"/>
        ///     <list type="bullet|number|table">
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
        ///             <description>Successfully spawned or dropped <see cref="Item"/></description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.INVALID_VALUE"/> (102)</term>
        ///             <description>json couldn't be found at that <paramref name="path"/></description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.GAMEOBJECT_ALREADY_EXISTS"/> (201)</term>
        ///             <description>The <see cref="Item"/> has already been dropped and cannot be dropped again until picked up</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public static (ReturnCode, ReturnCode) spawnItemDrop(
            string path,
            Vector2 pos,
            out Item entity,
            KVStorage inheritData = null)
        {
            return (spawnItem(path, out entity, inheritData),
                entity.DropItem(pos));
        }

        /// <summary>
        /// Creates a List of <see cref="Entity.Item"/>
        /// </summary>
        /// <param name="path">The Resource-Path pointing to a json-file containing the data for spawning the <see cref="Item"/>s</param>
        /// <param name="amount">The amount of Items to be spawned from <paramref name="path"/></param>
        /// <param name="entityList">A List of finished <see cref="Item"/>s</param>
        /// <param name="inheritData">Data of type <see cref="KVStorage"/> that replaces and overwrites Data from the json from <paramref name="path"/></param>
        /// <param name="inventoryToAddTo">The <see cref="Inventory.Core"/> where the spawned Item should be added to</param>
        /// <param name="showInventoryMessage">Sets if a Message should be prompted when the Item gets added to the Inventory</param>
        /// <param name="stackInInventory">Sets if the created <see cref="Item"/> will be stacked if possible</param>
        /// <seealso cref="spawnItem"/>
        /// <returns>
        ///     A list of <see cref="ReturnCode"/> sorted by <see cref="Item"/> to be spawned
        ///     <list type="bullet|number|table">
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
        ///             <description>Successfully spawned <see cref="Item"/></description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.INVALID_VALUE"/> (102)</term>
        ///             <description>json couldn't be found at that <paramref name="path"/></description>
        ///         </item>
        ///     </list>
        /// </returns>
        public static ReturnCode[] spawnItems(
            string path, 
            int amount,
            out IEnumerable<Item> entityList,
            KVStorage inheritData = null,
            Inventory.Core inventoryToAddTo = null,
            bool showInventoryMessage = false,
            bool stackInInventory = true)
        {
            List<Item> newEntities = new List<Item>();
            List<ReturnCode> outputCode = new List<ReturnCode>();

            for (int i = 0; i < amount; i++)
            {
                outputCode.Add(spawnItem(path, out Item entity, inheritData, inventoryToAddTo, showInventoryMessage, stackInInventory));
                newEntities.Add(entity);
            }

            entityList = newEntities;

            return outputCode.ToArray();
        }

        /// <summary>
        /// Creates a List of <see cref="Entity.Item"/>
        /// </summary>
        /// <param name="itemDataList">A List of Tuples containing a Resource-Path to the json and a <see cref="KVStorage"/> containing the inheritDoc</param>
        /// <param name="entityList">A List of finished <see cref="Item"/>s</param>
        /// <param name="inventoryToAddTo">The <see cref="Inventory.Core"/> where the spawned Item should be added to</param>
        /// <param name="showInventoryMessage">Sets if a Message should be prompted when the Item gets added to the Inventory</param>
        /// <param name="stackInInventory">Sets if the created <see cref="Item"/> will be stacked if possible</param>
        /// <seealso cref="spawnItem"/>
        /// <returns>
        ///     A list of <see cref="ReturnCode"/> sorted by <see cref="Item"/> to be spawned
        ///     <list type="bullet|number|table">
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
        ///             <description>Successfully spawned <see cref="Item"/></description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.INVALID_VALUE"/> (102)</term>
        ///             <description>json couldn't be found at that <paramref name="path"/></description>
        ///         </item>
        ///     </list>
        /// </returns>
        public static ReturnCode[] spawnItems(
            IEnumerable<(string, KVStorage)> itemDataList,
            out IEnumerable<Item> entityList,
            Inventory.Core inventoryToAddTo = null,
            bool showInventoryMessage = false,
            bool stackInInventory = true)
        {
            List<Item> newEntities= new List<Item>();
            List<ReturnCode> outputCode = new List<ReturnCode>();

            foreach((string, KVStorage) itemData in itemDataList)
            { 
                outputCode.Add(spawnItem(itemData.Item1, out Item entity, itemData.Item2, inventoryToAddTo, showInventoryMessage, stackInInventory));
                newEntities.Add(entity);
            }

            entityList = newEntities;

            return outputCode.ToArray();
        }

        /// <summary>
        /// Creates an <see cref="Entity.Item"/>
        /// </summary>
        /// <param name="path">The Resource-Path pointing to a json-file containing the data for spawning the <see cref="Item"/></param>
        /// <param name="entity">The finished <see cref="Item"/></param>
        /// <param name="inheritData">Data of type <see cref="KVStorage"/> that replaces and overwrites Data from the json from <paramref name="path"/></param>
        /// <param name="inventoryToAddTo">The <see cref="Inventory.Core"/> where the spawned Item should be added to</param>
        /// <param name="showInventoryMessage">Sets if a Message should be prompted when the Item gets added to the Inventory</param>
        /// <param name="stackInInventory">Sets if the created <see cref="Item"/> will be stacked if possible</param>
        /// <returns>
        ///     <list type="bullet|number|table">
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
        ///             <description>Successfully spawned <see cref="Item"/></description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.INVALID_VALUE"/> (102)</term>
        ///             <description>json couldn't be found at that <paramref name="path"/></description>
        ///         </item>
        ///     </list>
        /// </returns>
        public static ReturnCode spawnItem(
            string path,
            out Item entity,
            KVStorage inheritData = null,
            Inventory.Core inventoryToAddTo = null,
            bool showInventoryMessage = false,
            bool stackInInventory = true)
        {
            ReturnCode code = spawnEntity(path, Vector2.zero, out entity, inheritData);

            if (inventoryToAddTo != null)
                inventoryToAddTo.Add(entity, showInventoryMessage, stackInInventory);

            return code;
        }

        /// <summary>
        /// Creates an Entity based on <see cref="Entity.Core"/>
        /// </summary>
        /// <param name="path">The Resource-Path pointing to a json-file containing the data for spawning the entity</param>
        /// <param name="pos">The position where the Entity should be spawned if it owns a <see cref="GameObject"/></param>
        /// <param name="entity">The finished Entity of type <typeparamref name="T"/></param>
        /// <param name="inheritData">Data of type <see cref="KVStorage"/> that replaces and overwrites Data from the json from <paramref name="path"/></param>
        /// <typeparam name="T">The type of Entity to be created. Limited to <see cref="Core"/> and <see cref="Item"/></typeparam>
        /// <returns>
        ///     <list type="bullet|number|table">
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
        ///             <description>Successfully spawned Entity</description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.INVALID_VALUE"/> (102)</term>
        ///             <description>json couldn't be found at that <paramref name="path"/></description>
        ///         </item>
        ///     </list>
        /// </returns>
        public static ReturnCode spawnEntity<T>(string path, Vector2 pos, out T entity, KVStorage inheritData = null) where T : Core
        {
            entity = null;

            //load json from Resource-File
            TextAsset ta = Resources.Load(path) as TextAsset;
            if (ta == null)
                return ReturnCode.Code(102, "Invalid Path to Entity-Json", true);

            KVStorage data = MiniJSON.Json.Deserialize(ta.text) as KVStorage;

            KVStorage KVData = null;
            if (!data.getElement("data", ref KVData))
                KVData = new KVStorage();

            List<object> interfaces = null;
            if (data.getElement("interface", ref interfaces))
            {
                List<object> protectedList = null;
                if (!KVData.getElement("interfaceProtected", ref protectedList))
                    protectedList = new List<object>();

                foreach (string s in interfaces.OfType<string>())
                {
                    if (inheritData == null)
                        inheritData = new KVStorage();

                    inheritData = inheritData.overwriteData(CentreBrain.data.jsonInterface[s], protectedList.OfType<string>().ToList());
                }
            }


            if (inheritData != null)
            {
                KVStorage overwrite = null;
                if (inheritData.getElement("overwriteData", ref overwrite))
                {
                    List<object> protectedList = null;
                    if (!KVData.getElement("protected", ref protectedList))
                        protectedList = new List<object>();

                    KVData = KVData.overwriteData(overwrite, protectedList.OfType<string>().ToList());

                    KVStorage publicOverwrite = null;
                    if (KVData.getElement("overwriteData", ref publicOverwrite))
                        publicOverwrite.Set(overwrite);
                }

                KVStorage replace = null;
                if (inheritData.getElement("replaceData", ref replace))
                    KVData = KVData.replaceData(replace);
            }

            KVStorage replaceData = null;
            KVData.getElement("replaceData", ref replaceData);

            object KVDataObject = null;
            KVData.calculateData(out KVDataObject, replaceData);
            KVData = KVDataObject as KVStorage;

            data.Set("data", KVData);

            data = data.replaceData(replaceData);

            if (typeof(T) == typeof(Core))
                entity = (T)(object)new Core(data, pos, replaceData);
            else if (typeof(T) == typeof(Item))
                entity = (T)(object)new Item(data, pos, replaceData);

            CentreBrain.data.entities.Add(entity.id, entity);

            return ReturnCode.SUCCESS;
        }
    }
}