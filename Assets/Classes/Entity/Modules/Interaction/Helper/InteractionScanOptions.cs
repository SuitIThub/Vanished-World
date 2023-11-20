using Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionScanOptions
{
    public static Dictionary<string, ViewScanner.scanOption> options = new Dictionary<string, ViewScanner.scanOption>()
    {
        { "isITEM",      isITEM      },
        { "isNPC",       isNPC       },
        { "isOBJ",       isOBJ       },
        { "notITEM",     notITEM     },
        { "notNPC",      notNPC      },
        { "notOBJ",      notOBJ      },
        { "isStackable", isStackable },
        { "isFunnel",    isFunnel    }
    };

    public static bool isITEM(ViewScanner _scan, RaycastHit2D _hit, Vector2 _pos, Entity.Core entity)
    {
        return entity.objectType == "ITEM";
    }

    public static bool isNPC(ViewScanner _scan, RaycastHit2D _hit, Vector2 _pos, Entity.Core entity)
    {
        return entity.objectType == "NPC";
    }

    public static bool isOBJ(ViewScanner _scan, RaycastHit2D _hit, Vector2 _pos, Entity.Core entity)
    {
        return entity.objectType == "OBJ";
    }

    public static bool notITEM(ViewScanner _scan, RaycastHit2D _hit, Vector2 _pos, Entity.Core entity)
    {
        return entity.objectType != "ITEM";
    }

    public static bool notNPC(ViewScanner _scan, RaycastHit2D _hit, Vector2 _pos, Entity.Core entity)
    {
        return entity.objectType != "NPC";
    }

    public static bool notOBJ(ViewScanner _scan, RaycastHit2D _hit, Vector2 _pos, Entity.Core entity)
    {
        return entity.objectType != "OBJ";
    }

    public static bool isFunnel(ViewScanner _scan, RaycastHit2D _hit, Vector2 _pos, Entity.Core entity)
    {
        bool isFunnel = true;
        entity.Info().getElement("isFunnel", ref isFunnel);

        if (!entity.Inventory(out _))
            isFunnel = false;

        return isFunnel;
    }

    public static bool isStackable(ViewScanner scan, RaycastHit2D _hit, Vector2 _pos, Entity.Core entity)
    {
        if (entity == null)
            return true;

        Item itemGround = null;
        if (!scan.data.getElement("item", ref itemGround) || entity.GetType() != typeof(Item))
            return false;

        Item item = entity as Item;

        return item.canBeMergedWith(itemGround) != 401;
    }
}
