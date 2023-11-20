using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathExtensions
{
    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    public static Vector2 RadianToVector2(float radian, float length)
    {
        return RadianToVector2(radian) * length;
    }
    public static Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }
    public static Vector2 DegreeToVector2(float degree, float length)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad) * length;
    }
    public static float Vector2ToDegree(Vector2 vector)
    {
        if (vector.y >= 0)
            return Vector2.Angle(Vector2.right, vector);
        else
            return Vector2.Angle(Vector2.left, vector) + 180;
    }

    public static float Vector2ToDegree(Vector2 start, Vector2 end)
    {
        Vector2 vector = end - start;

        if (vector.y >= 0)
            return Vector2.Angle(Vector2.right, vector);
        else
            return Vector2.Angle(Vector2.left, vector) + 180;
    }

    public static float Vector2ToRadian(Vector2 vector)
    {
        return Vector2ToDegree(vector) * Mathf.Deg2Rad;
    }

    public static float Round(this float value, int decimals = 0)
    {
        return (float)Math.Round((double)value, decimals);
    }

    public static float FloorRound(this float value, int decimals = 0)
    {
        return Mathf.Floor(value * Mathf.Pow(10, decimals)) / Mathf.Pow(10, decimals);
    }

    public static float CeilRound(this float value, int decimals = 0)
    {
        return Mathf.Ceil(value * Mathf.Pow(10, decimals)) / Mathf.Pow(10, decimals);
    }

    public static float Multiples(this float value, int multiples = 1)
    {
        return Mathf.Round(value / multiples) * multiples;
    }

    public static float FloorMultiples(this float value, int multiples = 1)
    {
        return Mathf.Floor(value / multiples) * multiples;
    }

    public static float CeilMultiples(this float value, int multiples = 1)
    {
        return Mathf.Ceil(value / multiples) * multiples;
    }

    static float sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    public static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = sign(pt, v1, v2);
        d2 = sign(pt, v2, v3);
        d3 = sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    public static Vector2 createMiddle(Vector2 v1, Vector2 v2)
    {
        return v1 + (v2 - v1) / 2;
    }

    public static float difference(this float f1, float f2)
    {
        float diff = f1 - f2;
        if (diff < 0)
            diff = -diff;
        return diff;
    }

}
