using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Entity.IModule;

namespace Entity.Interaction
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

        private protected List<string> _interactions;
        public InteractionExecuter[] interactions
        {
            get => _interactions
                .Where(k => CentreBrain.data.interactions.ContainsKey(k))
                .Select(k => CentreBrain.data.interactions[k])
                .ToArray();
        }

        public delegate ReturnCode interactMethod(InteractionData<Entity.Core> data);

        public Dictionary<string, ModuleMethod> methods { get; private set; }
        private protected Dictionary<string, interactMethod> sender;
        private protected Dictionary<string, interactMethod> reciever;

        public Core()
        {
            IntegrateMethods();

            ModuleType = "dummy";
            parent = null;
        }

        public Core(Entity.Core parent, KVStorage json, KVStorage replaceData = null)
        {
            IntegrateMethods();

            this.parent = parent;

            string moduleType = "dummy";
            json.getElement("module", ref moduleType, replaceData);
            this.ModuleType = moduleType;

            List<object> interactions = null;
            if (json.getElement("interactions", ref interactions, replaceData))
            {
                this._interactions = interactions.OfType<string>().ToList();
                KVStorage interactData = new KVStorage("center", parent.gameObject);
                foreach (string preset in this._interactions)
                {
                    if (!CentreBrain.data.interactions.ContainsKey(preset))
                    {
                        CentreBrain.data.interactions.Add(preset,
                            InteractionExecuter.createInteractionFromPreset(preset, interactData, replaceData)
                        );
                    }
                }
            }
            else
                this._interactions = new List<string>();
        }

        public Core(string type, Entity.Core parent)
        {
            IntegrateMethods();

            ModuleType = type;
            this.parent = parent;
        }

        public Core(Core core)
        {
            IntegrateMethods();

            ModuleType = core.ModuleType;
            parent = core.parent;
        }

        public ReturnCode recieveDropDefault(InteractionData<Entity.Core> data)
        {
            Entity.Item item = null;
            if (!data.getElement("item", ref item))
                return ReturnCode.Code(101, "Item is missing in InteractionData", true);

            if (item.storedInventory == null)
                return ReturnCode.Code(301, $"Item {parent.id} isn't stored in an Inventory so it can't be dropped.");

            if (parent.Inventory(out Inventory.Core inventory))
            {
                Inventory.Core oldInventory = item.storedInventory;
                ReturnCode code = inventory.Add(item, true, true, false);

                if (code.boolean)
                {
                    oldInventory.Remove(item, true, false);
                    data.cancelInteraction();
                }
                if (code == 402)
                {
                    CentreBrain.messageSystem.sendMessage(
                        parent.gameObject,
                        $"{item.displayName} (x{code.message}) removed.",
                        item.icon,
                        true,
                        5
                    );
                }
            }

            return ReturnCode.Code(301, $"{parent.id} has no inventory");
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
            sender = new Dictionary<string, interactMethod>();
            reciever = new Dictionary<string, interactMethod>();

            reciever.Add("drop", recieveDropDefault);
        }

        /// <summary>
        /// used to add new delegate-methods to Module
        /// </summary>
        /// <param name="type">the key for finding the delegate-method</param>
        /// <param name="method">the method to be added</param>
        public virtual void AddMethod(string type, ModuleMethod method)
        {
            if (methods.ContainsKey(type))
                methods[type] = method;
            else
                methods.Add(type, method);
        }

        /// <summary>
        /// used to add new delegate-sender-interaction-methods to Module
        /// </summary>
        /// <param name="type">the key for finding the delegate-method</param>
        /// <param name="method">the method to be added</param>
        public virtual void AddSender(string type, interactMethod method)
        {
            if (sender.ContainsKey(type))
                sender[type] = method;
            else
                sender.Add(type, method);
        }

        /// <summary>
        /// used to add new delegate-reciever-interaction-methods to Module
        /// </summary>
        /// <param name="type">the key for finding the delegate-method</param>
        /// <param name="method">the method to be added</param>
        public virtual void AddReciever(string type, interactMethod method)
        {
            if (reciever.ContainsKey(type))
                reciever[type] = method;
            else
                reciever.Add(type, method);
        }

        /// <summary>
        /// Used to send interactions to another entity
        /// </summary>
        /// <param name="data">the <see cref="InteractionData{T}"/> containing the information needed to work</param>
        /// <param name="keys">a list of keys the method will be able to use</param>
        /// <param name="forceSender">override bool to send all keys ignoring the <paramref name="keys"/>-parameter</param>
        /// <returns>a new List of keys that have not been used</returns>
        public IEnumerable<string> SendInteraction(InteractionData<Entity.Core> data, IEnumerable<string> keys = null, bool forceSender = false)
        {
            if (keys == null)
                keys = data.interactionKeys;

            if (keys.Count() == 0)
                return keys;

            if (!forceSender)
            {
                foreach (string key in data.interactionKeys.Where(x => sender.ContainsKey(x)))
                {
                    if (!sender[key](data).boolean)
                        sender["default"](data);
                }

                List<string> newKeys = data.interactionKeys.Where(x => !sender.ContainsKey(x)).ToList();

                return newKeys;
            }
            else
            {
                List<string> outputKeys = new List<string>();
                foreach(string key in data.interactionKeys)
                {
                    if ((!sender.ContainsKey(key) || !sender[key](data).boolean)
                        && !sender["default"](data).boolean)
                        outputKeys.Add(key);
                }
                return outputKeys;
            }
        }

        /// <summary>
        /// Used to handle incoming interactions from other Entities
        /// </summary>
        /// <remarks>
        /// uses all keys in <paramref name="data"/> to reroute the incoming interaction to their delegates
        /// </remarks>
        /// <param name="data">the <see cref="InteractionData{T}"/> containing all information about the interaction</param>
        /// <returns>
        ///     <list type="bullet|number|table">
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
        ///             <description>Successfully rerouted and handled the interaction</description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.FAILED"/> (102)</term>
        ///             <description>a interaction could not be rerouted</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public ReturnCode RecieveInteraction(InteractionData<Entity.Core> data)
        {
            ReturnCode output = ReturnCode.SUCCESS;

            foreach (string type in data.interactionKeys)
            {
                ReturnCode code = RecieveInteraction(type, data);
                output = (code) ? output : code;
            }

            return output;
        }

        /// <summary>
        /// Used to handle incoming interactions from other Entities
        /// </summary>
        /// <param name="type">the key used to reroute the interation to their delegates</param>
        /// <param name="data">the <see cref="InteractionData{T}"/> containing all information about the interaction</param>
        /// <returns>
        ///     <list type="bullet|number|table">
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
        ///             <description>Successfully rerouted and handled the interaction</description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.FAILED"/> (102)</term>
        ///             <description>a interaction could not be rerouted</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public ReturnCode RecieveInteraction(string type, InteractionData<Entity.Core> data)
        {
            if (reciever.ContainsKey(type))
                return reciever[type](data);
            return ReturnCode.Code(102, $"No reciever of type {type} found.");
        }
    }
}
