using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JsonScriptCalculateExtensions
{
    #region calculate Data

    public static bool calculateData(this object data, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = data;

        List<object> listl = null;
        if (data.getElement(ref listl, replaceData, separator))
        {
            List<object> list = new List<object>();
            foreach (object o in listl)
            {
                if (o.calculateData(out object lOutput, replaceData, separator))
                    list.Add(lOutput);
                else
                    list.Add(o);
            }

            string key = "";
            if (list.getElement(0, ref key, replaceData, separator))
            {
                if (key == "RANDOM_LIST")
                    return getRandomFromList(list, out output, replaceData, separator);
                else if (key == "RANDOM_INT")
                    return getRandomInt(list, out output, replaceData, separator);
                else if (key == "RANDOM_FLOAT")
                    return getRandomFloat(list, out output, replaceData, separator);
                else if (key == "EQUALS" || key == "==")
                    return getEqual(list, out output, replaceData, separator);
                else if (key == "LARGER_THAN" || key == ">")
                    return getLarger(list, out output, replaceData, separator);
                else if (key == "SMALLER_THAN" || key == "<")
                    return getSmaller(list, out output, replaceData, separator);
                else if (key == "LARGER_EQUAL" || key == ">=")
                    return getLargerEqual(list, out output, replaceData, separator);
                else if (key == "SMALLER_EQUAL" || key == "<=")
                    return getSmallerEqual(list, out output, replaceData, separator);
                else if (key == "ADD" || key == "+")
                    return getAdd(list, out output, replaceData, separator);
                else if (key == "SUBTRACT" || key == "-")
                    return getSubtract(list, out output, replaceData, separator);
                else if (key == "MULTIPLY" || key == "*")
                    return getMultiply(list, out output, replaceData, separator);
                else if (key == "DIVIDE" || key == "/")
                    return getDivide(list, out output, replaceData, separator);
                else if (key == "SQRT")
                    return getSqrt(list, out output, replaceData, separator);
                else if (key == "PWR")
                    return getPwr(list, out output, replaceData, separator);
                else if (key == "ROUND")
                    return getRoundValue(list, out output, replaceData, separator);
                else if (key == "ROUND_UP")
                    return getRoundValueUp(list, out output, replaceData, separator);
                else if (key == "ROUND_DOWN")
                    return getRoundValueDown(list, out output, replaceData, separator);
                else if (key == "STANDARTIZE")
                    return getStandartized(list, out output, replaceData, separator);
                else if (key == "APPEND")
                    return getAppendString(list, out output, replaceData, separator);
                else
                {
                    output = list;
                    return true;
                }
            }
            else
            {
                output = list;
                return true;
            }
        }

        if (data is KVStorage)
        {
            KVStorage kv = (KVStorage)data;
            List<string> keys = new List<string>(kv.Keys);
            foreach (string key in keys)
            {
                kv[key].calculateData(out object dictOutput, replaceData, separator);
                kv[key] = dictOutput;
            }
            output = kv;
            return true;
        }

        int i = 0;
        if (data.getElement(ref i, replaceData, separator))
        {
            output = i;
            return true;
        }
        float f = 0f;
        if (data.getElement(ref f, replaceData, separator))
        {
            output = f;
            return true;
        }
        double d = 0.0;
        if (data.getElement(ref d, replaceData, separator))
        {
            output = d;
            return true;
        }
        long l = 0L;
        if (data.getElement(ref l, replaceData, separator))
        {
            output = l;
            return true;
        }
        bool b = false;
        if (data.getElement(ref b, replaceData, separator))
        {
            output = b;
            return true;
        }
        Vector2 v2 = Vector2.zero;
        if (data.getElement(ref v2, replaceData, separator))
        {
            output = v2;
            return true;
        }
        Vector2Int v2i = Vector2Int.zero;
        if (data.getElement(ref v2i, replaceData, separator))
        {
            output = v2i;
            return true;
        }
        Vector3 v3 = Vector3.zero;
        if (data.getElement(ref v3, replaceData, separator))
        {
            output = v3;
            return true;
        }
        Vector3Int v3i = Vector3Int.zero;
        if (data.getElement(ref v3i, replaceData, separator))
        {
            output = v3i;
            return true;
        }
        string s = "";
        if (data.getElement(ref s, replaceData, separator))
        {
            output = s;
            return true;
        }

        return true;
    }

    public static bool getAppendString(this List<object> list, out object output, KVStorage replaceData, string separator = "{}")
    {
        output = list;

        if (list.Count != 3 && list.Count != 4)
            return false;

        string s1 = "";
        int i = 0;
        string s2 = " ";
        if (!list[1].getElement(ref s1, replaceData, separator) ||
            !list[2].getElement(ref i, replaceData, separator))
            return false;

        if (list.Count == 4)
            list[3].getElement(ref s2, replaceData, separator);

        while (s1.Length < i)
            s1 = s2 + s1;

        output = s1;

        return true;
    }

    /// <summary>
    /// ["RANDOM_LIST", "<element1>", "<element2>", "..."]
    /// an element from the list except the first one, which acts as the key
    /// </summary>
    public static bool getRandomFromList(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count < 2)
            return false;

        int pos = UnityEngine.Random.Range(1, list.Count);
        output = list[pos];

        return output.calculateData(out output, replaceData, separator);
    }

    /// <summary>
    /// ["RANDOM_INT", "<num1>", "<num2>"]
    /// a random Int-Value from <num1>(inclusive) to <num2>(exclusive)
    /// </summary>
    public static bool getRandomInt(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 3)
            return false;

        int i1 = 0;
        int i2 = 0;

        if (!list[1].getElement(ref i1, replaceData, separator) ||
            !list[2].getElement(ref i2, replaceData, separator))
            return false;

        output = UnityEngine.Random.Range(i1, i2);

        return true;
    }

    /// <summary>
    /// ["RANDOM_INT", "<num1>", "<num2>"]
    /// a random Float-Value from <num1>(inclusive) to <num2>(inclusive)
    /// </summary>
    public static bool getRandomFloat(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 3)
            return false;

        float f1 = 0;
        float f2 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) ||
            !list[2].getElement(ref f2, replaceData, separator))
            return false;

        output = UnityEngine.Random.Range(f1, f2);

        return true;
    }

    /// <summary>
    /// ["EQUALS || ==", "<left>", "<right>", "<true>", "<false>"]
    /// checks if <left> equals <right> and returns <true> if equal and <false> if not
    /// </summary>
    public static bool getEqual(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 5)
            return false;

        if (list[1].Equals(list[2]))
            list[3].calculateData(out output, replaceData, separator);
        else
            list[4].calculateData(out output, replaceData, separator);

        return true;
    }

    /// <summary>
    /// ["LARGER_THAN || >", "<left>", "<right>", "<true>", "<false>"]
    /// checks if <left> is larger than <right> and returns <true> if its larger and <false> if not
    /// </summary>
    public static bool getLarger(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 5)
            return false;

        float f1 = 0;
        float f2 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) || 
            !list[2].getElement(ref f2, replaceData, separator))
            return false;

        if (f1 > f2)
            list[3].calculateData(out output, replaceData, separator);
        else
            list[4].calculateData(out output, replaceData, separator);

        return true;
    }

    /// <summary>
    /// ["SMALLER_THAN || <", "<left>", "<right>", "<true>", "<false>"]
    /// checks if <left> is smaller than <right> and returns <true> if its smaller and <false> if not
    /// </summary>
    public static bool getSmaller(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 5)
            return false;

        float f1 = 0;
        float f2 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) || 
            !list[2].getElement(ref f2, replaceData, separator))
            return false;

        if (f1 < f2)
            list[3].calculateData(out output, replaceData, separator);
        else
            list[4].calculateData(out output, replaceData, separator);

        return true;
    }

    /// <summary>
    /// ["LARGER_EQUAL || >=", "<left>", "<right>", "<true>", "<false>"]
    /// checks if <left> is larger or equal to <right> and returns <true> if its larger or equal and <false> if not
    /// </summary>
    public static bool getLargerEqual(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 5)
            return false;

        float f1 = 0;
        float f2 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) || 
            !list[2].getElement(ref f2, replaceData, separator))
            return false;

        if (f1 >= f2)
            list[3].calculateData(out output, replaceData, separator);
        else
            list[4].calculateData(out output, replaceData, separator);

        return true;
    }

    /// <summary>
    /// ["SMALLER_EQUAL || <=", "<left>", "<right>", "<true>", "<false>"]
    /// checks if <left> is smaller or equal to <right> and returns <true> if its smaller or equal and <false> if not
    /// </summary>
    public static bool getSmallerEqual(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 5)
            return false;

        float f1 = 0;
        float f2 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) || 
            !list[2].getElement(ref f2, replaceData, separator))
            return false;

        if (f1 <= f2)
            list[3].calculateData(out output, replaceData, separator);
        else
            list[4].calculateData(out output, replaceData, separator);

        return true;
    }

    /// <summary>
    /// ["ADD || +", "<num1>", "<num2>"]
    /// adds <num1> and <num2> together
    /// </summary>
    public static bool getAdd(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 3)
            return false;

        float f1 = 0;
        float f2 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) || 
            !list[2].getElement(ref f2, replaceData, separator))
            return false;

        output = "" + (f1 + f2);

        return true;
    }

    /// <summary>
    /// ["SUBTRACT || -", "<num1>", "<num2>"]
    /// subtracts <num2> from <num1>
    /// </summary>
    public static bool getSubtract(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 3)
            return false;

        float f1 = 0;
        float f2 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) || 
            !list[2].getElement(ref f2, replaceData, separator))
            return false;

        output = "" + (f1 - f2);

        return true;
    }

    /// <summary>
    /// ["MULTIPLY || *", "<num1>", "<num2>"]
    /// multiplies <num1> by <num2>
    /// </summary>
    public static bool getMultiply(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 3)
            return false;

        float f1 = 0;
        float f2 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) || 
            !list[2].getElement(ref f2, replaceData, separator))
            return false;

        output = "" + (f1 * f2);

        return true;
    }

    /// <summary>
    /// ["DIVIDE || /", "<num1>", "<num2>"]
    /// divides <num1> by <num2>
    /// </summary>
    public static bool getDivide(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 3)
            return false;

        float f1 = 0;
        float f2 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) || 
            !list[2].getElement(ref f2, replaceData, separator))
            return false;

        output = "" + (f1 / f2);

        return true;
    }

    /// <summary>
    /// ["SQRT", "<num1>", "[num2]"]
    /// calculates the [num2]th sqrt of <num1>; if [num2] is not given 2 is used
    /// </summary>
    public static bool getSqrt(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;


        if (list.Count == 2 || list.Count == 3)
        {
            float f1 = 0;
            if (!list[1].getElement(ref f1, replaceData, separator))
                return false;

            if (list.Count == 3)
            {
                float f2 = 0;
                if (!list[2].getElement(ref f2, replaceData, separator))
                    return false;

                output = "" + Mathf.Pow(f1, (1 / f2));
            }
            else
                output = "" + Mathf.Sqrt(f1);

            return true;
        }

        return false;
    }

    /// <summary>
    /// ["PWR", "<num1>", "[num2]"]
    /// calculates the [num2]th power of <num1>; if [num2] is not given 2 is used
    /// </summary>
    public static bool getPwr(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;


        if (list.Count == 2 || list.Count == 3)
        {
            float f1 = 0;
            if (!list[1].getElement(ref f1, replaceData, separator))
                return false;

            if (list.Count == 3)
            {
                float f2 = 0;
                if (!list[2].getElement(ref f2, replaceData, separator))
                    return false;

                output = "" + Mathf.Pow(f1, f2);
            }
            else
                output = "" + Mathf.Pow(f1, 2);

            return true;
        }

        return false;
    }

    /// <summary>
    /// ["ROUND", "<num1>", "<num2>"]
    /// rounds <num1> to <num2> decimal places
    /// </summary>
    public static bool getRoundValue(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 3)
            return false;

        float f1 = 0;
        int i1 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) ||
            !list[2].getElement(ref i1, replaceData, separator))
            return false;

        output = (float)Math.Round(f1, i1);

        return true;
    }

    /// <summary>
    /// ["ROUND_UP", "<num1>", "<num2>"]
    /// rounds <num1> up to <num2> decimal places
    /// </summary>
    public static bool getRoundValueUp(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 3)
            return false;

        float f1 = 0;
        int i1 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) ||
            !list[2].getElement(ref i1, replaceData, separator))
            return false;

        output = (float)Math.Round(f1, i1, MidpointRounding.AwayFromZero);

        return true;
    }

    /// <summary>
    /// ["ROUND_DOWN", "<num1>", "<num2>"]
    /// rounds <num1> down to <num2> decimal places
    /// </summary>
    public static bool getRoundValueDown(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 3)
            return false;

        float f1 = 0;
        int i1 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) ||
            !list[2].getElement(ref i1, replaceData, separator))
            return false;

        output = (float)Math.Round(f1, i1, MidpointRounding.ToEven);

        return true;
    }

    /// <summary>
    /// ["STANDARTIZE", "<num1>", "<num2>"]
    /// rounds <num1> to the nearest <num2> divider
    /// </summary>
    public static bool getStandartized(this List<object> list, out object output, KVStorage replaceData = null, string separator = "{}")
    {
        output = list;

        if (list.Count != 3)
            return false;

        float f1 = 0;
        float f2 = 0;

        if (!list[1].getElement(ref f1, replaceData, separator) ||
            !list[2].getElement(ref f2, replaceData, separator))
            return false;

        output = (float)Math.Round(f1 / f2) * f2;

        return true;
    }
    #endregion
}
