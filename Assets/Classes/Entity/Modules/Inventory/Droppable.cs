using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Inventory
{
    public class Droppable : Core
    {
        [Serializable]
        public new class Save : Core.Save
        {
            public Save(Droppable input) : base(input)
            {

            }
        }

        public class Drop
        {
            public string preset;
            public int minAmount;
            public int maxAmount;

            public Drop(string preset)
            {
                this.preset = preset;
                minAmount = 1; 
                maxAmount = 1;
            }
            public Drop(string preset, int amount)
            {
                this.preset = preset;
                minAmount = amount;
                maxAmount = amount;
            }
            public Drop(string preset, int minAmount, int maxAmount)
            {
                this.preset = preset;
                this.minAmount= minAmount;
                this.maxAmount= maxAmount;
            }
            public Drop(int percent, string preset)
            {
                this.preset = preset;
                minAmount = -1;
                maxAmount = percent;
            }

            public bool Generate(Entity.Core parent, out Item item)
            {
                KVStorage replaceData = parent.Info().getReplaceData();
                KVStorage overwriteData = parent.Info().getOverwriteData();
                KVStorage inheritData = new KVStorage();
                inheritData.Set("replaceData", replaceData);
                inheritData.Set("overwriteData", overwriteData);
                
                EntitySpawnerExtensions.spawnItem($"Json/Item/{preset}", out item, inheritData);

                int num = minAmount;
                if (minAmount != -1)
                    num = UnityEngine.Random.Range(minAmount, maxAmount + 1);
                else
                {
                    float percent = UnityEngine.Random.Range(0f, 100f);
                    if (percent < maxAmount)
                        num = 1;
                }
                if (num == 0)
                    return false;

                item.Info().setElement("amount", num);

                return true;
            }
        }

        public Drop[] dropList;

        public Droppable() : base()
        {
            dropList = new Drop[0];
        }

        public Droppable(Entity.Core parent, KVStorage json, KVStorage replaceData = null) : base(parent, json, replaceData)
        {
            List<Drop> dropList = new();

            List<object> list = new();
            if (json.getElement("drop", ref list))
            {
                foreach(object o in list)
                {
                    int amount1 = 0;
                    int amount2 = 0;
                    string text1 = "";
                    string text2 = "";
                    if (o.GetType() == typeof(List<object>))
                    {
                        List<object> drop = o as List<object>;
                        if (drop.Count == 1 && drop[0].GetType() == typeof(string))
                        {
                            dropList.Add(new Drop(drop[0] as string));
                        }
                        else if (drop.Count == 2 &&
                            drop[0].getElement(ref text1) &&
                            drop[1].getElement(ref text2) &&
                            text1.EndsWith("%") &&
                            int.TryParse(text1.Replace("%", ""), out int percent))
                        {
                            dropList.Add(new Drop(percent, text2));
                        }
                        else if (drop.Count == 2 &&
                            drop[0].getElement(ref amount1) &&
                            drop[1].getElement(ref text1))
                        {
                            dropList.Add(new Drop(text1, amount1));
                        }
                        else if (drop.Count == 3 &&
                            drop[0].getElement(ref amount1) &&
                            drop[1].getElement(ref amount2) &&
                            drop[2].getElement(ref text1))
                        {
                            dropList.Add(new Drop(text1, amount1, amount2));
                        }
                    }
                    else if (o.getElement(ref text1))
                        dropList.Add(new Drop(text1));
                }
            }

            this.dropList = dropList.ToArray();
        }

        public Droppable(string type, Entity.Core parent) : base(type, parent)
        {
            dropList = new Drop[0];
        }

        public override IModule Copy()
        {
            return new Droppable(ModuleType, parent);
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

            foreach(Drop drop in dropList)
            {
                if (drop.Generate(parent, out Item item))
                    item.DropItem(parent.realPos);
            }

            return;
        }

        public override void IntegrateMethods()
        {
            base.IntegrateMethods();
        }
    }
}
