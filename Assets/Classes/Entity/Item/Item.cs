using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Entity
{
    public class Item : Core
    {
        public int UniqueID
        {
            get
            {
                string text = this.Info().comparableString.replaceData(this.Info().data, "[]");
                return text.GetHashCode();
            }
        }

        public Inventory.Core storedInventory;

        public Item() : base()
        {

        }

        public Item(KVStorage json, Vector2 pos, KVStorage replaceData = null, string separator = "{}") : 
            base (json, pos, replaceData, separator)
        {
            
        }

        /// <summary>
        /// test <see cref="Item"/>
        /// </summary>
        /// <param name="pos">position des <see cref="Item"/></param>
        /// <param name="findFreeSpace">verschiebt die Position dahin wo Platz ist</param>
        /// <returns>
        ///     ob das Item gedroppt wurde
        ///     <list type="table">
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
        ///             <description>Successfully spawned <see cref="Item"/></description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.GAMEOBJECT_ALREADY_EXISTS"/> (201)</term>
        ///             <description>The <see cref="Item"/> has already been dropped and cannot be dropped again until picked up</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public ReturnCode DropItem(Vector2 pos, bool findFreeSpace = false, int sortingOffset = 0)
        {
            if (gameObject)
                return ReturnCode.Code(201, $"{id} already has a GameObject!", true);

            Vector2 scale = Vector2.one;

            this.Info().getElement("materialProperty.scale", ref scale);

            if (findFreeSpace)
            {
                List<Vector2> usedList = new List<Vector2>();
                pos.findFreeSpot(scale.x * 0.1f, ref usedList, ref pos);
            }

            Transform parent = CentreBrain.data.Folders.ITEM.transform;
            GameObject preset = CentreBrain.data.Prefabs["Item"];

            GameObject go = GameObject.Instantiate(preset, Vector3.zero, Quaternion.identity, parent);

            

            go.transform.localPosition = pos;
            go.transform.localScale = scale * 5;

            go.name = id;

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = icon.sprite;
            sr.sortingOrder = 32767 - (int)(pos.y * 10) - sortingOffset;

            gameObject = go;

            return ReturnCode.SUCCESS;
        }

        public ReturnCode TryStack(Item item)
        {
            int itemAmount = 1;
            int itemMaxAmount = 1;
            item.getElement("property.amount", ref itemAmount);
            item.getElement("property.maxAmount", ref itemMaxAmount);

            int amount = 1;
            int maxAmount = 1;
            this.getElement("property.amount", ref amount);
            this.getElement("property.maxAmount", ref maxAmount);

            if (amount == maxAmount)
                return ReturnCode.Code(401);

            if (UniqueID == item.UniqueID)
            {
                if (amount + itemAmount > maxAmount)
                {
                    int diff = amount - maxAmount;
                    itemAmount += diff;
                    amount = maxAmount;

                    this.Info().setElement("property.amount", amount);
                    item.Info().setElement("property.amount", itemAmount);

                    return ReturnCode.Code(402, "" + (-diff));
                }
                else
                {
                    amount += itemAmount;

                    itemAmount = 0;

                    this.Info().setElement("property.amount", amount);
                    item.Info().setElement("property.amount", itemAmount);

                    return ReturnCode.Code(405, "" + itemAmount);
                }
            }

            return ReturnCode.Code(401);
        }

        #region Debug

        public override void DebugWindow(DebugWindow window)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Stored Inventory: ");
            if (storedInventory == null)
                GUILayout.Label("-");
            else
            {
                GUILayout.Label($"{storedInventory.parent.displayName} ({storedInventory.id})");
                if (GUILayout.Button("", GUILayout.Width(15)))
                    window.selectEntity(storedInventory.parent);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            base.DebugWindow(window);
        }

        #endregion
    }
}