using UnityEngine;

namespace Entity
{
    public class PickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            string id = collision.gameObject.name;
            string itemID = gameObject.name;
            if (CentreBrain.data.entities.ContainsKey(id) && CentreBrain.data.entities.ContainsKey(itemID))
            {
                Core entity = CentreBrain.data.entities[id];

                InteractionData<Entity.Core> data = new(entity, entity, new string[] { "pick_up" });

                Item item = CentreBrain.data.entities[itemID] as Item;

                item.Interact(out Interaction.Core interact);
                interact.RecieveInteraction(data);
            }
        }
    }
}
