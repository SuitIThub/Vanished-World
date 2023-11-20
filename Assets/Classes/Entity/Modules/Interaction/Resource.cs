using System;

namespace Entity.Interaction
{
    public class Resource : Core, IModuleSubVariant
    {
        [Serializable]
        public new class Save : Core.Save
        {
            public Save(Resource input) : base(input)
            {

            }
        }

        public Resource() : base()
        {

        }

        public Resource(Entity.Core parent, KVStorage json, KVStorage replaceData = null) : base(parent, json, replaceData)
        {

        }

        public Resource(string type, Entity.Core parent) : base(type, parent)
        {

        }

        public override IModule Copy()
        {
            return new Resource(ModuleType, parent);
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

        public ReturnCode recieverHit(InteractionData<Entity.Core> data)
        {
            float damage = 0;
            if (data.data.getElement("damage", ref damage))
            {
                float hp = 0;
                parent.Info().getElement("property.HP", ref hp);
                hp -= damage;
                parent.Info().setElement("property.HP", hp, true);
                if (hp <= 0)
                {
                    parent.Destroy();
                    return ReturnCode.SUCCESS;
                }
                CentreBrain.messageSystem.sendMessage(parent.gameObject, $"-{damage} HP ({hp})", 2);
                return ReturnCode.SUCCESS;
            }

            return ReturnCode.Code(101, "'damage' property missing in InteractionData", true);
        }

        public override void IntegrateMethods()
        {
            base.IntegrateMethods();

            AddReciever("hit", recieverHit);
        }
    }
}
