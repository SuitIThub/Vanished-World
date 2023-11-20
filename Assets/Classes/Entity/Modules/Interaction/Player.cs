using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Interaction
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
            registerKeys();
        }

        public Player(Entity.Core parent, KVStorage json, KVStorage replaceData = null) : base(parent, json, replaceData)
        {
            registerKeys();
        }

        public Player(string type, Entity.Core parent) : base(type, parent)
        {
            registerKeys();
        }

        private void registerKeys()
        {
            CentreBrain.keyManager.Add("testMethod",
                new KeyManager.Key(KeyCode.R, KeyManager.KeyBase.KeyDir.down, new KVStorage("type", "pick_up"), startInteraction)
            );
        }

        public void startInteraction(KVStorage data)
        {
            if (InteractionExecuter.isRunning)
                return;

            string type = "";
            if (data.getElement("type", ref type) && CentreBrain.data.interactions.ContainsKey(type))
            {
                CentreBrain.data.interactions[type].cancelMethod = endInteraction;
                CentreBrain.data.interactions[type].Trigger(data, parent);
                InteractionExecuter.isRunning = true;
            }
        }

        public void endInteraction()
        {
            InteractionExecuter.isRunning = false;
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

        public ReturnCode senderHit(InteractionData<Entity.Core> intData)
        {
            List<Entity.Core> entities = null;
            if (intData.data.getElement("entities", ref entities))
            {
                intData.data.Add("damage", 5);

                foreach(Entity.Core entity in entities)
                {
                    if (entity.Interact(out Core interact))
                        interact.RecieveInteraction(intData);
                }

                return ReturnCode.SUCCESS;
            }

            return ReturnCode.Code(102, "InteractionData is missing Entities to aim for", true);
        }

        public ReturnCode senderDefault(InteractionData<Entity.Core> intData)
        {
            List<Entity.Core> entities = null;
            if (intData.data.getElement("entities", ref entities))
            {
                foreach(Entity.Core entity in entities)
                {
                    if (entity.Interact(out Core interact))
                        interact.RecieveInteraction(intData);
                }

                return ReturnCode.SUCCESS;
            }

            return ReturnCode.Code(101, "InteractionData is missing Entities to redirect to", true);
        }

        public override void IntegrateMethods()
        {
            base.IntegrateMethods();

            AddSender("default", senderDefault);
            AddSender("hit", senderHit);
        }
    }
}