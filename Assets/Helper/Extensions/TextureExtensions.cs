using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureExtensions
{
    public static Sprite toSprite(this Texture2D texture)
    {
        if (texture == null)
            return null;

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }
}
