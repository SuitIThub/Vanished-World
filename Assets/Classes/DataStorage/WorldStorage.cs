using Entity;
using System.Collections.Generic;
using UnityEngine;

public class WorldStorage
{
    public class Prefab
    {
        public Dictionary<string, GameObject> Prefabs { get; private set; }

        public GameObject this[string key]
        {
            get 
            {
                if (!Prefabs.ContainsKey(key))
                    return null;
                return Prefabs[key]; 
            }
        }

        public Prefab()
        {
            Prefabs = new Dictionary<string, GameObject>
            {
                ["Message"                   ] = Load("Presets/Message")                     ,
                ["Entity"                    ] = Load("Presets/Entity")                      ,
                ["Item"                      ] = Load("Presets/Item")                        ,
                ["MeshGraphic"               ] = Load("Presets/MeshGraphic")                 ,
                ["MultiSlider"               ] = Load("Components/Multislider/Multislider")  ,
                ["MultiSliderSlider"         ] = Load("Components/Multislider/Prefab/Slider"),
                ["Window"                    ] = Load("Presets/Window/Window")               ,
                ["WindowModule"              ] = Load("Presets/Window/Module")               ,
                ["WindowModuleImage"         ] = Load("Presets/Window/Image")                ,
                ["WindowModuleText"          ] = Load("Presets/Window/Text")                 ,
                ["WindowModuleInteraction"   ] = Load("Presets/Window/Interaction")          ,
                ["WindowModuleInteractionTag"] = Load("Presets/Window/Tag")                  ,
            };
        }

        public bool ContainsKey(string key)
        {
            return Prefabs.ContainsKey(key);
        }

        public static GameObject Load(string preset)
        {
            return (GameObject)Resources.Load(preset);
        }
    }

    public class Folder
    {
        public GameObject Message;

        public GameObject NPC;
        public GameObject OBJ;
        public GameObject ITEM;

        public GameObject Graphics;
        public GameObject ElementDisplay;

        public GameObject DisplayScreen;

        public Folder()
        {
            Message = GameObject.Find("MessagesFolder");

            NPC = GameObject.Find("NPC");
            OBJ = GameObject.Find("OBJ");
            ITEM = GameObject.Find("ITEM");

            Graphics = GameObject.Find("Graphics");
            ElementDisplay = GameObject.Find("ElementDisplay");

            DisplayScreen = GameObject.Find("DisplayScreen");
        }
    }

    public class JsonInterface
    {
        private Dictionary<string, KVStorage> interfaces;

        public KVStorage this[string key]
        {
            get
            {
                if (!interfaces.ContainsKey(key))
                    return new KVStorage();
                return interfaces[key];
            }
        }

        public JsonInterface()
        {
            interfaces = new Dictionary<string, KVStorage>();

            TextAsset[] texts = Resources.LoadAll<TextAsset>("Json/Interface");
            foreach(TextAsset text in texts)
            {
                KVStorage kv = MiniJSON.Json.Deserialize(text.text) as KVStorage;
                string name = "";
                if (kv.getElement("interface", ref name))
                {
                    kv.Remove("interface");
                    interfaces.Add(name, kv);
                }
            }
        }
    }

    public Entity.Core player;

    /// <completionlist cref="CentreBrain"/>
    public Prefab Prefabs;
    public Folder Folders;
    public JsonInterface jsonInterface;

    public Dictionary<string, Entity.Core> entities;

    public delegate void UpdateMethod();
    public Dictionary<string, UpdateMethod> updateMethod;

    public int sortingCenter = 0;

    public Dictionary<string, InteractionExecuter> interactions;
    public Dictionary<string, object> elementDisplays;

    public WorldStorage()
    {
        Prefabs = new Prefab();
        Folders = new Folder();
        jsonInterface = new JsonInterface();

        entities = new Dictionary<string, Entity.Core>();

        updateMethod = new Dictionary<string, UpdateMethod>();

        interactions = new Dictionary<string, InteractionExecuter>();
    }

    public void removeUpdateMethodPerBaseID(string id)
    {
        List<string> keys = new List<string>(updateMethod.Keys);
        keys.ForEach(key => { if (key.StartsWith(id)) updateMethod.Remove(key); });
    }
}
