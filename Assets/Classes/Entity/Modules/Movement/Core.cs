using System;
using System.Collections.Generic;
using UnityEngine;
using static Entity.IModule;

namespace Entity.Movement
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

        public Vector2 viewDir;

        public Core()
        {
            IntegrateMethods();

            ModuleType = "none";
            parent = null;

            viewDir = Vector2.zero;
        }

        public Core(Entity.Core parent, KVStorage json, KVStorage replaceData = null, string separator = "{}")
        {
            IntegrateMethods();

            this.parent = parent;

            string moduleType = "dummy";
            json.getElement("type", ref moduleType, replaceData, separator);
            this.ModuleType = moduleType;

            viewDir = Vector2.zero;
        }

        public Core(string type, Entity.Core parent)
        {
            IntegrateMethods();

            ModuleType = type;
            this.parent = parent;

            viewDir = Vector2.zero;
        }

        public Core(Core core)
        {
            IntegrateMethods();

            ModuleType = core.ModuleType;
            parent = core.parent;

            viewDir = Vector2.zero;
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
