using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MeshGraph
{
    public class Circle : MeshGraph
    {
        protected float degree;
        protected float angle;

        public Circle() : base()
        {
            degree = 360;
            angle = 0;
        }

        public override MeshGraph setAngle(float _)
        {
            return this;
        }

        protected override void setMeshData()
        {
            vertices = new Vector3[edges + 3];
            uv = new Vector2[vertices.Length];
            triangles = new int[(edges + 1) * 3];

            float angle = this.angle;

            vertices[0] = Vector2.zero;

            for (int i = 0; i <= edges + 1; i++)
            {
                Vector3 vertex;
                float shortestDist = radius;

                if (blockedByCollider)
                {
                    RaycastHit2D[] hits = Physics2D.RaycastAll(center, angle.degreeToVector2(), radius);
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider == null)
                            continue;
                        string id = hit.collider.gameObject.getId();
                        if (id == null || ignoreIDs.Contains(id))
                            continue;
                        if (hit.distance < shortestDist)
                            shortestDist = hit.distance;
                    }
                }

                vertex = angle.degreeToVector2(shortestDist);

                vertices[i + 1] = vertex;

                if (i > 0)
                {
                    int index = (i - 1) * 3;
                    triangles[index + 0] = 0;
                    triangles[index + 1] = i - 1;
                    triangles[index + 2] = i;
                }

                angle -= degree / edges;
            }
        }
    }

    public class Cone : Circle
    {
        public Cone(float degree) : base()
        {
            this.degree = degree;
        }

        public override MeshGraph setAngle(float angle)
        {
            angle += degree / 2;

            if (angle >= 360)
                angle -= 360f;

            this.angle = angle;

            return this;
        }

        protected override void setMeshData()
        {
            base.setMeshData();
        }
    }

    public string id { get; }

    protected GameObject go;

    protected Vector2 center;
    protected int edges;
    protected float radius;

    protected Vector3[] vertices;
    protected Vector2[] uv;
    protected int[] triangles;

    protected bool blockedByCollider;

    protected string[] ignoreIDs;

    protected LayerMask layerMask;

    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;

    protected Color color = Color.white;

    public MeshGraph()
    {
        id = IdUtilities.id;

        center = Vector2.zero;
        edges = 0;
        radius = 1;
        ignoreIDs = new string[0];
        layerMask = LayerMask.GetMask("Default");
    }

    public abstract MeshGraph setAngle(float angle);

    public MeshGraph setCenter(Vector2 center)
    {
        this.center = center;
        if (go)
            go.transform.position = center;

        return this;
    }

    public MeshGraph setEdges(int edges)
    {
        this.edges = edges;
        return this;
    }

    public MeshGraph setRadius(float radius)
    {
        this.radius = radius;
        return this;
    }

    public MeshGraph setIgnores(params string[] ids)
    {
        ignoreIDs = ids;
        return this;
    }

    public MeshGraph setCollision(bool blockedByCollider)
    {
        this.blockedByCollider = blockedByCollider;
        return this;
    }

    public MeshGraph setColor(Color c)
    {
        color = c;
        return this;
    }

    public MeshGraph setAlpha(float a)
    {
        color.a = a;
        return this;
    }

    public MeshGraph Draw(bool autoUpdate = true)
    {
        if (autoUpdate)
            CentreBrain.data.updateMethod.Add(id, updateGraph);

        go = GameObject.Instantiate(
            CentreBrain.data.Prefabs["MeshGraphic"], 
            center, 
            Quaternion.identity, 
            CentreBrain.data.Folders.Graphics.transform
        );

        meshFilter = go.GetComponent<MeshFilter>();
        meshRenderer = go.GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = 32767;
        updateGraph();

        return this;
    }

    public void Stop(KVStorage _ = null)
    {
        GameObject.Destroy(go);
        CentreBrain.data.updateMethod.Remove(id);
    }

    public void updateGraph()
    {
        setMeshData();

        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.uv = uv;
        meshFilter.mesh.triangles = triangles;

        meshRenderer.material.color = color;
    }

    protected abstract void setMeshData();
}
    
