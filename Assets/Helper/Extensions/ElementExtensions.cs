using Entity;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static KeyManager;

public static class ElementExtensions
{
    public static bool ToValue<T>(this object input, ref T output, KVStorage replaceData = null, string separator = "{}")
    {
        bool b = false;

        if (input == null)
        {
            output = default(T);
            return false;
        }

        if (typeof(T) == typeof(string))
        {
            b = input.ToString(out string s, replaceData, separator);
            output = (T)(object)s;
        }
        else if (typeof(T) == typeof(int))
        {
            b = input.ToInt(out int i, replaceData, separator);
            output = (T)(object)i;
        }
        else if (typeof(T) == typeof(float) || typeof(T) == typeof(Single))
        {
            b = input.ToFloat(out float f, replaceData, separator);
            output = (T)(object)f;
        }
        else if (typeof(T) == typeof(bool))
        {
            b = input.ToBool(out bool bo, replaceData, separator);
            output = (T)(object)bo;
        }
        else if (typeof(T) == typeof(Vector2))
        {
            b = input.ToVector2(out Vector2 v2, replaceData, separator);
            output = (T)(object)v2;
        }
        else if (typeof(T) == typeof(Vector3))
        {
            b = input.ToVector3(out Vector3 v3, replaceData, separator);
            output = (T)(object)v3;
        }
        else if (typeof(T) == typeof(Vector2Int))
        {
            b = input.ToVector2Int(out Vector2Int v2i, replaceData, separator);
            output = (T)(object)v2i;
        }
        else if (typeof(T) == typeof(Vector3Int))
        {
            b = input.ToVector3Int(out Vector3Int v3, replaceData, separator);
            output = (T)(object)v3;
        }
        else if (typeof(T) == typeof(SpriteData))
        {
            b = input.ToSpriteData(out SpriteData sd, replaceData, separator).boolean;
            output = (T)(object)sd;
        }
        else if (input.GetType().IsAssignableFrom(typeof(T)))
        {
            b = true;
            output = (T)(object)input;
        }
        else if (input.GetType() == typeof(T))
        {
            b = true;
            output = (T)(object)input;
        }

        return b;
    }

    public static ReturnCode ToSpriteData(this object input, out SpriteData output, KVStorage replaceData = null, string separator = "{}")
    {
        output = null;

        if (input is List<object>)
        {
            List<object> l = input as List<object>;
            string texturePath = "";
            string textureName = "";

            if (l.Count == 1 &&
                l[0].getElement(ref texturePath))
            {
                texturePath = texturePath.replaceData(replaceData, separator);

                return SpriteData.createSprite(texturePath, out output);
            }
            else if (l.Count == 2 &&
                l[0].getElement(ref texturePath) &&
                l[1].getElement(ref textureName))
            {
                texturePath = texturePath.replaceData(replaceData, separator);
                textureName = textureName.replaceData(replaceData, separator);

                return SpriteData.createSprite(texturePath, textureName, out output);
            }
            else
                return ReturnCode.Code(102, "Input-Object is invalid", true);
        }
        else if (input is string)
        {
            string path = (string)input;
            path = path.replaceData(replaceData, separator);
            return SpriteData.createSprite(path, out output);
        }

        return ReturnCode.FAILED;
    }

    public static bool ToString(this object input, out string output, KVStorage replaceData = null, string separator = "{}")
    {
        output = "";
        if (input is string)
        {
            output = input as string;
            output = output.replaceData(replaceData, separator);
        }
        else if (input is int)
        {
            output = "" + (int)input;
        }
        else if (input is float || input is Single)
        {
            output = "" + (float)input;
        }
        else if (input is long)
        {
            output = "" + (long)input;
        }
        else if (input is double)
        {
            output = "" + (double)input;
        }
        else if (input is bool)
        {
            output = "" + (bool)input;
        }
        else if (input is Vector2)
        {
            Vector2 v2 = (Vector2)input;
            output = $"{v2.x}:{v2.y}";
        }
        else if (input is Vector3)
        {
            Vector3 v3 = (Vector3)input;
            output = $"{v3.x}:{v3.y}:{v3.z}";
        }
        else if (input is Vector2Int)
        {
            Vector2Int v2i = (Vector2Int)input;
            output = $"{v2i.x}:{v2i.y}";
        }
        else if (input is Vector3Int)
        {
            Vector3Int v3i = (Vector3Int)input;
            output = $"{v3i.x}:{v3i.y}:{v3i.z}";
        }
        else if (input is List<object>)
        {
            List<object> l = input as List<object>;
            if (l.calculateData(out object o, replaceData, separator))
                return o.getElement(ref output, replaceData, separator);
            else
            {
                output = "";
                return false;
            }
        }
        else
        {
            output = "";
            return false;
        }

        return true;
    }
    public static bool ToInt(this object input, out int output, KVStorage replaceData = null, string separator = "{}")
    {
        output = 0;
        if (input is string)
        {
            string text = (string)input;
            text = text.replaceData(replaceData, separator);
            return int.TryParse(text, out output);
        }
        else if (input is int)
        {
            output = (int)input;
        }
        else if (input is float || input is Single)
        {
            output = Mathf.FloorToInt((float)input);
        }
        else if (input is long)
        {
            output = (int)input;
        }
        else if (input is bool)
        {
            output = ((bool)input) ? 1 : 0;
        }
        else if (input is List<object>)
        {
            List<object> l = input as List<object>;
            if (l.calculateData(out object o, replaceData, separator))
                return o.getElement(ref output, replaceData, separator);
            else
            {
                output = 0;
                return false;
            }
        }
        else
        {
            output = 0;
            return false;
        }

        return true;
    }

    public static bool ToFloat(this object input, out float output, KVStorage replaceData = null, string separator = "{}")
    {
        output = 0f;
        if (input is string)
        {
            string text = (string)input;
            text = text.replaceData(replaceData, separator);
            return float.TryParse(text, out output);
        }
        else if (input is int)
        {
            output = (int)input;
        }
        else if (input is float || input is Single)
        {
            output = (float)input;
        }
        else if (input is long)
        {
            output = (long)input;
        }
        else if (input is bool)
        {
            output = ((bool)input) ? 1f : 0f;
        }
        else if (input is List<object>)
        {
            List<object> l = input as List<object>;
            if (l.calculateData(out object o, replaceData, separator))
                return o.getElement(ref output, replaceData, separator);
            else
            {
                output = 0f;
                return false;
            }
        }
        else
        {
            output = 0;
            return false;
        }

        return true;
    }

    public static bool ToBool(this object input, out bool output, KVStorage replaceData = null, string separator = "{}")
    {
        output = false;
        if (input is string)
        {
            string text = (string)input;
            text = text.replaceData(replaceData, separator);
            return bool.TryParse(text, out output);
        }
        else if (input is int)
        {
            output = ((int)input == 1);
        }
        else if (input is bool)
        {
            output = ((bool)input);
        }
        else if (input is List<object>)
        {
            List<object> l = input as List<object>;
            if (l.calculateData(out object o, replaceData, separator))
                return o.ToValue(ref output, replaceData, separator);
            else
            {
                output = false;
                return false;
            }
        }
        else
        {
            output = false;
            return false;
        }

        return true;
    }

    public static bool ToVector2(this object input, out Vector2 output, KVStorage replaceData = null, string separator = "{}")
    {
        output = Vector2.zero;
        if (input is Vector2)
        {
            output = (Vector2)input;
        }
        else if (input is Vector3)
        {
            output = ((Vector3)input).toVec2();
        }
        else if (input is Vector2Int)
        {
            output = ((Vector2Int)input).toVec2();
        }
        else if (input is Vector3Int)
        {
            output = ((Vector3Int)input).toVec2();
        }
        else if (input is string)
        {
            string text = (string)input;
            text = text.replaceData(replaceData, separator);
            string[] s = text.Split(":");
            if (s.Length >= 2 && s[0].ToFloat(out float f1) && s[1].ToFloat(out float f2))
            {
                output = new Vector2(f1, f2);
            }
            else
                return false;
        }
        else if (input is List<object>)
        {
            List<object> l = input as List<object>;
            if (l.calculateData(out object o, replaceData, separator))
                return o.ToValue(ref output, replaceData, separator);
            else
                return false;
        }
        else
            return false;

        return true;
    }

    public static bool ToVector3(this object input, out Vector3 output, KVStorage replaceData = null, string separator = "{}")
    {
        output = Vector3.zero;
        if (input is Vector2)
        {
            output = ((Vector2)input).toVec3();
        }
        else if (input is Vector3)
        {
            output = (Vector3)input;
        }
        else if (input is Vector2Int)
        {
            output = ((Vector2Int)input).toVec3();
        }
        else if (input is Vector3Int)
        {
            output = ((Vector3Int)input).toVec3();
        }
        else if (input is string)
        {
            string text = (string)input;
            text = text.replaceData(replaceData, separator);
            string[] s = text.Split(":");
            if (s.Length >= 3
                && s[0].ToFloat(out float f1)
                && s[1].ToFloat(out float f2)
                && s[2].ToFloat(out float f3))
            {
                output = new Vector3(f1, f2, f3);
            }
            else
                return false;
        }
        else if (input is List<object>)
        {
            List<object> l = input as List<object>;
            if (l.calculateData(out object o, replaceData, separator))
                return o.ToValue(ref output, replaceData, separator);
            else
                return false;
        }
        else
            return false;

        return true;
    }

    public static bool ToVector2Int(this object input, out Vector2Int output, KVStorage replaceData = null, string separator = "{}")
    {
        output = Vector2Int.zero;
        if (input is Vector2)
        {
            output = ((Vector2)input).toVec2Int();
        }
        else if (input is Vector3)
        {
            output = ((Vector3)input).toVec2Int();
        }
        else if (input is Vector2Int)
        {
            output = (Vector2Int)input;
        }
        else if (input is Vector3Int)
        {
            output = ((Vector3Int)input).toVec2Int();
        }
        else if (input is string)
        {
            string text = (string)input;
            text = text.replaceData(replaceData, separator);
            string[] s = text.Split(":");
            if (s.Length >= 2 && s[0].ToInt(out int i1) && s[1].ToInt(out int i2))
            {
                output = new Vector2Int(i1, i2);
            }
            else
                return false;
        }
        else if (input is List<object>)
        {
            List<object> l = input as List<object>;
            if (l.calculateData(out object o, replaceData, separator))
                return o.ToValue(ref output, replaceData, separator);
            else
                return false;
        }
        else
            return false;

        return true;
    }

    public static bool ToVector3Int(
        this object input,
        out Vector3Int output,
        KVStorage replaceData = null,
        string separator = "{}")
    {
        output = Vector3Int.zero;
        if (input is Vector2)
        {
            output = ((Vector2)input).toVec3Int();
        }
        else if (input is Vector3)
        {
            output = ((Vector3)input).toVec3Int();
        }
        else if (input is Vector2Int)
        {
            output = ((Vector2Int)input).toVec3Int();
        }
        else if (input is Vector3Int)
        {
            output = (Vector3Int)input;
        }
        else if (input is string)
        {
            string text = (string)input;
            text = text.replaceData(replaceData, separator);
            string[] s = text.Split(":");
            if (s.Length >= 3
                && s[0].ToInt(out int i1)
                && s[1].ToInt(out int i2)
                && s[2].ToInt(out int i3))
            {
                output = new Vector3Int(i1, i2, i3);
            }
            else
                return false;
        }
        else if (input is List<object>)
        {
            List<object> l = input as List<object>;
            if (l.calculateData(out object o, replaceData, separator))
                return o.ToValue(ref output, replaceData, separator);
            else
                return false;
        }
        else
            return false;

        return true;
    }

    public static bool getElement<T, U>(this InteractionData<U> input, string key, ref T output, KVStorage replaceData = null, string separator = "{}") where U : IIDBase
    {
        return input.data.getElement(key, ref output, replaceData, separator);
    }

    public static bool getElement<T>(this Entity.Core input, string key, ref T output, KVStorage replaceData = null, string separator = "{}")
    {
        return input.Info().getElement(key, ref output, replaceData, separator);
    }

    public static bool getElement<T>(this Entity.Info.Core input, string key, ref T output, KVStorage replaceData = null, string separator = "{}")
    {
        return input.data.getElement(key, ref output, replaceData, separator);
    }

    public static bool getElement<T, U>(this KeyValuePair<string, U> input, ref T output, KVStorage replaceData = null, string separator = "{}")
    {
        return input.Value.getElement(ref output, replaceData, separator);
    }

    public static bool getElement<T>(this Vector2 input, string key, ref T output, KVStorage replaceData = null, string separator = "{}")
    {
        if (input == null)
            return false;

        output = default(T);

        switch(key)
        {
            case "x":
                return input.x.ToValue(ref output, replaceData, separator);
            case "y":
                return input.y.ToValue(ref output, replaceData, separator);
            default:
                return false;
        }
    }

    public static bool getElement<T>(this Vector3 input, string key, ref T output, KVStorage replaceData = null, string separator = "{}")
    {
        if (input == null)
            return false;

        output = default(T);

        switch (key)
        {
            case "x":
                return input.x.ToValue(ref output, replaceData, separator);
            case "y":
                return input.y.ToValue(ref output, replaceData, separator);
            case "z":
                return input.z.ToValue(ref output, replaceData, separator);
            default:
                return false;
        }
    }

    public static bool getElement<T>(this Vector2Int input, string key, ref T output, KVStorage replaceData = null, string separator = "{}")
    {
        if (input == null)
            return false;

        output = default(T);

        switch (key)
        {
            case "x":
                return input.x.ToValue(ref output, replaceData, separator);
            case "y":
                return input.y.ToValue(ref output, replaceData, separator);
            default:
                return false;
        }
    }

    public static bool getElement<T>(this Vector3Int input, string key, ref T output, KVStorage replaceData = null, string separator = "{}")
    {
        if (input == null)
            return false;

        output = default(T);

        switch (key)
        {
            case "x":
                return input.x.ToValue(ref output, replaceData, separator);
            case "y":
                return input.y.ToValue(ref output, replaceData, separator);
            case "z":
                return input.z.ToValue(ref output, replaceData, separator);
            default:
                return false;
        }
    }

    public static bool getElement<T>(this KVStorage input, string key, ref T output, KVStorage replaceData = null, string separator = "{}")
    {
        if (input == null)
            return false;

        if (key.Contains('.'))
        {
            string[] keys = key.Split('.', 2);

            if (!input.ContainsKey(keys[0]))
                return false;

            if (input[keys[0]] is KVStorage)
                return (input[keys[0]] as KVStorage).getElement(keys[1], ref output, replaceData, separator);
            else if (input[keys[0]] is List<object>)
                return (input[keys[0]] as List<object>).getElement(keys[1], ref output, replaceData, separator);
            else if (input[keys[0]] is Vector2)
                return ((Vector2)input[keys[0]]).getElement(keys[1], ref output, replaceData, separator);
            else if (input[keys[0]] is Vector3)
                return ((Vector3)input[keys[0]]).getElement(keys[1], ref output, replaceData, separator);
            else if (input[keys[0]] is Vector2Int)
                return ((Vector2Int)input[keys[0]]).getElement(keys[1], ref output, replaceData, separator);
            else if (input[keys[0]] is Vector3Int)
                return ((Vector3Int)input[keys[0]]).getElement(keys[1], ref output, replaceData, separator);
            else
                return input[keys[0]].getElement(ref output, replaceData, separator);   
        }

        if (!input.ContainsKey(key))
            return false;

        return input[key].getElement(ref output, replaceData, separator);
    }

    public static bool getElement<T, U>(this List<T> input, int index, ref U output, KVStorage replaceData = null, string separator = "{}")
    {
        if (input == null)
            return false;

        if (input.Count <= index)
            return false;

        return input[index].getElement(ref output, replaceData, separator);
    }

    public static bool getElement<T, U>(this List<T> input, string key, ref U output, KVStorage replaceData = null, string separator = "{}")
    {
        if (input == null)
            return false;

        int index = 0;

        if (key.Contains('.'))
        {
            string[] keys = key.Split('.', 2);

            if (!keys[0].ToInt(out index))
                return false;

            if (index < 0 || index >= input.Count)
                return false;

            if (input[index] is KVStorage)
                return (input[index] as KVStorage).getElement(keys[1], ref output, replaceData, separator);
            else if (input[index] is List<object>)
                return (input[index] as List<object>).getElement(keys[1], ref output, replaceData, separator);
            else if (input[index] is Vector2)
                return ((Vector2)(object)input[index]).getElement(keys[1], ref output, replaceData, separator);
            else if (input[index] is Vector3)
                return ((Vector3)(object)input[index]).getElement(keys[1], ref output, replaceData, separator);
            else if (input[index] is Vector2Int)
                return ((Vector2Int)(object)input[index]).getElement(keys[1], ref output, replaceData, separator);
            else if (input[index] is Vector3Int)
                return ((Vector3Int)(object)input[index]).getElement(keys[1], ref output, replaceData, separator);
            else
                return input[index].getElement(ref output, replaceData, separator);
        }

        if (!key.ToInt(out index))
            return false;

        if (index < 0 || index >= input.Count)
            return false;

        return input[index].getElement(ref output, replaceData, separator);
    }

    public static bool getElement<T, U>(this T input, ref U output, KVStorage replaceData = null, string seperator = "{}")
    {
        return input.ToValue(ref output, replaceData, seperator);
    }

    public static bool getEnum<T, U>(this InteractionData<U> input, string key, ref T output, KVStorage replaceData = null, string separator = "{}") where T : struct, Enum where U : IIDBase
    {
        return input.data.getEnum(key, ref output, replaceData, separator);
    }

    public static bool getEnum<T>(this Entity.Info.Core input, string key, ref T output, KVStorage replaceData = null, string separator = "{}") where T : struct, Enum
    {
        return input.data.getEnum(key, ref output, replaceData, separator);
    }

    public static bool getEnum<T, U>(this KeyValuePair<string, U> input, ref T output, KVStorage replaceData = null, string separator = "{}") where T : struct, Enum
    {
        return input.Value.getEnum(ref output, replaceData, separator);
    }


    public static bool getEnum<T, U>(this List<U> input, int index, ref T output, KVStorage replaceData = null, string separator = "{}") where T : struct, Enum
    {
        output = default(T);

        if (input.Count <= index)
            return false;

        return input[index].getEnum(ref output, replaceData, separator);
    }

    public static bool getEnum<T, U>(this List<U> input, string key, ref T output, KVStorage replaceData = null, string separator = "{}") where T : struct, Enum
    {
        if (input == null)
            return false;

        output = default(T);

        int index = 0;

        if (key.Contains('.'))
        {
            string[] keys = key.Split('.', 2);

            if (!keys[0].ToInt(out index))
                return false;

            if (index < 0 || index >= input.Count)
                return false;

            if (input[index] is KVStorage)
                return (input[index] as KVStorage).getEnum(keys[1], ref output, replaceData, separator);
            else if (input[index] is List<object>)
                return (input[index] as List<object>).getEnum(keys[1], ref output, replaceData, separator);
            else
                return input[index].getEnum(ref output, replaceData, separator);
        }

        if (!key.ToInt(out index))
            return false;

        if (index < 0 || index >= input.Count)
            return false;

        return input[index].getEnum(ref output, replaceData, separator);
    }

    public static bool getEnum<T>(this KVStorage input, string key, ref T output, KVStorage replaceData = null, string separator = "{}") where T : struct, Enum
    {
        if (input == null)
            return false;

        if (key.Contains('.'))
        {
            string[] keys = key.Split('.', 2);

            if (!input.ContainsKey(keys[0]))
                return false;

            if (input[keys[0]] is KVStorage)
                return (input[keys[0]] as KVStorage).getEnum(keys[1], ref output, replaceData, separator);
            else if (input[keys[0]] is List<object>)
                return (input[keys[0]] as List<object>).getEnum(keys[1], ref output, replaceData, separator);
            else
                return input[keys[0]].getEnum(ref output, replaceData, separator);
        }

        if (!input.ContainsKey(key))
            return false;

        return input[key].getEnum(ref output, replaceData, separator);
    }

    public static bool getEnum<T>(this object input, ref T output, KVStorage replaceData = null, string separator = "{}") where T : struct, Enum
    {
        if (input == null)
            return false;

        if (input is T)
        {
            output = (T)input;
            return true;
        }
        else if (input is string)
        {
            return Enum.TryParse(input as string, out output);
        }
        else if (input is int)
        {
            if (!Enum.IsDefined(typeof(T), (int)input))
                return false;
            else
            {
                output = (T)Enum.ToObject(typeof(T), (int)input);
            }
        }
        return false;
    }

    public static bool checkElement<T>(this KVStorage input, string key, T compare, KVStorage replaceData = null, string separator = "{}")
    {
        if (input == null)
            return false;

        T data = default(T);
        if (!input.getElement(key, ref data, replaceData, separator))
            return false;

        return data.Equals(compare);
    }

    public static bool checkElement<T>(this List<object> input, string key, T compare, KVStorage replaceData = null, string separator = "{}")
    {
        if (input == null)
            return false;

        T data = default(T);
        if (!input.getElement(key, ref data, replaceData, separator))
            return false;

        return data.Equals(compare);
    }

    public static bool checkElement<T>(this List<object> input, int index, T compare, KVStorage replaceData = null, string separator = "{}")
    {
        if (input == null)
            return false;

        T data = default(T);
        if (!input.getElement(index, ref data, replaceData, separator))
            return false;

        return data.Equals(compare);
    }
}