using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    public static class EntityExtensions
    {
        public static Info.Core Info<T>(this T entity) where T : Entity.Core
        {
            return (Info.Core)entity.modules["information"];
        }

        public static KVStorage Data(this Core entity)
        {
            return ((Info.Core)entity.modules["information"]).data;
        }

        public static bool Move(this Core entity, out Movement.Core move)
        {
            move = null;
            if (entity.modules.ContainsKey("movement"))
            {
                move = (Movement.Core)entity.modules["movement"];
                return true;
            }
            return false;
        }

        public static bool Interact(this Core entity, out Interaction.Core interact)
        {
            interact = null;
            if (entity.modules.ContainsKey("interaction"))
            {
                interact = (Interaction.Core)entity.modules["interaction"];
                return true;
            }
            return false;
        }

        public static bool Inventory(this Core entity, out Inventory.Core inventory)
        {
            inventory = null;
            if (entity.modules.ContainsKey("inventory"))
            {
                inventory = (Inventory.Core)entity.modules["inventory"];
                return true;
            }
            return false;
        }

        public static void setOutline(this Core entity, int color = -1)
        {
            List<cakeslice.Outline> outlines = null;
            if (entity.Info().getElement("components.outline", ref outlines))
            {
                foreach (cakeslice.Outline outline in outlines)
                {
                    outline.enabled = (color != -1);
                    outline.color = (color != -1) ? color : 0;
                }
            }
        }

        public static void colliderSetActive(this Core entity, bool enabled = true)
        {
            List<Collider2D> colliders = null;
            if (entity.Info().getElement("components.collider", ref colliders))
            {
                foreach (Collider2D collider in colliders)
                {
                    collider.enabled = enabled;
                }
            }
        }

        public static void colliderSetTrigger(this Core entity, bool enabled = true)
        {
            List<Collider2D> colliders = null;
            if (entity.Info().getElement("components.collider", ref colliders))
            {
                foreach (Collider2D collider in colliders)
                {
                    collider.isTrigger = enabled;
                }
            }
        }
    }
}
