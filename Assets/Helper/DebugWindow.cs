using Entity.Inventory;
using System.Collections.Generic;
using UnityEngine;

public class DebugWindow : MonoBehaviour
{
    private Rect fpsRect = new Rect(1000000, 0, 400, 100);

    public Rect windowsOverview = new Rect(0, 0, 200, 10);
    public Rect entityDataRect = new Rect(200, 0, 350, 10);

    private bool[] activeWindows = new bool[] { false, false, false, false, false };
    private bool windowsOverviewActive = false;

    int frameCount = 0;
    float dt = 0.0f;
    float fps = 0.0f;
    float updateRate = 1.0f;

    GUIStyle labelStyle;

    [Range(10, 100)]
    public float fontSize = 20;

    private void Start()
    {
        labelStyle = new GUIStyle();
        labelStyle.fontSize = 20;
        labelStyle.alignment = TextAnchor.UpperRight;
    }

    private void OnGUI()
    {
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1.0f / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1.0f / updateRate;
        }

        fpsRect = ClampToWindow(fpsRect);
        GUI.Label(fpsRect, $"FPS: {Mathf.FloorToInt(fps)}\n" +
            $"Screen: {Screen.width}:{Screen.height}", labelStyle);

        windowsOverview = GUILayout.Window(0, windowsOverview, windowOverview, "Windows", GUILayout.ExpandHeight(true));
        if (GUI.Button(new Rect(0, windowsOverview.y + windowsOverview.height, 100, 20), "Dev Windows"))
            windowsOverviewActive = !windowsOverviewActive;

        if (activeWindows[1]) entityDataRect = ClampToWindow(GUILayout.Window(3, entityDataRect, windowEntityData, "Entity Data", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)));
    }

    void windowOverview(int id)
    {
        if (!windowsOverviewActive)
            windowsOverview.y = -windowsOverview.height;
        else
            windowsOverview.y = 0;

        activeWindows[0] = GUILayout.Toggle(activeWindows[0], "Debug Settings");
        activeWindows[1] = GUILayout.Toggle(activeWindows[1], "Entity Data");
    }

    #region EntityData Display

    private bool selectingEntity = false;
    private Entity.Core displayingEntity = null;
    private List<Entity.Core> entityHistory = new List<Entity.Core>();
    private int historyPos = 0;
    private List<Entity.Core> possibleEntities = new List<Entity.Core>();

    void windowEntityData(int id)
    {
        if (GUI.Button(new Rect(entityDataRect.width - 17, 2, 15, 15), ""))
            activeWindows[1] = false;

        if (entityHistory.Count > 0)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<<<") && historyPos > 0)
            {
                historyPos--;
                displayingEntity = entityHistory[historyPos];
                selectingEntity = false;
                possibleEntities.Clear();
            }
            GUILayout.Label($"    ({historyPos + 1} / {entityHistory.Count})    ");
            if (GUILayout.Button(">>>") && historyPos < entityHistory.Count - 1)
            {
                historyPos++;
                displayingEntity = entityHistory[historyPos];
                selectingEntity = false;
                possibleEntities.Clear();
            }
            GUILayout.EndHorizontal();
        }

        if (selectingEntity)
        {
            GUILayout.Label("Please click on Entity to select.");
            if (GUILayout.Button("Cancel"))
            {
                CentreBrain.keyManager.Remove("Debug_EntityWindow_Select");
                selectingEntity = false;
            }
        }
        else
        {
            if (GUILayout.Button("Select Entity"))
            {
                selectingEntity = true;

                CentreBrain.keyManager.Add("Debug_EntityWindow_Select",
                    new KeyManager.Mouse(0, KeyManager.KeyBase.KeyDir.down, null, selectEntityClick)
                );
            }

            if (possibleEntities.Count > 0)
            {
                GUILayout.Label("Found Entities:");
                for (int i = 0; i < possibleEntities.Count; i++)
                {
                    Entity.Core entity = possibleEntities[i];

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{entity.displayName} ({entity.id})");
                    if (GUILayout.Button("", GUILayout.Width(15)))
                    {
                        selectEntity(entity);
                        possibleEntities.Clear();
                    }
                    GUILayout.EndHorizontal();
                }
        
                if (GUILayout.Button("Cancel"))
                    possibleEntities.Clear();
            }
            else if (displayingEntity != null)
            {
                displayingEntity.DebugWindow(this);
            }
            
        }

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    public void selectEntity(Entity.Core entity)
    {
        int index = entityHistory.FindIndex(x => x.id == entity.id);

        if (index == -1)
        {
            displayingEntity = entity;
            entityHistory.Insert(0, entity);
            historyPos = 0;
        }
        else
        {
            historyPos = index;
            displayingEntity = entityHistory[historyPos];
        }
    }

    public void selectEntityClick(KVStorage data)
    {
        Vector2 mousePos = Vector2.zero;
        if (!data.getElement("mousePos", ref mousePos))
            return;

        List<Entity.Core> entities = new List<Entity.Core>();

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.up, 0);
        foreach(RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;

            Entity.Core entity = hit.collider.gameObject.getEntity();
            if (entity != null)
                entities.Add(entity);
        }

        if (entities.Count == 1)
        {
            selectEntity(entities[0]);
            CentreBrain.keyManager.Remove("Debug_EntityWindow_Select");
            selectingEntity = false;
        }
        else if (entities.Count > 1)
        {
            possibleEntities = entities;
            CentreBrain.keyManager.Remove("Debug_EntityWindow_Select");
            selectingEntity = false;
        }
    }

    #endregion

    private Rect ClampToWindow(Rect input)
    {
        if (input.x < 0)
            input.x = 0;
        if (input.y < 0)
            input.y = 0;
        if (input.x + input.width > Screen.width)
            input.x = Screen.width - input.width;
        if (input.y + input.height > Screen.height)
            input.y = Screen.height - input.height;
        return input;
    }
}
