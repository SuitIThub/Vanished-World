using Entity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CentreBrain : MonoBehaviour
{
    private const int UPDATE_METHOD_BATCH_LIMIT = 50;

    public static WorldStorage data;

    public static KeyManager keyManager;
    public static GameObject cameraGO;

    public static Camera cam;

    public static MessageSystem messageSystem;

    public static Entity.Inventory.InventoryManager inventoryManager;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 120;

        data = new WorldStorage();
        cam = GetComponent<Camera>();

        messageSystem = new MessageSystem();

        ReturnCode.initialize();

        StartTestScenario();
    }

    private void Update()
    {
        PositionCamera();

        messageSystem.Update();

        CheckUpdateMethods();
    }


    private int updateMethodPos = 0;
    private void CheckUpdateMethods()
    {
        if (data.updateMethod.Count > 0)
        {
            int startPos = updateMethodPos;
            for (int i = 0;
                i < ((UPDATE_METHOD_BATCH_LIMIT != -1) ? UPDATE_METHOD_BATCH_LIMIT : data.updateMethod.Count);
                i++)
            {
                data.updateMethod.ElementAt(updateMethodPos).Value();

                updateMethodPos++;

                if (updateMethodPos >= data.updateMethod.Count)
                    updateMethodPos = 0;

                if (updateMethodPos == startPos)
                    break;
            }
        }
    }

    private void StartTestScenario()
    {
        keyManager = GetComponent<KeyManager>();

        EntitySpawnerExtensions.spawnEntity(
            "Json/NPC/Player",
            new Vector2(200, 100),
            out Entity.Core player,
            null);
        cameraGO = player.gameObject;
        data.player = player;

        InteractionExecuter.initialize(player);

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
                EntitySpawnerExtensions.spawnEntity(
                    "Json/OBJ/Tree",
                    new Vector2(195f - (i * 2f), 95 - (j * 2f)),
                    out Entity.Core _,
                    null
                );
        }

        if (player.Inventory(out Entity.Inventory.Core inventory))
        {
            EntitySpawnerExtensions.spawnItem(
                "Json/Item/Tools/PremiumPickaxe",
                out Entity.Item itemPick,
                null,
                inventory, 
                true, 
                false
            );

            EntitySpawnerExtensions.spawnItems(
                "Json/Item/Stick",
                2,
                out IEnumerable<Entity.Item> items,
                null,
                inventory,
                true,
                false
            );
        }

        EntitySpawnerExtensions.spawnEntity(
            "Json/OBJ/CraftingTable",
            new Vector2(200, 105),
            out Entity.Core table,
            null
        );
    }

    private void PositionCamera()
    {
        if (cameraGO)
        {
            Vector3 pos = cameraGO.transform.position;
            pos.z = -10;
            cam.transform.position = pos;
        }
    }
}
