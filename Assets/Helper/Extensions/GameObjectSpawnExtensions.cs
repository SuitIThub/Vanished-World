using System.Collections.Generic;
using UnityEngine;

public static class GameObjectSpawnExtensions
{
    private delegate ReturnCode spawnComp(KVStorage json, GameObject parent, KVStorage compStorage, KVStorage replaceData = null, string separator = "{}");
    private static Dictionary<string, spawnComp> compSpawner = new Dictionary<string, spawnComp>()
    {
        {"rigidbody", spawnRigidbody},
        {"spriteRenderer", spawnSpriteRenderer},
        {"collider", spawnCollider},
        {"sortingOrderUpdater", spawnSortingOrderUpdater},
        {"outline", spawnOutliner}
    };

    /// <summary>
    /// Creates and inserts the GameObject for a <see cref="Entity.Core"/> into the Game World
    /// </summary>
    /// <param name="id">The <see cref="Entity.Core.id"/> to be used to name the GameObject for linking</param>
    /// <param name="json">The <see cref="KVStorage"/> containing the Data to build the GameObject</param>
    /// <param name="go">The finished GameObject</param>
    /// <param name="compStorage">The <see cref="KVStorage"/> containing all the created GameObject components for the <see cref="Entity.Core"/> to use</param>
    /// <param name="replaceData">The <see cref="Entity.Core"/>-replaceData containing potential inheriting-data</param>
    /// <param name="separator">Seperator used by <paramref name="replaceData"/> to find keys in strings</param>
    /// <returns>
    ///     <list type="bullet|number|table">
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
    ///             <description>Successfully spawned GameObject</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ReturnCode spawnEntityGameObject(string id, KVStorage json, out GameObject go, KVStorage compStorage, KVStorage replaceData = null, string separator = "{}")
    {
        Vector2 position = Vector2.zero;
        json.getElement("position", ref position, replaceData, separator);
        Vector2 scale = Vector2.one;
        json.getElement("scale", ref scale, replaceData, separator);
        Transform parent = null;
        json.getElement("parent", ref parent, replaceData, separator);
        GameObject preset = CentreBrain.data.Prefabs["Entity"];
        string presetStr = "Entity";
        json.getElement("preset", ref presetStr, replaceData, separator);
        if (CentreBrain.data.Prefabs.ContainsKey("presetStr"))
            preset = CentreBrain.data.Prefabs[presetStr];

        go = GameObject.Instantiate(preset, Vector3.zero, Quaternion.identity, parent);

        go.transform.localPosition = position;

        go.transform.localScale = scale;

        go.name = id;

        //load GameObject-Components
        List<object> components = null;
        if (json.getElement("components", ref components, replaceData, separator))
        {
            foreach (object o in components)
            {
                KVStorage kv = null;

                if (!o.getElement(ref kv, replaceData, separator))
                    continue;

                spawnComponent(kv, go, compStorage, replaceData, separator);

            }
        }

        ReturnCode outputCode = ReturnCode.SUCCESS;

        //load Sub-GameObjects
        List<object> children = null;
        if (json.getElement("children", ref children))
        {
            foreach (object o in children)
            {
                KVStorage childKV = null;
                if (o.getElement(ref childKV))
                {
                    childKV.Add("parent", go.transform);
                    ReturnCode code = spawnEntityGameObject(id, childKV, out _, compStorage, replaceData, separator);
                    outputCode = (code.boolean) ? outputCode : code;
                }
            }
        }

        return outputCode;
    }


    /// <summary>
    /// finds the correct component-creators and initializes their creation
    /// </summary>
    /// <param name="json">the <see cref="KVStorage"/> containing the data needed for creating the component</param>
    /// <param name="parent">the GameObject where the component will be added</param>
    /// <param name="compStorage">The <see cref="KVStorage"/> containing all the created GameObject components for the <see cref="Entity.Core"/> to use</param>
    /// <param name="replaceData">The <see cref="Entity.Core"/>-replaceData containing potential inheriting-data</param>
    /// <param name="separator">Seperator used by <paramref name="replaceData"/> to find keys in strings</param>
    /// <returns>
    ///     <list type="bullet|number|table">
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
    ///             <description>Successfully created all Components</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.INVALID_VALUE"/> (102)</term>
    ///             <description>Either 'type'-keyword is missing in <paramref name="json"/> or there is no way to create the component for 'type'</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ReturnCode spawnComponent(KVStorage json, GameObject parent, KVStorage compStorage, KVStorage replaceData = null, string separator = "{}")
    {
        string type = "";
        if (json.getElement("type", ref type, replaceData, separator))
        {
            if (compSpawner.ContainsKey(type))
                return compSpawner[type](json, parent, compStorage, replaceData, separator);
            return ReturnCode.Code(102, $"type:{type} for component doesn't exist", true);
        }

        return ReturnCode.Code(102, $"json for GameObject in {parent.name} doesn't contain a 'type' for the component", true);
    }

    /// <summary>
    /// creates a <see cref="Rigidbody2D"/>-component and inserts it into the GameObject
    /// </summary>
    /// <param name="json">the <see cref="KVStorage"/> containing the data needed for creating the component</param>
    /// <param name="parent">the GameObject where the component will be added</param>
    /// <param name="compStorage">The <see cref="KVStorage"/> containing all the created GameObject components for the <see cref="Entity.Core"/> to use</param>
    /// <param name="replaceData">The <see cref="Entity.Core"/>-replaceData containing potential inheriting-data</param>
    /// <param name="separator">Seperator used by <paramref name="replaceData"/> to find keys in strings</param>
    /// <returns>
    ///     <list type="bullet|number|table">
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
    ///             <description>Successfully created all Components</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ReturnCode spawnRigidbody(KVStorage json, GameObject parent, KVStorage compStorage, KVStorage replaceData = null, string separator = "{}")
    {
        Rigidbody2D rb = parent.AddComponent<Rigidbody2D>();

        RigidbodyType2D bodyType = RigidbodyType2D.Dynamic;
        json.getEnum("bodyType", ref bodyType, replaceData, separator);
        rb.bodyType = bodyType;

        bool simulated = false;
        json.getElement("simulated", ref simulated, replaceData, separator);
        rb.simulated = simulated;

        bool autoMass = false;
        json.getElement("autoMass", ref autoMass, replaceData, separator);
        rb.useAutoMass = autoMass;

        float mass = 1;
        json.getElement("mass", ref mass, replaceData, separator);
        rb.mass = mass;

        float linearDrag = 0;
        json.getElement("linearDrag", ref linearDrag, replaceData, separator);
        rb.drag = linearDrag;

        float angularDrag = 0;
        json.getElement("angularDrag", ref angularDrag, replaceData, separator);
        rb.angularDrag = angularDrag;

        float gravity = 0;
        json.getElement("gravity", ref gravity, replaceData, separator);
        rb.gravityScale = gravity;

        CollisionDetectionMode2D collision = CollisionDetectionMode2D.Discrete;
        json.getEnum("collision", ref collision, replaceData, separator);
        rb.collisionDetectionMode = collision;

        RigidbodySleepMode2D sleepMode = RigidbodySleepMode2D.StartAwake;
        json.getEnum("sleepMode", ref sleepMode, replaceData, separator);
        rb.sleepMode = sleepMode;

        RigidbodyConstraints2D constraint = RigidbodyConstraints2D.None;
        List<object> list = null;
        if (json.getElement("constraints", ref list))
        {
            bool[] bools = { false, false, false };

            foreach(object o in list)
            {
                string s = "";
                o.getElement(ref s, replaceData, separator);

                if (s == "x")
                    bools[0] = true;
                else if (s == "y")
                    bools[1] = true;
                else if (s == "z")
                    bools[2] = true;
            }

            if (bools[0])
            {
                if (bools[1])
                {
                    if (bools[2])
                        constraint = RigidbodyConstraints2D.FreezeAll;
                    else
                        constraint = RigidbodyConstraints2D.FreezePosition;
                }
                else
                {
                    if (bools[2])
                        constraint = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                    else
                        constraint = RigidbodyConstraints2D.FreezePositionX;
                }
            }
            else
            {
                if (bools[1])
                {
                    if (bools[2])
                        constraint = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                    else
                        constraint = RigidbodyConstraints2D.FreezePositionY;
                }
                else
                {
                    if (bools[2])
                        constraint = RigidbodyConstraints2D.FreezeRotation;
                    else
                        constraint = RigidbodyConstraints2D.None;
                }
            }
        }
        rb.constraints = constraint;

        if (!compStorage.ContainsKey("rigidbody"))
            compStorage.Add("rigidbody", new List<Rigidbody2D>());
        ((List<Rigidbody2D>)compStorage["rigidbody"]).Add(rb);

        return ReturnCode.SUCCESS;
    }

    /// <summary>
    /// creates a <see cref="SpriteRenderer"/>-component and inserts it into the GameObject
    /// </summary>
    /// <param name="json">the <see cref="KVStorage"/> containing the data needed for creating the component</param>
    /// <param name="parent">the GameObject where the component will be added</param>
    /// <param name="compStorage">The <see cref="KVStorage"/> containing all the created GameObject components for the <see cref="Entity.Core"/> to use</param>
    /// <param name="replaceData">The <see cref="Entity.Core"/>-replaceData containing potential inheriting-data</param>
    /// <param name="separator">Seperator used by <paramref name="replaceData"/> to find keys in strings</param>
    /// <returns>
    ///     <list type="bullet|number|table">
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
    ///             <description>Successfully created all Components</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ReturnCode spawnSpriteRenderer(KVStorage json, GameObject parent, KVStorage compStorage, KVStorage replaceData = null, string separator = "{}")
    {
        SpriteRenderer sr = parent.AddComponent<SpriteRenderer>();

        SpriteData data = null;
        if (!json.getElement("texture", ref data, replaceData, separator))
            SpriteData.createSprite(out data);
        sr.sprite = data.sprite;

        if (json.checkElement("flip", "x", replaceData, separator))
            sr.flipX = true;
        else if (json.checkElement("flip", "y", replaceData, separator))
            sr.flipY = true;
        else
        {
            if (json.checkElement("flip.0", "x", replaceData, separator))
                sr.flipX = true;
            else if (json.checkElement("flip.0", "y", replaceData, separator))
                sr.flipY = true;
            if (json.checkElement("flip.1", "x", replaceData, separator))
                sr.flipX = true;
            else if (json.checkElement("flip.1", "y", replaceData, separator))
                sr.flipY = true;
        }

        SpriteDrawMode drawMode = SpriteDrawMode.Simple;
        json.getEnum("drawMode", ref drawMode, replaceData, separator);
        sr.drawMode = drawMode;

        SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.None;
        json.getEnum("maskInteraction", ref maskInteraction, replaceData, separator);
        sr.maskInteraction = maskInteraction;

        SpriteSortPoint sortPoint = SpriteSortPoint.Center;
        json.getEnum("sortPoint", ref maskInteraction, replaceData, separator);
        sr.spriteSortPoint = sortPoint;

        int sortingLayerID = 0;
        string sortingLayerName = "";
        if (json.getElement("sortingLayer", ref sortingLayerID, replaceData, separator))
            sr.sortingLayerID = sortingLayerID;
        else if (json.getElement("sortingLayer", ref sortingLayerName, replaceData, separator))
            sr.sortingLayerName = sortingLayerName;

        int sortingOffset = 0;
        json.getElement("sortingOffset", ref sortingOffset, replaceData, separator);
        sr.sortingOrder = 32767 - (int)(parent.transform.position.y * 10) - sortingOffset;

        if (!compStorage.ContainsKey("spriteRenderer"))
            compStorage.Add("spriteRenderer", new List<SpriteRenderer>());
        ((List<SpriteRenderer>)compStorage["spriteRenderer"]).Add(sr);

        return ReturnCode.SUCCESS;
    }

    /// <summary>
    /// creates a <see cref="Collider2D"/>-component and inserts it into the GameObject
    /// </summary>
    /// <remarks>
    /// depending on the collider type defined in <paramref name="json"/>, a sub-method will be used to create a specific type of <see cref="Collider2D"/>
    /// </remarks>
    /// <param name="json">the <see cref="KVStorage"/> containing the data needed for creating the component</param>
    /// <param name="parent">the GameObject where the component will be added</param>
    /// <param name="compStorage">The <see cref="KVStorage"/> containing all the created GameObject components for the <see cref="Entity.Core"/> to use</param>
    /// <param name="replaceData">The <see cref="Entity.Core"/>-replaceData containing potential inheriting-data</param>
    /// <param name="separator">Seperator used by <paramref name="replaceData"/> to find keys in strings</param>
    /// <returns>
    ///     <list type="bullet|number|table">
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
    ///             <description>Successfully created all Components</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ReturnCode spawnCollider(KVStorage json, GameObject parent, KVStorage compStorage, KVStorage replaceData = null, string separator = "{}")
    {
        string type = "";
        json.getElement("collider", ref type, replaceData, separator);

        switch (type)
        {
            case "box":
                return spawnBoxCollider(json, parent, compStorage, replaceData, separator);
            case "circle":
                return spawnCircleCollider(json, parent, compStorage, replaceData, separator);
            case "capsule":
                return spawnCapsuleCollider(json, parent, compStorage, replaceData, separator);
        }

        return ReturnCode.SUCCESS;
    }

    /// <summary>
    /// creates a <see cref="BoxCollider2D"/>-component and inserts it into the GameObject
    /// </summary>
    /// <param name="json">the <see cref="KVStorage"/> containing the data needed for creating the component</param>
    /// <param name="parent">the GameObject where the component will be added</param>
    /// <param name="compStorage">The <see cref="KVStorage"/> containing all the created GameObject components for the <see cref="Entity.Core"/> to use</param>
    /// <param name="replaceData">The <see cref="Entity.Core"/>-replaceData containing potential inheriting-data</param>
    /// <param name="separator">Seperator used by <paramref name="replaceData"/> to find keys in strings</param>
    /// <returns>
    ///     <list type="bullet|number|table">
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
    ///             <description>Successfully created all Components</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ReturnCode spawnBoxCollider(KVStorage json, GameObject parent, KVStorage compStorage, KVStorage replaceData = null, string separator = "{}")
    {
        BoxCollider2D bc = parent.AddComponent<BoxCollider2D>();

        bool isTrigger = false;
        json.getElement("isTrigger", ref isTrigger, replaceData, separator);
        bc.isTrigger = isTrigger;

        bool effector = false;
        json.getElement("effector", ref effector, replaceData, separator);
        bc.usedByEffector = effector;

        bool composite = false;
        json.getElement("composite", ref composite, replaceData, separator);
        bc.usedByComposite = composite;

        bool autoTiling = false;
        json.getElement("autoTiling", ref autoTiling, replaceData, separator);
        bc.autoTiling = autoTiling;

        Vector2 offset = Vector2.zero;
        json.getElement("offset", ref offset, replaceData, separator);
        bc.offset = offset;

        Vector2 size = Vector2.one;
        json.getElement("size", ref size, replaceData, separator);
        bc.size = size;

        float edgeRadius = 0;
        json.getElement("edgeRadius", ref edgeRadius, replaceData, separator);
        bc.edgeRadius = edgeRadius;

        if (!compStorage.ContainsKey("collider"))
            compStorage.Add("collider", new List<Collider2D>());
        ((List<Collider2D>)compStorage["collider"]).Add(bc);

        return ReturnCode.SUCCESS;
    }

    /// <summary>
    /// creates a <see cref="CircleCollider2D"/>-component and inserts it into the GameObject
    /// </summary>
    /// <param name="json">the <see cref="KVStorage"/> containing the data needed for creating the component</param>
    /// <param name="parent">the GameObject where the component will be added</param>
    /// <param name="compStorage">The <see cref="KVStorage"/> containing all the created GameObject components for the <see cref="Entity.Core"/> to use</param>
    /// <param name="replaceData">The <see cref="Entity.Core"/>-replaceData containing potential inheriting-data</param>
    /// <param name="separator">Seperator used by <paramref name="replaceData"/> to find keys in strings</param>
    /// <returns>
    ///     <list type="bullet|number|table">
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
    ///             <description>Successfully created all Components</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ReturnCode spawnCircleCollider(KVStorage json, GameObject parent, KVStorage compStorage, KVStorage replaceData = null, string separator = "{}")
    {
        CircleCollider2D cc = parent.AddComponent<CircleCollider2D>();

        bool isTrigger = false;
        json.getElement("isTrigger", ref isTrigger, replaceData, separator);
        cc.isTrigger = isTrigger;

        bool effector = false;
        json.getElement("effector", ref effector, replaceData, separator);
        cc.usedByEffector = effector;

        Vector2 offset = Vector2.zero;
        json.getElement("offset", ref offset, replaceData, separator);
        cc.offset = offset;

        float radius = 0;
        json.getElement("radius", ref radius, replaceData, separator);
        cc.radius = radius;

        if (!compStorage.ContainsKey("collider"))
            compStorage.Add("collider", new List<Collider2D>());
        ((List<Collider2D>)compStorage["collider"]).Add(cc);

        return ReturnCode.SUCCESS;
    }

    /// <summary>
    /// creates a <see cref="CapsuleCollider2D"/>-component and inserts it into the GameObject
    /// </summary>
    /// <param name="json">the <see cref="KVStorage"/> containing the data needed for creating the component</param>
    /// <param name="parent">the GameObject where the component will be added</param>
    /// <param name="compStorage">The <see cref="KVStorage"/> containing all the created GameObject components for the <see cref="Entity.Core"/> to use</param>
    /// <param name="replaceData">The <see cref="Entity.Core"/>-replaceData containing potential inheriting-data</param>
    /// <param name="separator">Seperator used by <paramref name="replaceData"/> to find keys in strings</param>
    /// <returns>
    ///     <list type="bullet|number|table">
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
    ///             <description>Successfully created all Components</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ReturnCode spawnCapsuleCollider(KVStorage json, GameObject parent, KVStorage compStorage, KVStorage replaceData = null, string separator = "{}")
    {
        CapsuleCollider2D cc = parent.AddComponent<CapsuleCollider2D>();

        bool isTrigger = false;
        json.getElement("isTrigger", ref isTrigger, replaceData, separator);
        cc.isTrigger = isTrigger;

        bool effector = false;
        json.getElement("effector", ref effector, replaceData, separator);
        cc.usedByEffector = effector;

        Vector2 offset = Vector2.zero;
        json.getElement("offset", ref offset, replaceData, separator);
        cc.offset = offset;

        Vector2 size = Vector2.one;
        json.getElement("size", ref size, replaceData, separator);
        cc.size = size;

        CapsuleDirection2D direction = CapsuleDirection2D.Vertical;
        json.getEnum("direction", ref direction, replaceData, separator);
        cc.direction = direction;

        if (!compStorage.ContainsKey("collider"))
            compStorage.Add("collider", new List<Collider2D>());
        ((List<Collider2D>)compStorage["collider"]).Add(cc);

        return ReturnCode.SUCCESS;
    }

    /// <summary>
    /// creates a <see cref="SortingOrderUpdater"/>-component and inserts it into the GameObject
    /// </summary>
    /// <param name="json">the <see cref="KVStorage"/> containing the data needed for creating the component</param>
    /// <param name="parent">the GameObject where the component will be added</param>
    /// <param name="compStorage">The <see cref="KVStorage"/> containing all the created GameObject components for the <see cref="Entity.Core"/> to use</param>
    /// <param name="replaceData">The <see cref="Entity.Core"/>-replaceData containing potential inheriting-data</param>
    /// <param name="separator">Seperator used by <paramref name="replaceData"/> to find keys in strings</param>
    /// <returns>
    ///     <list type="bullet|number|table">
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
    ///             <description>Successfully created all Components</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ReturnCode spawnSortingOrderUpdater(KVStorage json, GameObject parent, KVStorage compStorage, KVStorage replaceData = null, string separator = "{}")
    {
        SortingOrderUpdater sou = parent.AddComponent<SortingOrderUpdater>();

        json.getElement("sortingOffset", ref sou.sortingOffset, replaceData, separator);

        if (!compStorage.ContainsKey("sortingOrderUpdater"))
            compStorage.Add("sortingOrderUpdater", new List<SortingOrderUpdater>());
        ((List<SortingOrderUpdater>)compStorage["sortingOrderUpdater"]).Add(sou);

        return ReturnCode.SUCCESS;
    }

    /// <summary>
    /// creates a <see cref="cakeslice.Outline"/>-component and inserts it into the GameObject
    /// </summary>
    /// <param name="json">the <see cref="KVStorage"/> containing the data needed for creating the component</param>
    /// <param name="parent">the GameObject where the component will be added</param>
    /// <param name="compStorage">The <see cref="KVStorage"/> containing all the created GameObject components for the <see cref="Entity.Core"/> to use</param>
    /// <param name="replaceData">The <see cref="Entity.Core"/>-replaceData containing potential inheriting-data</param>
    /// <param name="separator">Seperator used by <paramref name="replaceData"/> to find keys in strings</param>
    /// <returns>
    ///     <list type="bullet|number|table">
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
    ///             <description>Successfully created all Components</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ReturnCode spawnOutliner(KVStorage json, GameObject parent, KVStorage compStorage, KVStorage replaceData = null, string separator = "{}")
    {
        cakeslice.Outline o = parent.AddComponent<cakeslice.Outline>();

        o.enabled = false;

        int color = 0;
        json.getElement("color", ref color, replaceData, separator);
        o.color = color;

        if (!compStorage.ContainsKey("outline"))
            compStorage.Add("outline", new List<cakeslice.Outline>());
        ((List<cakeslice.Outline>)compStorage["outline"]).Add(o);

        return ReturnCode.SUCCESS;
    }
}
