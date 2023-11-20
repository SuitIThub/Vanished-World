using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Interaction
{
    public class Tool : Item
    {
        public Tool() : base()
        {

        }

        public Tool(Entity.Core parent, KVStorage json, KVStorage replaceData = null) : base(parent, json, replaceData)
        {
            
        }
    }
}