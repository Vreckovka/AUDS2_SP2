using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures.DynamicHash
{
    class Node 
    {
        public Node Parent { get; set; }

        public virtual void VypisNode() { }

        public virtual string SaveNode()
        {
            return "";
        }
    }
}
