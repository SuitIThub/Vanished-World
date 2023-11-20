using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class ReturnCode
{
    public enum codeEnum
    {
        UNDEFINED_CODE = -2,
        FAILED = -1,
        SUCCESS = 0,
        //Values
        MISSING_VALUE = 101,
        INVALID_VALUE = 102,
        //GameObject
        GAMEOBJECT_ALREADY_EXISTS = 201,
        //Entity
        ENTITY_MODULE_MISSING = 301,
        //Item
        ITEM_NO_MERGE = 401,
        ITEM_PARTIAL_MERGE = 402,
        ITEM_NO_ADD = 403,
        ITEM_NO_DROP = 404,
        ITEM_FULL_MERGE = 405,
        ITEM_REMOVED = 406,
        //ElementDisplay
        ELEMENT_SELECTED = 501,
        ELEMENT_MARKED = 502,
        ELEMENT_DESELECTED = 503,
    }

    private static int[] overrideTrue = new int[] { 405, 406 };

    private static ReturnCode _SUCCESS = new ReturnCode(0);
    private static ReturnCode _FAILED = new ReturnCode(-1, "Unknown ERROR");
    public static ReturnCode SUCCESS { get => _SUCCESS; }
    public static ReturnCode FAILED 
    {
        get
        {
            _FAILED.printMessage();
            return _FAILED;
        } 
    }

    public static Dictionary<codeEnum, ReturnCode> returnCodeDefaults = null;

    public codeEnum code { get; private set; }
    public int codeInt => (int)code;
    public string message { get; private set; }

    public bool boolean => ((int)code >= 0 && (int)code <= 99) || overrideTrue.Contains(codeInt);

    private ReturnCode(int code, string message = "", bool print = false)
    {
        this.code = (codeEnum)code;
        this.message = message;
        if (print)
            printMessage();
    }

    /// <summary>
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    private ReturnCode(codeEnum code, string message = "", bool print = false)
    {
        this.code = code;
        this.message = message;
        if (print)
            printMessage();
    }

    public static void initialize()
    {
        returnCodeDefaults = new Dictionary<codeEnum, ReturnCode>();
        foreach(codeEnum enums in Enum.GetValues(typeof(codeEnum)))
        {
            returnCodeDefaults.Add(enums, new ReturnCode(enums));
        }
    }

    public static ReturnCode Code(codeEnum code, string message = "", bool print = false)
    {
        if (message != "" || print != false)
            return new ReturnCode(code, message, print);

        if (code == codeEnum.SUCCESS)
            return SUCCESS;

        if (code == codeEnum.FAILED)
            return FAILED;

        return returnCodeDefaults[code];
    }

    public static ReturnCode Code(int i, string message = "", bool print = false)
    {
        if (!Enum.IsDefined(typeof(codeEnum), i))
            return FAILED;

        if (message != "" || print != false)
            return new ReturnCode(i, message, print);

        if (i == 0)
            return SUCCESS;

        if (i == -1)
            return FAILED;

        return returnCodeDefaults[(codeEnum)i];
    }

    public void printMessage()
    {
        Debug.LogError($"[ERROR {codeInt}] {message}");
    }

    public static ReturnCode getByBool(bool value)
    {
        return ((value) ? _SUCCESS : _FAILED);
    }

    public static bool operator ==(ReturnCode obj1, object obj2)
    {
        return obj1.Equals(obj2);
    }

    public static bool operator !=(ReturnCode obj1, object obj2)
    {
        return !obj1.Equals(obj2);
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() == typeof(bool))
            return (((int)code >= 0 && (int)code <= 99) == (bool)obj);

        if (obj.GetType() == typeof(int))
            return (codeInt == (int)obj);

        if (obj.GetType() == typeof(codeEnum))
            return (code == (codeEnum)obj);

        if (obj.GetType() == typeof(string))
            return (code.ToString() == (string)obj);

        return base.Equals(obj);
    }

    public static bool operator true(ReturnCode val)
    {
        return ((int)val.code >= 0 && (int)val.code <= 99);
    }
    public static bool operator false(ReturnCode val)
    {
        return !((int)val.code >= 0 && (int)val.code <= 99);
    }

    public override int GetHashCode()
    {
        return (codeInt + message).GetHashCode();
    }
}
