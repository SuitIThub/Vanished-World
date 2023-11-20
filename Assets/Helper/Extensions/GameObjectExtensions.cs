using UnityEngine;

public static class GameObjectExtensions
{
    public static Entity.Core getEntity(this GameObject go)
    {
        string id = go.getId();

        if (id == null)
            return null;
        else
            return (CentreBrain.data.entities.ContainsKey(id)) ? CentreBrain.data.entities[id] : null;
    }

    public static string getId(this GameObject go)
    {
        return (CentreBrain.data.entities.ContainsKey(go.name)) ? go.name : null;
    }

    public static void destroy(this GameObject go, bool destroyChildren = false)
    {
        if (destroyChildren)
        {
            for (int i = go.transform.childCount - 1; i >= 0; i--)
                go.transform.GetChild(i).gameObject.destroy(true);
        }

        GameObject.Destroy(go);
    }
}
