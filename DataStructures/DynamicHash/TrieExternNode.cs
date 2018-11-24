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
        public bool IsLeft { get; set; }
        public int ValidCount { get; set; }
        public TrieExternNode(long blockOffset,int validCount, TrieInternNode parent, bool isLeft)
        {
            BlockOffset = blockOffset;
            Parent = parent;
            IsLeft = isLeft;
            ValidCount = validCount;
        }
    }
}
