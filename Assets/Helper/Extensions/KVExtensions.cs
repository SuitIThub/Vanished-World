using System.Collections.Generic;

public static class KVExtensions
{
    public static KVStorage overwriteData(this KVStorage data, KVStorage overwriteData, List<string> protectedList = null)
    {
        if (protectedList == null)
            protectedList = new List<string>();

        KVStorage replace = null;
        if (!data.getElement("replaceData", ref replace))
        {
            replace = new KVStorage();
            data.Add("replaceData", replace);
        }
        KVStorage overwrite = null;
        if (!data.getElement("overwriteData", ref overwrite))
        {
            overwrite = new KVStorage();
            data.Add("overwriteData", overwrite);
        }

        foreach (string key in overwriteData.Keys)
        {
            if (protectedList.Contains(key))
                continue;

            replace.Set(key, overwriteData[key]);
            overwrite.Set(key, overwriteData[key]);
        }

        return data;
    }

    public static KVStorage replaceData(this KVStorage data, KVStorage replaceData, string separator = "{}")
    {
        KVStorage output = new KVStorage();

        foreach(KeyValuePair<string, object> kvp in data)
        {
            KeyValuePair<string, object> k = kvp.replaceData(replaceData, separator);
            output.Add(k.Key, k.Value);
        }

        data = output;
        return data;
    }

    public static KeyValuePair<string, object> replaceData(this KeyValuePair<string, object> data, KVStorage replaceData, string separator = "{}")
    {
        object output = data.Value;
        object o = data.Value;

        if (o == null)
            return data;

        if (o.GetType() == typeof(string))
            output = (o as string).replaceData(replaceData, separator);
        else if (o.GetType() == typeof(List<object>))
            output = (o as List<object>).replaceData(replaceData, separator);
        else if (o.GetType() == typeof(object[]))
            output = (o as object[]).replaceData(replaceData, separator);
        else if (o.GetType() == typeof(KVStorage))
            output = (o as KVStorage).replaceData(replaceData, separator);
        else if (o.GetType() == typeof(KeyValuePair<string, object>))
            output = ((KeyValuePair<string, object>)o).replaceData(replaceData, separator);

        return new KeyValuePair<string, object>(data.Key, output);
    }
}
