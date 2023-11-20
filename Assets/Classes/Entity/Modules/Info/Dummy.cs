using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Info
{
    public class Dummy : Core, IModuleSubVariant
    {
        [Serializable]
        public new class Save : Core.Save
        {
            public Save(Dummy input) : base(input)
            {

            }
        }

        public Dummy(string dataKey = "InfoDummy") : base(dataKey)
        {

        }

        public Dummy(Entity.Core parent, KVStorage json, string dataKey = "InfoDummy", KVStorage replaceData = null) : base(parent, json, dataKey, replaceData)
        {

        }

        public Dummy(string type, Entity.Core parent, string dataKey = "InfoDummy") : base(type, parent, dataKey)
        {

        }

        public override IModule Copy()
        {
            return new Dummy(ModuleType, parent);
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
