using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static bool getValue<T>(this Dictionary<string, T> dict, string key, out T module)
    {
        module = default;

        if (!dict.ContainsKey(key))
            return false;
        module = dict[key];
        return true;
    }

    public static void Set<T, U>(this Dictionary<T, U> dict, T key, U value)
    {
        if (!dict.ContainsKey(key))
            dict.Add(key, value);
    }
}
