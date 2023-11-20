using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    public class Core : IIDBase
    {
        [Serializable]
        public class Save
        {

        }

        public string id { get; }

        public string name { get; private set; }
        public string _displayName { get; private set; }
        public string displayName 
        { 
            get => _displayName.replaceData(this.Data(), "[]");
        }
        public string _displayNamePlural { get; private set; }
        public string displayNamePlural
        {
            get => _displayNamePlural.replaceData(this.Data(), "[]");
        }

        public string type { get; private set; }
        public string objectType { get; private set; }

        public Dictionary<string, IModule> modules;

        public GameObject gameObject { get; protected private set; }

        public SpriteData icon;

        public bool isDestroyed { get; private set; } = false;

        public Vector2 pos
        {
            get
            {
                if (gameObject == null || gameObject.transform == null)
                    return Vector2.negativeInfinity;

                return gameObject.transform.position;
            }
            set => gameObject.transform.position = value.toVec3();
        }

        public Vector2 width;
        public Vector2 offset;
        public Vector2 absPos { get => new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y)); }
        public Vector2 realPos { get => (pos + offset); }

        public Core()
        {
            id = IdUtilities.id;

            name = "EMPTY";
            _displayName = "EMPTY";
            type = "none";
            objectType = "none";

            modules = new Dictionary<string, IModule>();
            gameObject = null;
        }

        public Core(KVStorage json, Vector2 pos, KVStorage replaceData = null, string separator = "{}")
        {
            gameObject = null;
            id = IdUtilities.id;

            string name = "EMPTY";
            json.getElement("name", ref name, replaceData, separator);
            this.name = name;

            string displayName = name;
            json.getElement("displayName", ref displayName, replaceData, separator);
            this._displayName = displayName;

            string displayNamePlural = displayName;
            json.getElement("displayNamePlural", ref displayNamePlural, replaceData, separator);
            this._displayNamePlural = displayNamePlural;

            string type = "EMPTY";
            json.getElement("type", ref type, replaceData, separator);
            this.type = type;

            string objectType = "EMPTY";
            json.getElement("objectType", ref objectType, replaceData, separator);
            this.objectType = objectType;

            if (!json.getElement("icon", ref icon, replaceData, separator))
                SpriteData.createSprite(out icon);

            KVStorage componentData = new KVStorage();

            gameObject = null;
            KVStorage goKV = null;
            if (json.getElement("gameObject", ref goKV))
            {
                goKV.Add("name", id);

                goKV.getElement("position", ref pos, replaceData, separator);
                goKV.Set("position", pos);


                Vector2 width = Vector2.one;
                goKV.getElement("width", ref width, replaceData, separator);
                this.width = width;

                Vector2 offset = Vector2.zero;
                goKV.getElement("offset", ref offset, replaceData, separator);
                this.offset = offset;

                Vector2 scale = Vector2.one;
                goKV.getElement("scale", ref scale, replaceData, separator);
                goKV.Set("scale", scale);

                goKV.Add("preset", "Entity");

                Transform parentGO = CentreBrain.data.Folders.OBJ.transform;
                if (objectType == "OBJ")
                    parentGO = CentreBrain.data.Folders.OBJ.transform;
                else if (objectType == "NPC")
                    parentGO = CentreBrain.data.Folders.NPC.transform;
                else if (objectType == "ITEM")
                    parentGO = CentreBrain.data.Folders.ITEM.transform;
                goKV.Add("parent", parentGO);

                if (GameObjectSpawnExtensions.spawnEntityGameObject(id, goKV, out GameObject go, componentData, replaceData, separator))
                    gameObject = go;
            }

            KVStorage publicReplace = (replaceData != null) ? new KVStorage(replaceData) : new KVStorage();
            List<object> privateKeys = null;
            if (!json.getElement("data.localOnly", ref privateKeys))
                privateKeys = new List<object>();
            publicReplace.Remove(privateKeys);

            this.modules = new Dictionary<string, IModule>();
            List<object> modules = null;
            List<string> addedModules = new List<string>();
            if (json.getElement("modules", ref modules, replaceData, separator))
            {
                foreach (object m in modules)
                {
                    KVStorage moduleKV = null;
                    if (m.getElement(ref moduleKV))
                    {
                        moduleKV = moduleKV.replaceData(replaceData, separator);
                        moduleKV.Add("parent", this);
                        string moduleType = "";
                        if (moduleKV.getElement("module", ref moduleType))
                        {
                            if (moduleType == "information")
                            {
                                moduleKV.Add("replaceData", publicReplace);
                                KVStorage overwriteData = null;
                                if (!json.getElement("data.overwriteData", ref overwriteData, publicReplace, "{}"))
                                    overwriteData = new KVStorage();
                                moduleKV.Add("overwriteData", overwriteData);
                            }

                            if (ModuleExtensions.GetModule(this, moduleKV, out IModule module))
                            {
                                addedModules.Add(moduleType);
                                this.modules.Add(moduleType, module);
                            }
                        }
                    }
                } 
            }

            if (!addedModules.Contains("information"))
            {
                KVStorage kv = new KVStorage
                {
                    { "module", "information" },
                    { "type", "dummy" },
                    { "parent", this },
                    { "replaceData", publicReplace }
                };
                if (ModuleExtensions.GetModule(this, kv, out IModule module))
                    this.modules.Add("information", module);
            }

            if (gameObject)
            {
                this.Info().setElement("components", componentData, false);
            }

        }
        
        /// <summary>
        /// destroys the GameObject of this Entity
        /// </summary>
        public void DestroyGO()
        {
            if (gameObject)
                GameObject.Destroy(gameObject);
            gameObject = null;
        }

        /// <summary>
        /// Destroys the Entity, its GameObject and all contained Modules
        /// </summary>
        public void Destroy()
        {
            CentreBrain.data.entities.Remove(id);
            foreach(IModule module in modules.Values)
                module.Destroy();
            CentreBrain.messageSystem.destroyMessagesByID(id);
            DestroyGO();
            isDestroyed = true;
        }

        #region Debug

        public virtual void DebugWindow(DebugWindow window)
        {
            GUILayout.Label($"ID: {id}");
            GUILayout.Label($"Name: {name}");
            GUILayout.Label($"displayName: {displayName}");
            GUILayout.Label($"displayNamePlural: {displayNamePlural}");
            GUILayout.Space(5);
            GUILayout.Label($"Position: {pos.x}:{pos.y}");
            GUILayout.Label($"Absolute Position: {absPos.x}:{absPos.y}");
            GUILayout.Label($"Real Position: {realPos.x}:{realPos.y}");
            GUILayout.Space(5);
            GUILayout.Label($"Width: {width.x}:{width.y}");
            GUILayout.Label($"Offset: {offset.x}:{offset.y}");
        }

        #endregion
    }
}