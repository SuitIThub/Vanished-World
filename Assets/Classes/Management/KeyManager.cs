using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeyManager : MonoBehaviour
{
    public abstract class KeyBase
    {
        public enum KeyType
        {
            mouse,
            key,
            scroll
        }

        public enum KeyDir
        {
            down,
            up,
            hold
        }

        public KeyType type { get; }

        public KeyDir direction { get; }

        public KeyMethod[] methods { get; }

        public KVStorage data;

        public KeyBase(KeyType type, KeyDir direction, KVStorage data = null, params KeyMethod[] methods)
        {
            this.type = type;
            this.direction = direction;
            this.methods = methods;
            
            if (data == null)
                data = new KVStorage();

            data.Add("keyType", type);
            data.Add("keyDir", direction);
            this.data = data;
        }

        public abstract void checkInput(object data);
    }

    public class Key : KeyBase
    {
        public KeyCode keyCode { get; }

        public Key(KeyCode keyCode, KeyDir direction, KVStorage data = null, params KeyMethod[] methods) : 
            base(KeyType.key, direction, data, methods)
        {
            this.keyCode = keyCode;
            this.data.Add("keyCode", keyCode);
        }

        public override void checkInput(object _)
        {
            bool b = false;
            switch (direction)
            {
                case KeyDir.down:
                    b = Input.GetKeyDown(keyCode);
                    break;
                case KeyDir.up:
                    b = Input.GetKeyUp(keyCode);
                    break;
                case KeyDir.hold:
                    b = Input.GetKey(keyCode);
                    break;
            }

            if (!b)
                return;

            foreach (KeyMethod km in methods)
                km(data);
        }
    }

    public class Mouse : KeyBase
    {
        public int mouseKey { get; }

        public Mouse(int mouseKey, KeyDir direction, KVStorage data = null, params KeyMethod[] methods) :
            base(KeyType.mouse, direction, data, methods)
        {
            this.mouseKey = mouseKey;

            this.data.Add("mouseKey", mouseKey);
        }

        public override void checkInput(object _)
        {
            bool b = false;
            switch (direction)
            {
                case KeyDir.down:
                    b =  Input.GetMouseButtonDown(mouseKey);
                    break;
                case KeyDir.up:
                    b =  Input.GetMouseButtonUp(mouseKey);
                    break;
                case KeyDir.hold:
                    b = Input.GetMouseButton(mouseKey);
                    break;
            }

            if (!b)
                return;

            data.Add("mousePos", CentreBrain.cam.getMousePos());

            foreach (KeyMethod km in methods)
                km(data);
        }
    }

    public class Scroll : KeyBase
    {
        public Scroll(KeyDir direction, KVStorage data = null, params KeyMethod[] methods) :
            base(KeyType.scroll, direction, data, methods)
        {

        }

        public override void checkInput(object data)
        {
            bool b = false;
            float delta = (float)data;
            switch (direction)
            {
                case KeyDir.down:
                    b = delta < 0;
                    break;
                case KeyDir.up:
                    b = delta > 0;
                    break;
                case KeyDir.hold:
                    b = delta != 0;
                    break;
            }

            if (!b)
                return;

            this.data.Set("keyDelta", delta);

            foreach (KeyMethod km in methods)
                km(this.data);
        }
    }

    public delegate void KeyMethod(KVStorage data);

    public Dictionary<string, KeyBase> keyName { get; private set; }

    private Dictionary<string, List<Key>> key;
    private Dictionary<string, List<Mouse>> mouse;
    private Dictionary<KeyBase.KeyDir, List<Scroll>> scroll;

    private void Awake()
    {
        keyName = new Dictionary<string, KeyBase>();

        key = new Dictionary<string, List<Key>>();
        mouse = new Dictionary<string, List<Mouse>>();
        scroll = new Dictionary<KeyBase.KeyDir, List<Scroll>>();
    }

    // Update is called once per frame
    void Update()
    {
        checkKeyInput();
    }
     
    private void checkKeyInput()
    {
        List<string> keyKeys = key.Keys.ToList();
        foreach(string k in keyKeys.Where(x => key.ContainsKey(x)))
            key[k].Last().checkInput(null);
        List<string> mouseKeys = mouse.Keys.ToList();
        foreach (string k in mouseKeys.Where(x => mouse.ContainsKey(x)))
            mouse[k].Last().checkInput(null);
        float delta = Input.mouseScrollDelta.y;
        List<KeyBase.KeyDir> scrollKeys = scroll.Keys.ToList();
        foreach (KeyBase.KeyDir k in scrollKeys.Where(x => scroll.ContainsKey(x)))
            scroll[k].Last().checkInput(delta);
    }

    public void Add(string name, KeyBase key)
    {
        if (keyName.ContainsKey(name))
        {
            Debug.LogError($"Key {name} has already been added!");
            return;
        }

        switch (key.type)
        {
            case KeyBase.KeyType.mouse:
                Mouse m = (Mouse)key;
                if (m.mouseKey < 0 || m.mouseKey > 9)
                    return;
                keyName.Add(name, key);
                string dataKeyM = m.mouseKey + m.direction.ToString();
                if (!mouse.ContainsKey(dataKeyM))
                    mouse.Add(dataKeyM, new List<Mouse>());
                mouse[dataKeyM].Add(m);
                return;
            case KeyBase.KeyType.key:
                Key k = (Key)key;
                keyName.Add(name, key);
                string dataKeyK = k.keyCode.ToString() + k.direction.ToString();
                if (!this.key.ContainsKey(dataKeyK))
                    this.key.Add(dataKeyK, new List<Key>());
                this.key[dataKeyK].Add(k);
                return;
            case KeyBase.KeyType.scroll:
                keyName.Add(name, key);
                if (!scroll.ContainsKey(key.direction))
                    scroll.Add(key.direction, new List<Scroll>());
                scroll[key.direction].Add((Scroll)key);
                return;
        }
    }

    public void Remove(string name)
    {
        if (!keyName.ContainsKey(name))
            return;

        KeyBase key = keyName[name];

        keyName.Remove(name);

        switch (key.type)
        {
            case KeyBase.KeyType.mouse:
                Mouse m = (Mouse)key;
                string dataKeyM = m.mouseKey + m.direction.ToString();
                mouse[dataKeyM].Remove(m);
                if (mouse[dataKeyM].Count == 0)
                    mouse.Remove(dataKeyM);
                return;
            case KeyBase.KeyType.key:
                Key k = (Key)key;
                string dataKeyK = k.keyCode.ToString() + k.direction.ToString();
                this.key[dataKeyK].Remove(k);
                if (this.key[dataKeyK].Count == 0)
                    this.key.Remove(dataKeyK);
                return;
            case KeyBase.KeyType.scroll:
                scroll[key.direction].Remove((Scroll)key);
                if (scroll[key.direction].Count == 0)
                    scroll.Remove(key.direction);
                return;
        }
    }

    public bool Pull(string name, out KeyBase key)
    {
        return Get(name, out key, true);
    }

    public bool Get(string name, out KeyBase key, bool delete = false)
    {
        key = null;

        if (!keyName.ContainsKey(name))
            return false;

        key = keyName[name];

        if (delete)
            Remove(name);

        return true;
    }
}
