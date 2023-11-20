using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdUtilities : MonoBehaviour
{
    static string time = "";
    static int value = 0;

    public static string id
    {
        get
        {
            string timeNow = DateTime.Now.ToString("yyyyMMddHHmmss");

            if (time != timeNow)
                value = 0;

            time = timeNow;

            return timeNow + value++.ToString("0000000000");
        }
    }
}
