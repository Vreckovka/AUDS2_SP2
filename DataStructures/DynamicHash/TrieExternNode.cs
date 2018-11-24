using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures.DynamicHash
{
    class TrieExternNode : Node
    {
        public long BlockOffset { get; set; }
        public TrieInternNode Parent { get; set; }
        public TrieExternNode(long blockOffset, TrieInternNode parent)
        {
            BlockOffset = blockOffset;
            Parent = parent;
        }    
    }
}
