using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteData
{
    [Serializable]
    public class Save
    {
        public string texturePath;
        public string spriteName;

        public Save(SpriteData data)
        {
            texturePath = data.texturePath;
            spriteName = data.spriteName;
        }
    }
    public Sprite sprite;

    public string texturePath;
    public string spriteName;

    public Save save
    {
        get
        {
            return new Save(this);
        }
    }

    public SpriteData()
    {
        texturePath = "";
        spriteName = "";
        sprite = null;
    }

    public Vector2 normalizeSize(float maxWidth = -1, float maxHeight = -1)
    {
        if (sprite.rect.height > sprite.rect.width && maxWidth == -1)
        {
            float scale = sprite.rect.width / sprite.rect.height;
            return new Vector2(maxHeight, maxHeight * scale);
        }
        else if (sprite.rect.height < sprite.rect.width && maxWidth == -1)
        {
            float scale = sprite.rect.height / sprite.rect.width;
            return new Vector2(maxWidth * scale, maxWidth);
        }
        else if (sprite.rect.height > sprite.rect.width && maxHeight == -1)
        {
            float scale = sprite.rect.width / sprite.rect.height;
            return new Vector2(maxHeight * scale, maxHeight);
        }
        else if (sprite.rect.height < sprite.rect.width && maxHeight == -1)
        {
            float scale = sprite.rect.height / sprite.rect.width;
            return new Vector2(maxHeight, maxHeight * scale);
        }
        else if (maxHeight == -1)
            return new Vector2(maxWidth, maxWidth);
        else if (maxWidth == -1)
            return new Vector2(maxHeight, maxHeight);
        else
            return new Vector2(maxWidth, maxHeight);
    }

    public Vector2 normalizeSize(float maxSize)
    {
        if (sprite.rect.height > sprite.rect.width)
        {
            float scale = sprite.rect.width / sprite.rect.height;
            return new Vector2(maxSize * scale, maxSize);
        }
        else if (sprite.rect.height < sprite.rect.width)
        {
            float scale = sprite.rect.height / sprite.rect.width;
            return new Vector2(maxSize, maxSize * scale);
        }
        else
            return Vector2.one * maxSize;
    }

    public void normalizeScale(ref float scale, float size)
    {
        if (sprite.rect.height != size)
        {
            scale = size / sprite.rect.height;

            if (sprite.rect.width * scale > size)
                scale = size / sprite.rect.width;
        }
    }

    public static ReturnCode createSprite(Save save, out SpriteData spriteData)
    {
        //if (WorldDatabase.sprites != null && WorldDatabase.sprites.ContainsKey(save.texturePath + "\\" + save.spriteName))
        //{
        //    spriteData = WorldDatabase.sprites[save.texturePath + "\\" + save.spriteName];
        //    return true;
        //}

        return createSprite(save.texturePath, save.spriteName, out spriteData);
    }

    public static ReturnCode createSprite(string texturePath, string spriteName, out Sprite sprite)
    {
        SpriteData sd = null;
        ReturnCode output = createSprite(texturePath, spriteName, out sd);
        sprite = sd.sprite;
        return output;
    }

    public static ReturnCode createSprite(string texturePath, string spriteName, out SpriteData spriteData)
    {
        //if (WorldDatabase.sprites != null && WorldDatabase.sprites.ContainsKey(texturePath + "\\" + spriteName))
        //{
        //    spriteData = WorldDatabase.sprites[texturePath + "\\" + spriteName];
        //    return true;
        //}

        spriteData = new SpriteData();
        spriteData.texturePath = texturePath;
        spriteData.spriteName = spriteName;

        //if (WorldDatabase.sprites != null)
        //    WorldDatabase.sprites.Add(spriteData.ToString(), spriteData);

        if (spriteName == "")
            spriteData.sprite = getTexture(texturePath);
        else
            spriteData.sprite = getTexture(texturePath, spriteName);

        if (spriteData.sprite == null)
        {
            spriteData.sprite = getTexture("Sprites/Black Square");
            return ReturnCode.Code(101, "Texture loading failed", true);
        }

        return ReturnCode.Code(0);
    }


    public static ReturnCode createSprite(out Sprite sprite)
    {
        SpriteData sd = null;
        ReturnCode output = createSprite(out sd);
        sprite = sd.sprite;
        return output;
    }


    public static ReturnCode createSprite(out SpriteData spriteData)
    {
        return createSprite("Sprites/Black Square", out spriteData);
    }


    public static ReturnCode createSprite(string texturePath, out Sprite sprite)
    {
        SpriteData sd = null;
        ReturnCode output = createSprite(texturePath, out sd);
        sprite = sd.sprite;
        return output;
    }


    public static ReturnCode createSprite(string texturePath, out SpriteData spriteData)
    {
        return createSprite(texturePath, "", out spriteData);
    }


    public static ReturnCode createSprite(List<object> list, out Sprite sprite)
    {
        SpriteData sd = null;
        ReturnCode output = createSprite(list, out sd);
        sprite = sd.sprite;
        return output;
    }


    public static ReturnCode createSprite(List<object> list, out SpriteData spriteData)
    {
        spriteData = null;
        if (list.Count < 1)
            return createSprite(out spriteData);

        string texturePath = "";
        string spriteName = "";

        if (!list[0].getElement(ref texturePath))
            return createSprite(out spriteData);

        if (list.Count >= 2)
            list[1].getElement(ref spriteName);

        return createSprite(texturePath, spriteName, out spriteData);
    }

    public override string ToString()
    {
        return texturePath + "\\" + spriteName;
    }

    public static Sprite getTexture(string path)
    {
        return (Resources.Load(path) as Texture2D).toSprite();
    }

    public static Sprite getTexture(string path, string sprite)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);

        foreach (Sprite s in sprites)
        {
            if (s.name == sprite)
            {
                return s;
            }
        }

        return null;
    }
}
