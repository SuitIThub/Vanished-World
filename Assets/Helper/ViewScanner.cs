using Entity;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Entity.InteractionExecuterViewScan;

public abstract class ViewScanner
{
    public static class Options
    {
        public readonly static Dictionary<string, scanOption> options = new()
            {
                {"isITEM",      isITEM},
                {"isNPC",       isNPC},
                {"isOBJ",       isOBJ},
                {"notITEM",     notITEM},
                {"notNPC",      notNPC},
                {"notOBJ",      notOBJ},
                {"isStackable", isStackable},
                {"isFunnel",    isFunnel }
            };

        public static bool isITEM(ViewScanner scanner, RaycastHit2D hit, Vector2 pos, Entity.Core entity)
        {
            return (entity != null) ? entity.objectType == "ITEM" : false;
        }

        public static bool isNPC(ViewScanner scanner, RaycastHit2D hit, Vector2 pos, Entity.Core entity)
        {
            return (entity != null) ? entity.objectType == "NPC" : false;
        }

        public static bool isOBJ(ViewScanner scanner, RaycastHit2D hit, Vector2 pos, Entity.Core entity)
        {
            return (entity != null) ? entity.objectType == "OBJ" : false;
        }

        public static bool notITEM(ViewScanner scanner, RaycastHit2D hit, Vector2 pos, Entity.Core entity)
        {
            return (entity != null) ? entity.objectType != "ITEM" : false;
        }

        public static bool notNPC(ViewScanner scanner, RaycastHit2D hit, Vector2 pos, Entity.Core entity)
        {
            return (entity != null) ? entity.objectType != "NPC" : false;
        }

        public static bool notOBJ(ViewScanner scanner, RaycastHit2D hit, Vector2 pos, Entity.Core entity)
        {
            return (entity != null) ? entity.objectType != "OBJ" : false;
        }

        public static bool isStackable(ViewScanner scanner, RaycastHit2D hit, Vector2 pos, Entity.Core entity)
        {
            if (entity == null)
                return false;
            else
            {
                Item itemGround = null;
                if (!scanner.data.getElement("item", ref itemGround) || entity.GetType() != typeof(Item))
                    return false;

                Item item = entity as Item;

                ReturnCode mergeCode = item.canBeMergedWith(itemGround);

                if (mergeCode == 401)
                return false;
                else
                    return true;
            }
        }

        public static bool isFunnel(ViewScanner scanner, RaycastHit2D hit, Vector2 pos, Entity.Core entity)
        {
            if (entity != null)
            {
                bool isFunnel = true;
                entity.Info().getElement("isFunnel", ref isFunnel);

                if (!entity.Inventory(out _))
                    isFunnel = false;

                return (isFunnel);
            }
            else
                return false;
        }
    }

    public string id { get; }

    public Vector2 center { get; private protected set; }

    public float scanRadius { get; private protected set; }

    public delegate void scanMethod(ViewScanner scanner, List<Entity.Core> filteredEntities, List<Entity.Core> unfilteredEntities, Vector2 pos);
    public delegate bool scanOption(ViewScanner scanner, RaycastHit2D hit, Vector2 pos, Entity.Core entity);

    private protected scanMethod OnUpdate = null;
    private protected scanMethod OnConfirm = null;

    private protected List<Entity.Core> filteredEntities;
    private protected List<Entity.Core> unfilteredEntities;
    private protected Vector2 pos;

    private Dictionary<int, List<RuntimeMethod>> options = new Dictionary<int, List<RuntimeMethod>>();

    private protected string[] ignoreIDs;

    public KVStorage data;

    public bool stopAfterConfirm { get; private set; }

    public InteractionExecuterViewScan executer;

    public ViewScanner()
    {
        id = IdUtilities.id;
        center = Vector2.zero;
        scanRadius = 1;

        data = new KVStorage();

        ignoreIDs = new string[0];

        stopAfterConfirm = true;

        executer = null;
    }

    #region setup methods

    public ViewScanner setCenter(Vector2 center)
    {
        this.center = center;
        return this;
    }

    public ViewScanner setRadius(float radius)
    {
        scanRadius = radius;
        return this;
    }

    public ViewScanner setRuntimeMethod(scanMethod method)
    {
        OnUpdate += method;
        return this;
    }

    public ViewScanner setConfirmMethod(scanMethod method)
    {
        OnConfirm += method;
        return this;
    }

    public ViewScanner setOptions(Dictionary<int, List<RuntimeMethod>> options)
    {
        this.options = options;
        return this;
    }

    public ViewScanner setIgnore(params string[] ignore)
    {
        this.ignoreIDs = ignore ?? new string[0];
        return this;
    }

    abstract public ViewScanner setScanRadius(float radius);

    public ViewScanner addData(string key, object value)
    {
        data.Add(key, value);
        return this;
    }

    public ViewScanner setCancelOnConfirm(bool stopAfterConfirm)
    {
        this.stopAfterConfirm = stopAfterConfirm;
        return this;
    }

    #endregion

    public ViewScanner Start(bool autoUpdate = true)
    {
        if (autoUpdate)
            CentreBrain.data.updateMethod.Add(id, updateScanner);

        filteredEntities = new List<Entity.Core>();
        unfilteredEntities = new List<Entity.Core>();
        pos = Vector2.zero;

        return this;
    }

    public void updateScanner()
    {
        pos = CentreBrain.cam.getMousePos();
        updateData();
    }

    private protected bool checkOptions(Entity.Core entity, RaycastHit2D hit)
    {
        if (entity == null)
            return false;

        foreach(List<RuntimeMethod> optionList in options.Values)
        {
            bool isSuccess = false;

            foreach(RuntimeMethod option in optionList)
            {
                if (option(executer, this, null, null, entity, pos))
                {
                    isSuccess = true;
                    break;
                }
            }

            if (!isSuccess)
                return false;
        }

        return true;
    }

    private protected abstract void updateData();

    public void confirmScan(KVStorage _ = null)
    {
        if (stopAfterConfirm)
            CentreBrain.data.updateMethod.Remove(id);

        OnConfirm?.Invoke(this, filteredEntities, unfilteredEntities, pos);
    }

    public void cancelScan(KVStorage _ = null)
    {
        CentreBrain.data.updateMethod.Remove(id);
    }

    #region options

    public static bool optionViewObject(ViewScanner scanner, RaycastHit2D hit, Vector2 _pos, Entity.Core _entity)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(scanner.center, scanner.center.delta(hit.point), hit.distance);

        foreach(RaycastHit2D rh in hits)
        {
            if (rh.collider == null)
                continue;

            string id = rh.transform.gameObject.getId();
            if (scanner.ignoreIDs.Contains(id))
                continue;

            if (rh.distance < hit.distance)
                return false;
        }

        return true;
    }

    public static bool optionFitsData(ViewScanner scanner, RaycastHit2D hit, Vector2 pos, Entity.Core entity)
    {
        return true;
    }

    #endregion
}

public class CircleScanner : ViewScanner
{
    public CircleScanner() : base()
    {


    }

    public override ViewScanner setScanRadius(float radius)
    {
        return this;
    }

    private protected override void updateData()
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>(Physics2D.CircleCastAll(center, scanRadius, Vector2.one, 0));

        filteredEntities = new List<Entity.Core>();
        unfilteredEntities = new List<Entity.Core>();
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            Entity.Core entity = hit.transform.gameObject.getEntity();
            if (entity == null)
                continue;

            if (!ignoreIDs.Contains(entity.id))
            {
                if (checkOptions(entity, hit) && !filteredEntities.Contains(entity))
                    filteredEntities.Add(entity);
                if (!unfilteredEntities.Contains(entity))
                    unfilteredEntities.Add(entity);
            }
        }
        OnUpdate?.Invoke(this, filteredEntities, unfilteredEntities, pos);
    }
}

public class LineScanner : ViewScanner
{
    public LineScanner() : base()
    {

    }

    public override ViewScanner setScanRadius(float radius)
    {
        return this;
    }

    private protected override void updateData()
    {
        pos = VectorExtensions.Clamp(center, pos, scanRadius);

        RaycastHit2D[] hits = Physics2D.RaycastAll(center, center.delta(pos), scanRadius);

        filteredEntities = new List<Entity.Core>();
        unfilteredEntities = new List<Entity.Core>();
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            Entity.Core entity = hit.transform.gameObject.getEntity();
            if (entity == null)
                continue;

            if (!ignoreIDs.Contains(entity.id))
            {
                if (checkOptions(entity, hit) && !filteredEntities.Contains(entity))
                    filteredEntities.Add(entity);
                if (!unfilteredEntities.Contains(entity))
                    unfilteredEntities.Add(entity);
            }
        }
        OnUpdate?.Invoke(this, filteredEntities, unfilteredEntities, pos);
    }
}

public class ConeScanner : ViewScanner
{
    private float degree;

    public ConeScanner(float degree) : base()
    {
        this.degree = degree;
    }

    public override ViewScanner setScanRadius(float radius)
    {
        this.degree = radius;

        return this;
    }

    private protected override void updateData()
    {
        pos = VectorExtensions.Clamp(center, pos, scanRadius);

        float direction = center.toDegree(pos);
        float degMin = direction - degree / 2;
        float degMax = direction + degree / 2;

        if (degMin < 0)
            degMin += 360f;
        if (degMax >= 360)
            degMax -= 360f;

        List<RaycastHit2D> hits = new List<RaycastHit2D>(Physics2D.CircleCastAll(center, scanRadius, Vector2.one, 0));
        hits.AddRange(Physics2D.RaycastAll(center, degMin.degreeToVector2(), scanRadius));
        hits.AddRange(Physics2D.RaycastAll(center, degMax.degreeToVector2(), scanRadius));

        filteredEntities = new List<Entity.Core>();
        unfilteredEntities = new List<Entity.Core>();
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            float dir = center.toDegree(hit.point);
            if ((dir >= degMin && dir <= degMax) || (dir <= degMax && degMin >= degMax) || (dir >= degMin && degMax <= degMin))
            {
                Entity.Core entity = hit.transform.gameObject.getEntity();
                if (entity == null)
                    continue;

                if (!ignoreIDs.Contains(entity.id))
                {
                    if (checkOptions(entity, hit) && !filteredEntities.Contains(entity))
                        filteredEntities.Add(entity);
                    if (!unfilteredEntities.Contains(entity))
                        unfilteredEntities.Add(entity);
                }
            }
        }
        OnUpdate?.Invoke(this, filteredEntities, unfilteredEntities, pos);
    }
}

public class PointScanner : ViewScanner
{
    private float radius;

    public PointScanner(float radius) : base()
    {
        this.radius = radius;
    }

    public override ViewScanner setScanRadius(float radius)
    {
        this.radius = radius;
        return this;
    }

    private protected override void updateData()
    {
        pos = VectorExtensions.Clamp(center, pos, scanRadius - radius);
        if (scanRadius < radius)
            pos = center;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(pos, radius, Vector2.one, 0);
        filteredEntities = new List<Entity.Core>();
        unfilteredEntities = new List<Entity.Core>();
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            Entity.Core entity = hit.transform.gameObject.getEntity();
            if (entity == null)
                continue;

            if (!ignoreIDs.Contains(entity.id))
            {
                if (checkOptions(entity, hit) && !filteredEntities.Contains(entity))
                    filteredEntities.Add(entity);
                if (!unfilteredEntities.Contains(entity))
                    unfilteredEntities.Add(entity);
            }
        }
        OnUpdate?.Invoke(this, filteredEntities, unfilteredEntities, pos);
    }
}