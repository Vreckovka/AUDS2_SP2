using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures.DynamicHash
{
    class TrieInternNode : Node
    {
        public Node Left { get; set; }
        public Node Right { get; set; }


        public override void VypisNode()
        {
            Console.WriteLine($"TRIE INTERN:\n" +
                              $"Block offset {Left}\n" +
                              $"Valid count {Right}\n");
        }

        public override string ToString()
        {
            return "TrieInternNode";
        }
    }
}
