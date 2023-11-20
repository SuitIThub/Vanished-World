using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    /// <summary>
    /// converts a Vector2 to a Vector3; (x, y) -> (x, y, 0)
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3 toVec3(this Vector2 v)
    {
        return new Vector3(v.x, v.y, 0);
    }

    public static Vector3 toVec3(this Vector2Int v)
    {
        return new Vector3(v.x, v.y, 0);
    }

    public static Vector3 toVec3(this Vector3Int v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static Vector2 toVec2(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector2 toVec2(this Vector2Int v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector2 toVec2(this Vector3Int v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector3Int toVec3Int(this Vector2 v)
    {
        return new Vector3Int((int)v.x, (int)v.y, 0);
    }

    public static Vector3Int toVec3Int(this Vector2Int v)
    {
        return new Vector3Int(v.x, v.y, 0);
    }

    public static Vector3Int toVec3Int(this Vector3 v)
    {
        return new Vector3Int((int)v.x, (int)v.y, (int)v.z);
    }

    public static Vector2Int toVec2Int(this Vector3 v)
    {
        return new Vector2Int((int)v.x, (int)v.y);
    }

    public static Vector2Int toVec2Int(this Vector2 v)
    {
        return new Vector2Int((int)v.x, (int)v.y);
    }

    public static Vector2Int toVec2Int(this Vector3Int v)
    {
        return new Vector2Int(v.x, v.y);
    }

    public static Vector2 radianToVector2(this float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    public static Vector2 radianToVector2(this float radian, float length)
    {
        return radian.radianToVector2() * length;
    }
    public static Vector2 degreeToVector2(this float degree)
    {
        return (degree * Mathf.Deg2Rad).radianToVector2();
    }
    public static Vector2 degreeToVector2(this float degree, float length)
    {
        return (degree * Mathf.Deg2Rad).radianToVector2() * length;
    }
    public static float toDegree(this Vector2 vector)
    {
        if (vector.y >= 0)
            return Vector2.Angle(Vector2.right, vector);
        else
            return Vector2.Angle(Vector2.left, vector) + 180;
    }

    public static float toDegree(this Vector2 start, Vector2 end)
    {
        Vector2 vector = end - start;

        if (vector.y >= 0)
            return Vector2.Angle(Vector2.right, vector);
        else
            return Vector2.Angle(Vector2.left, vector) + 180;
    }

    public static float toRadian(this Vector2 vector)
    {
        return vector.toDegree() * Mathf.Deg2Rad;
    }

    /// <summary>
    /// Calculates a delta-Vector directed from <paramref name="oldPos"/> to <paramref name="newPos"/>
    /// </summary>
    public static Vector2 delta(this Vector2 oldPos, Vector2 newPos)
    {
        return newPos - oldPos;
    }

    /// <summary>
    /// Calculates a delta-Vector directed from <paramref name="oldPos"/> to <paramref name="newPos"/>
    /// </summary>
    public static Vector3 delta(this Vector3 oldPos, Vector3 newPos)
    {
        return newPos - oldPos;
    }

    public static Vector2 Clamp(Vector2 center, Vector2 end, float length)
    {
        if (Vector2.Distance(center, end) <= length)
            return end;

        return center + center.toDegree(end).degreeToVector2(length);
    }

    public static Vector2 getScreenAddon(this Vector2 pos)
    {
        Vector2 centerPos = pos;
        if (float.IsPositiveInfinity(pos.x))
            centerPos.x = Screen.width / 2;
        else if (float.IsNegativeInfinity(pos.x))
            centerPos.x = -Screen.width / 2;
        if (float.IsPositiveInfinity(pos.y))
            centerPos.y = Screen.height / 2;
        else if (float.IsNegativeInfinity(pos.y))
            centerPos.y = -Screen.height / 2;
        return centerPos;

    }

    public static bool findFreeSpot(this Vector2 pos, float radius, ref List<Vector2> usedPos, ref Vector2 foundPos)
    {
        if (Physics2D.CircleCast(pos, radius, Vector2.one, 0f).collider == null)
        {
            foundPos = pos;
            return true;
        }

        if (usedPos.Count == 200)
            return false;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Vector2 p = pos + new Vector2(i, j) * 0.2f;
                if (usedPos.Contains(p))
                    continue;

                usedPos.Add(p);
                if (p.findFreeSpot(radius, ref usedPos, ref foundPos))
                    return true;
            }
        }

        return false;
    }
}
