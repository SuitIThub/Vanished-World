using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Entity.IModule;

namespace Entity.Info
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

        public string _description { get; private set; }
        public string description
        {
            get => _description.replaceData(data, "[]");
        }
        public Entity.Core parent { get; private set; }

        public string ModuleType { get; private set; }
        public Dictionary<string, ModuleMethod> methods { get; private set; }


        private KVStorage _data;
        public KVStorage data { get => _data; }

        private string[] _comparables;
        public string[] comparables
        {
            get => _comparables;
            set
            {
                comparableString = "";
                for (int i = 1; i < value.Length - 1; i++)
                    comparableString += value[i];
                _comparables = value;
            }
        }
        public string comparableString { get; private set; }

        public string dataKey { get; private protected set; }
        
        public Core(string dataKey = "InfoCore")
        {
            this.dataKey = dataKey;

            IntegrateMethods();

            ModuleType = "core";
            parent = null;
        }

        public Core(Entity.Core parent, KVStorage json, string dataKey = "InfoCore", KVStorage replaceData = null)
        {
            IntegrateMethods();

            json.calculateData(out object o, replaceData);
            json = (KVStorage)o;

            json.getElement("dataKey", ref dataKey, replaceData);

            this.dataKey = dataKey;

            string description = "";
            List<object> descriptionLines = new List<object>();
            if (json.getElement("description", ref descriptionLines, replaceData))
            {
                foreach (string s in descriptionLines.OfType<string>())
                    description += s + "\n";
            }
            else
                json.getElement("description", ref description, replaceData);

            this._description = description;

            string moduleType = "core";
            json.getElement("module", ref moduleType, replaceData);
            this.ModuleType = moduleType;

            this.parent = parent;

            if (!json.getElement("data", ref _data, replaceData))
                _data = new KVStorage();

            KVStorage replaceKV = null;
            if (!json.getElement("replaceData", ref replaceKV, replaceData))
                replaceKV = new KVStorage();

            List<object> comparables = new List<object>();
            if (json.getElement("comparables", ref comparables))
            {
                comparables.Insert(0, parent.name);
                comparables.Add(parent.id);
                this.comparables = comparables.OfType<string>().ToArray();
            }
            else
                this.comparables = new string[] {parent.name, parent.id};

            setElement("replaceData", replaceKV, false);

            KVStorage overwriteData = null;
            if (!json.getElement("overwriteData", ref overwriteData))
                overwriteData = new KVStorage();
            setElement("overwriteData", overwriteData);
        }

        public Core(string type, Entity.Core parent, string dataKey = "InfoCore")
        {
            this.dataKey = dataKey;

            IntegrateMethods();

            ModuleType = type;
            this.parent = parent;
        }

        public Core(Core core)
        {
            dataKey = core.dataKey;

            IntegrateMethods();

            ModuleType = core.ModuleType;
            parent = core.parent;
        }

        
        public virtual IModule Copy()
        {
            return new Core(this);
        }

        /// <summary>
        /// gets the archived replaceData used for inheriting Data and returns it
        /// </summary>
        /// <param name="publicOnly">determines if the replaceDate should be stripped of all 'localOnly' values</param>
        /// <returns>the replaceData</returns>
        public KVStorage getReplaceData(bool publicOnly = false)
        {
            KVStorage output = null;
            if (_data.getElement("replaceData", ref output))
            {
                if (publicOnly)
                {
                    List<object> list = null;
                    if (_data.getElement("localOnly", ref list))
                    {
                        foreach (object o in list)
                        {
                            string s = "";
                            if (o.getElement(ref s) && output.ContainsKey(s))
                                output.Remove(s);
                        }
                    }
                }

                return output;
            }
            else
                return new KVStorage();
        }

        /// <summary>
        /// gets the archived overwriteData used for inheriting Data and returns it
        /// </summary>
        /// <returns>the overwriteData</returns>
        public KVStorage getOverwriteData()
        {
            KVStorage output = null;
            if (_data.getElement("overwriteData", ref output))
                return output;
            else
                return new KVStorage();
        }

        /// <summary>
        /// sets a Value in the Info-Dataspace
        /// </summary>
        /// <param name="key">the key where the <paramref name="value"/> should be stored</param>
        /// <param name="value">the value to be stored</param>
        /// <param name="overwrite">determines if the value should be overwritten if a value with <paramref name="key"/> already exists</param>
        public virtual void setElement(string key, object value, bool overwrite = true)
        {
            if (overwrite)
                _data.Set(key, value);
            else
                _data.Add(key, value);
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
