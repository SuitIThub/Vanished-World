using Entity.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static KeyManager;
using static UnityEditor.Progress;
using static UnityEngine.Advertisements.Advertisement;
using static UnityEngine.EventSystems.EventTrigger;
using static ViewScanner;

namespace Entity
{
    public abstract class InteractionExecuter : IIDBase
    {
        public string id { get; }
        public string name;

        private protected string[] interactionKey;

        private protected InteractionData<Core> interactionData;

        public delegate void method();

        public method updateMethod;
        public method cancelMethod;

        public static bool isRunning = false;
        public static Core player;
        public static Inventory.Player playerInventory;

        public KVStorage data;

        private protected bool useOnOrigin = false;
        private protected bool useOnSender = false;

        private protected bool executePossible = true;

        public static void initialize(Core player)
        {
            InteractionExecuter.player = player;

            player.Inventory(out Inventory.Core inv);
            playerInventory = (Inventory.Player)inv;
        }

        public InteractionExecuter(KVStorage json, KVStorage data, KVStorage replaceData, string separator = "{}")
        {
            id = IdUtilities.id;

            json.getElement("name", ref name, replaceData, separator);

            List<object> oList = new List<object>();
            if (json.getElement("interaction", ref oList, replaceData, separator))
                interactionKey = oList.OfType<string>().ToArray();
            else
                interactionKey = new string[0];

            json.getElement("useOnOrigin", ref useOnOrigin, replaceData, separator);
            json.getElement("useOnSender", ref useOnSender, replaceData, separator);

            this.data = json;
        }

        public static InteractionExecuter createInteractionFromPreset(string preset, KVStorage data,
                                                                      KVStorage replaceData, string separator = "{}")
        {
            TextAsset ta = Resources.Load("Json/Interaction/" + preset) as TextAsset;
            KVStorage json = MiniJSON.Json.Deserialize(ta.text) as KVStorage;

            string type = "";
            json.getElement("data.type", ref type);

            return type switch
            {
                "scan" => new InteractionExecuterViewScan(json, data, replaceData, separator),
                _ => null
            };
        }

        public abstract void Trigger(KVStorage data, Core origin, Core sender = null, KVStorage replaceData = null);

        private protected abstract void updateInteraction(ViewScanner scanner, List<Core> filteredEntities,
                                                          List<Core> unfilteredEntities, Vector2 pos);

        private protected void executeInteractions(InteractionData<Core> data)
        {
            List<Core> entities = null;
            data.getElement("entities", ref entities);

            if (useOnSender && !entities.Contains(data.sender) && entities.Count == 0)
                entities.Add(data.sender);

            if (useOnOrigin && !entities.Contains(data.origin) && entities.Count == 0)
                entities.Add(data.origin);

            data.data.Set("entities", entities);

            IEnumerable<string> keys = data.interactionKeys;
            if (data.origin.Interact(out Interaction.Core interactO))
                keys = interactO.SendInteraction(data, keys);

            if (data.origin.id != data.sender.id &&
                data.sender.Interact(out Interaction.Core interactS))
                keys = interactS.SendInteraction(data, keys);

            if (CentreBrain.data.player.Interact(out Interaction.Core interactP))
                interactP.SendInteraction(data, keys, true);
        }

        public void cancelInteractionExtern(IIDBase sender)
        {
            if (sender.id == player.id)
                cancelInteraction();
        }

        private protected virtual void cancelInteraction(KVStorage _ = null)
        {
            CentreBrain.keyManager.Remove(id + "_Interaction_MoveUp");
            CentreBrain.keyManager.Remove(id + "_Interaction_MoveDown");
            CentreBrain.keyManager.Remove(id + "_Interaction_Select");
            CentreBrain.keyManager.Remove(id + "_Interaction_Confirm");
            CentreBrain.keyManager.Remove(id + "_Interaction_Cancel");

            cancelMethod?.Invoke();
        }


        private protected virtual void confirmInteraction(ViewScanner _, List<Core> _1, List<Core> _2, Vector2 _3)
        {
            executeInteractions(interactionData);
        }

        private protected abstract void registerKeys();

        private protected abstract void loadData(KVStorage data, KVStorage replaceData = null, string separator = "{}");
    }
}
