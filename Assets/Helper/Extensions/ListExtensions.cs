using System.Linq;
using System.Collections.Generic;

public static class ListExtensions
{
    #region replaceData

    public static List<object> replaceData(this List<object> list, KVStorage replaceData, string separator = "{}")
    {
        List<object> output = new List<object>();
        foreach(object o in list)
        {
            if (o.GetType() == typeof(string))
                output.Add((o as string).replaceData(replaceData, separator));
            else if (o.GetType() == typeof(List<object>))
                output.Add((o as List<object>).replaceData(replaceData, separator));
            else if (o.GetType() == typeof(object[]))
                output.Add((o as object[]).replaceData(replaceData, separator));
            else if (o.GetType() == typeof(KVStorage))
                output.Add((o as KVStorage).replaceData(replaceData, separator));
            else if (o.GetType() == typeof(KeyValuePair<string, object>))
                output.Add(((KeyValuePair<string, object>)o).replaceData(replaceData, separator));
            else
                output.Add(o);
        }

        list = output;

        return list;
    }

    public static object[] replaceData(this object[] arr, KVStorage replaceData, string separator = "{}")
    {
        object[] output = new object[arr.Length];
        int i = -1;
        foreach (object o in arr)
        {
            i++;

            if (o.GetType() == typeof(string))
                output[i] = (o as string).replaceData(replaceData, separator);
            else if (o.GetType() == typeof(List<object>))
                output[i] = (o as List<object>).replaceData(replaceData, separator);
            else if (o.GetType() == typeof(object[]))
                output[i] = (o as object[]).replaceData(replaceData, separator);
            else if (o.GetType() == typeof(KVStorage))
                output[i] = (o as KVStorage).replaceData(replaceData, separator);
            else if (o.GetType() == typeof(KeyValuePair<string, object>))
                output[i] = ((KeyValuePair<string, object>)o).replaceData(replaceData, separator);
        }

        arr = output;

        return arr;
    }

    #endregion

    public static (List<ElementDisplay<T>.Element>, List<ElementDisplay<T>.Element>, List<ElementDisplay<T>.Element>) getCrossSection<T>(
        this IEnumerable<ElementDisplay<T>.Element> oldList, 
        IEnumerable<ElementDisplay<T>.Element> newList) where T : class
    {
        List<ElementDisplay<T>.Element> existingList = new List<ElementDisplay<T>.Element>();
        List<ElementDisplay<T>.Element> missingList = oldList.ToList();
        List<ElementDisplay<T>.Element> addedList = new List<ElementDisplay<T>.Element>();

        foreach(ElementDisplay<T>.Element element in newList)
        {
            IEnumerable<ElementDisplay<T>.Element> e = missingList.Where(x => element.id == x.id);

            if (e.Count() > 0)
            {
                ElementDisplay<T>.Element el = e.First();
                missingList.Remove(el);
                existingList.Add(el);
            }
            else
                addedList.Add(element);
        }

        return (existingList, missingList, addedList);
    }

    public static void AddElem(this List<object> list, string key, object value, bool createMissing = false)
    {
        if (key.Contains('.'))
        {
            string[] keys = key.Split('.', 2);

            if (int.TryParse(keys[0], out int pos))
            {
                while (list.Count < pos - 1)
                    list.Add(null);

                if (list[pos] is KVStorage)
                    (list[pos] as KVStorage).Add(keys[1], value);
                else if (list[pos] is List<object>)
                    (list[pos] as List<object>).AddElem(keys[1], value);
            }
            else
                return;


            return;
        }

        if (int.TryParse(key, out int pos3))
        {
            while (list.Count < pos3)
                list.Add(null);
            list[pos3] = value;
        };
    }

    public static void AddElem(this List<object> list, KeyValuePair<string, object> kvp)
    {
        list.AddElem(kvp.Key, kvp.Value);
    }

    public static void AddElem(this List<object> list, List<object> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (list.Count < i - 1)
                list.Add(data[i]);
        }
    }

    public static void Set(this List<object> list, string key, object value, bool createMissing = false)
    {
        if (key.Contains('.'))
        {
            string[] keys = key.Split('.', 2);

            if (int.TryParse(keys[0], out int pos))
            {
                while (list.Count < pos - 1)
                    list.Add(null);

                if (list[pos] is KVStorage)
                    (list[pos] as KVStorage).Set(keys[1], value);
                else if (list[pos] is List<object>)
                    (list[pos] as List<object>).Set(keys[1], value);
            }
            else
                return;


            return;
        }

        if (int.TryParse(key, out int pos3))
        {
            while (list.Count < pos3)
                list.Add(null);
            list[pos3] = value;
        };
    }

    public static void Set(this List<object> list, KeyValuePair<string, object> kvp)
    {
        list.Set(kvp.Key, kvp.Value);
    }

    public static void Set(this List<object> list, List<object> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (list.Count < i - 1)
                list.Add(data[i]);
            else
                list[i] = data[i];
        }
    }
}
