using System.Collections.Generic;
using static Entity.IModule;

namespace Entity {
    public static class ModuleExtensions
    {
        public static bool GetModule(Entity.Core parent, KVStorage data, out IModule module)
        {
            string moduleType = "";
            string type = "";
            if (data.getElement("module", ref moduleType) &&
                data.getElement("type", ref type))
            {
                if (moduleType == "information")
                {
                    module = type switch
                    {
                        _ => new Info.Core(parent, data)
                    };
                    return true;
                }
                else if (moduleType == "inventory")
                {
                    module = type switch
                    {
                        "droppable" => new Inventory.Droppable(parent, data),
                        "player"    => new Inventory.Player(parent, data),
                        "display"   => new Inventory.Display(parent, data),
                        _           => new Inventory.Core(parent, data),
                    };
                    return true;
                }
                else if (moduleType == "interaction")
                {
                    module = type switch
                    {
                        "item"     => new Interaction.Item(parent, data),
                        "tool"     => new Interaction.Tool(parent, data),
                        "player"   => new Interaction.Player(parent, data),
                        "resource" => new Interaction.Resource(parent, data),
                        _          => new Interaction.Core(parent, data),
                    };
                    return true;
                }
                else if (moduleType == "movement")
                {
                    module = type switch
                    {
                        "player" => new Movement.Player(parent, data),
                        _        => new Movement.Core(parent, data),
                    };
                    return true;
                }
            }

            module = null;
            return false;
        }

        public static void Put(this Dictionary<string, ModuleMethod> dict, string key, ModuleMethod value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        public static ReturnCode Run(this Dictionary<string, ModuleMethod> dict, KVStorage input, out object output)
        {
            output = null;

            string methodKey = "";
            if (!input.getElement("methodKey", ref methodKey))
                return ReturnCode.Code(101);

            if (!dict.getValue(methodKey, out ModuleMethod method))
                return ReturnCode.Code(102);

            return method(input, out output);
        }
    }
}
