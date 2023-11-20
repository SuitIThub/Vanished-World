using System;
using UnityEngine;

namespace Entity.Movement
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

        private bool[] directions = { false, false, false, false };
        private Rigidbody2D rigidbody;
        const float MOVING_SPEED = 5f;

        public Player() : base()
        {
            rigidbody = parent.gameObject.GetComponent<Rigidbody2D>();

            registerKeys();
        }

        public Player(Entity.Core parent, KVStorage json, KVStorage replaceData = null) : base(parent, json, replaceData)
        {
            rigidbody = parent.gameObject.GetComponent<Rigidbody2D>();

            registerKeys();
        }

        public Player(string type, Entity.Core parent) : base(type, parent)
        {
            rigidbody = parent.gameObject.GetComponent<Rigidbody2D>();

            registerKeys();
        }

        public void registerKeys()
        {
            CentreBrain.keyManager.Add("startMoveUp",
                new KeyManager.Key(KeyCode.W, KeyManager.KeyBase.KeyDir.hold, new KVStorage("direction", 0), activateDirection)
            );
            CentreBrain.keyManager.Add("startMoveRight",
                new KeyManager.Key(KeyCode.D, KeyManager.KeyBase.KeyDir.hold, new KVStorage("direction", 1), activateDirection)
            );
            CentreBrain.keyManager.Add("startMoveDown",
                new KeyManager.Key(KeyCode.S, KeyManager.KeyBase.KeyDir.hold, new KVStorage("direction", 2), activateDirection)
            );
            CentreBrain.keyManager.Add("startMoveLeft",
                new KeyManager.Key(KeyCode.A, KeyManager.KeyBase.KeyDir.hold, new KVStorage("direction", 3), activateDirection)
            );

            CentreBrain.keyManager.Add("endMoveUp",
                new KeyManager.Key(KeyCode.W, KeyManager.KeyBase.KeyDir.up, new KVStorage("direction", 0), deactivateDirection)
            );
            CentreBrain.keyManager.Add("endMoveRight",
                new KeyManager.Key(KeyCode.D, KeyManager.KeyBase.KeyDir.up, new KVStorage("direction", 1), deactivateDirection)
            );
            CentreBrain.keyManager.Add("endMoveDown",
                new KeyManager.Key(KeyCode.S, KeyManager.KeyBase.KeyDir.up, new KVStorage("direction", 2), deactivateDirection)
            );
            CentreBrain.keyManager.Add("endMoveLeft",
                new KeyManager.Key(KeyCode.A, KeyManager.KeyBase.KeyDir.up, new KVStorage("direction", 3), deactivateDirection)
            );
        }

        public void activateDirection(KVStorage data)
        {
            int index = 0;
            data.getElement("direction", ref index);
            directions[index] = true;

            updateMovement();
        }

        public void deactivateDirection(KVStorage data)
        {
            int index = 0;
            data.getElement("direction", ref index);
            directions[index] = false;

            updateMovement();
        }

        public void updateMovement()
        {
            rigidbody.velocity = Vector2.zero;

            Vector2 dir = Vector2.zero;

            if (directions[0])
                dir += Vector2.up;
            if (directions[1])
                dir += Vector2.right;
            if (directions[2])
                dir += Vector2.down;
            if (directions[3])
                dir += Vector2.left;


            if (dir != Vector2.zero)
            {
                viewDir = dir;
                rigidbody.velocity = dir.toDegree().degreeToVector2(MOVING_SPEED);
            }
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
