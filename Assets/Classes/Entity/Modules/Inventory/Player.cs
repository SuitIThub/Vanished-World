using System;
using UnityEngine;

namespace Entity.Inventory
{
    public class Player : Core, IModuleSubVariant
    {
        [Serializable]
        public new class Save : Core.Save
        {
            public Save(Player input) : base(input)
            {

            }
        }

        public Player() : base()
        {
            CentreBrain.inventoryManager = new InventoryManager(this);

            registerKeys();
        }

        public Player(Entity.Core parent, KVStorage json, KVStorage replaceData = null) : base(parent, json, replaceData)
        {
            CentreBrain.inventoryManager = new InventoryManager(this);

            registerKeys();
        }

        public Player(string type, Entity.Core parent) : base(type, parent)
        {
            CentreBrain.inventoryManager = new InventoryManager(this);

            registerKeys();
        }

        private void registerKeys()
        {
            CentreBrain.keyManager.Add(parent.id + "_TriggerInventory",
                new KeyManager.Key(KeyCode.E, KeyManager.KeyBase.KeyDir.down, null, TriggerInventoryOverlay)
            );
        }

        private void TriggerInventoryOverlay(KVStorage _)
        {
            CentreBrain.inventoryManager.Start(false);
        }

        public override IModule Copy()
        {
            return new Player(ModuleType, parent);
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