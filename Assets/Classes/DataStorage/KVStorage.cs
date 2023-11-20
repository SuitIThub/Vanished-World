using System.Collections.Generic;

public class KVStorage : Dictionary<string, object>
{
    public Dictionary<string, object> data
    {
        get
        {
            return new Dictionary<string, object>(this);
        }
    }

    public KVStorage() : base()
    {
        
    }

    public KVStorage(int capacity) : base(capacity)
    {

    }

    public KVStorage(IEnumerable<KeyValuePair<string, object>> collection) : base(collection)
    {

    }

    public KVStorage(IDictionary<string, object> dict) : base(dict)
    {

    }

    public KVStorage(string key, object value) : base()
    {
        Add(key, value);
    }


    public void Add(string key, object value, bool createMissing = false)
    {
        if (key.Contains('.'))
        {
            string[] keys = key.Split('.', 2);

            if (ContainsKey(keys[0]))
            {
                if (this[keys[0]] is KVStorage)
                    (this[keys[0]] as KVStorage).Add(keys[1], value);
                else if (this[keys[0]] is List<object>)
                    (this[keys[0]] as List<object>).AddElem(keys[1], value);
            }
            else
            {
                if (int.TryParse(keys[1], out int pos))
                {
                    List<object> list = new List<object>();
                    Add(keys[0], list);
                    list.AddElem(keys[1], value);

                }
                else
                {
                    KVStorage kv = new KVStorage();
                    Add(keys[0], kv);
                    kv.Add(keys[1], value);
                }
            }
            

            return;
        }

        if (!ContainsKey(key))
            TryAdd(key, value);
    }

    public void Add(KeyValuePair<string, object> kvp)
    {
        Add(kvp.Key, kvp.Value);
    }

    public void Add(KVStorage dict)
    {
        foreach (KeyValuePair<string, object> kvp in dict)
            Add(kvp);
    }

    public void Set(string key, object value)
    {
        if (key.Contains('.'))
        {
            string[] keys = key.Split('.', 2);

            if (ContainsKey(keys[0]))
            {
                if (this[keys[0]] is KVStorage)
                    (this[keys[0]] as KVStorage).Set(keys[1], value);
                else if (this[keys[0]] is List<object>)
                    (this[keys[0]] as List<object>).Set(keys[1], value);
            }
            else
            {
                if (int.TryParse(keys[1], out int pos))
                {
                    List<object> list = new List<object>();
                    Add(keys[0], list);
                    list.AddElem(keys[1], value);

                }
                else
                {
                    KVStorage kv = new KVStorage();
                    Add(keys[0], kv);
                    kv.Add(keys[1], value);
                }
            }

            return;
        }

        if (!ContainsKey(key))
            Add(key, value);
        else
            this[key] = value;
    }

    public void Set(KeyValuePair<string, object> kvp)
    {
        Set(kvp.Key, kvp.Value);
    }

    public void Set(KVStorage dict)
    {
        foreach (KeyValuePair<string, object> kvp in dict)
            Set(kvp.Key, kvp.Value);
    }

    public void Remove(IEnumerable<string> keys)
    {
        if (keys == null)
            return;

        foreach(string key in keys)
            Remove(key);
    }

    public void Remove(IEnumerable<object> keys)
    {
        if (keys == null)
            return;

        foreach (object o in keys)
        {
            if (o.GetType() == typeof(string))
                Remove(o as string);
        }
    }
}
