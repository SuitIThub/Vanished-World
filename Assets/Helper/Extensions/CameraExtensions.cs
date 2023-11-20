using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraExtensions
{
    public static Vector2 getMousePos(this Camera camera)
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
