using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DataStructures.DynamicHash
{
    public class DynamicHash<T> where T : IHashRecord, IComparable<T>, new()
    {
        private int _depth;
        private Node _root;
        private int _blockCount;
        private int _blockSize;
        private int _sizeOfRecord;
        private long _lastOffset;
        private FileStream fs;
        public List<long> _freeBlocks;
        public int Count { get; set; }
        public DynamicHash(int blockCount, string pathOfFile)
        {
            _blockCount = blockCount;
            _sizeOfRecord = new T().GetSizeOfByteArray();
            fs = new FileStream(pathOfFile, FileMode.Create, FileAccess.ReadWrite);
            _root = new TrieExternNode(0, 0, null, false);
            _blockSize = (blockCount * _sizeOfRecord) + Block<T>.HeadSize;
            WriteBlockOnDisk(new Block<T>(0));
            _freeBlocks = new List<long>();
        }


        private void WriteBlockOnDisk(Block<T> block)
        {
            byte[] blockByte = new byte[_blockSize];
            BitConverter.GetBytes(block.ValidCount).CopyTo(blockByte, 0);
            BitConverter.GetBytes(block.ValidCountOfChain).CopyTo(blockByte, 4);
            BitConverter.GetBytes(block.SizeOfChain).CopyTo(blockByte, 8);
            BitConverter.GetBytes(block.NextOffset).CopyTo(blockByte, 12);

            int index = 0;

            foreach (T record in block.Records)
            {
                byte[] array = record.ToByteArray();
                Buffer.BlockCopy(array, 0, blockByte, (index * _sizeOfRecord) + Block<T>.HeadSize, array.Length);
                index++;
            };

            fs.Seek(block.Offset, SeekOrigin.Begin);
            fs.Write(blockByte, 0, blockByte.Length);

            block.Records = null;
        }


        private Block<T> ReadBlockFromDisk(long offset)
        {
            Block<T> block = new Block<T>(offset);
            var records = new List<T>();
            byte[] byteArray = new byte[(_sizeOfRecord * _blockCount) + Block<T>.HeadSize];

            fs.Seek(offset, SeekOrigin.Begin);
            fs.Read(byteArray, 0, _sizeOfRecord * _blockCount + Block<T>.HeadSize);

            block.ValidCount = BitConverter.ToInt32(byteArray, 0);
            block.ValidCountOfChain = BitConverter.ToInt32(byteArray, 4);
            block.SizeOfChain = BitConverter.ToInt32(byteArray, 8);
            block.NextOffset = BitConverter.ToInt64(byteArray, 12);

            for (int i = Block<T>.HeadSize; i < block.ValidCount * _sizeOfRecord + Block<T>.HeadSize; i += _sizeOfRecord)
            {

                byte[] recordArray = new byte[_sizeOfRecord];
                Array.Copy(byteArray, i, recordArray, 0, _sizeOfRecord);
                T record = new T();
                record.FromByteArray(recordArray);
                records.Add(record);

            }

            block.Records = records;
            return block;
        }
        private void VytvorPreplnovaciBlock(T data, Block<T> aktualny)
        {
            Block<T> prvy = aktualny;
            if (prvy.Records.Count > _blockCount)
                prvy.Delete(data);
            bool pridane = false;
            while (true)
            {
                while (aktualny.NextOffset != -1)
                {
                    aktualny = ReadBlockFromDisk(aktualny.NextOffset);
                    if (aktualny.ValidCount < _blockCount)
                    {
                        aktualny.Add(data);
                        prvy.ValidCountOfChain++;

                        if (prvy != aktualny)
                            WriteBlockOnDisk(aktualny);
                        WriteBlockOnDisk(prvy);

                        pridane = true;
                        break;
                    }
                }

                if (!pridane)
                {
                    _lastOffset = _lastOffset + _blockSize;
                    Block<T> preplnovaciBlock = new Block<T>(_lastOffset);
                    preplnovaciBlock.Add(data);
                    preplnovaciBlock.NextOffset = -1;

                    WriteBlockOnDisk(preplnovaciBlock);

                    aktualny.NextOffset = _lastOffset;
                    prvy.SizeOfChain++;
                    prvy.ValidCountOfChain++;

                    if (aktualny != prvy)
                        WriteBlockOnDisk(prvy);

                    WriteBlockOnDisk(aktualny);

                    break;
                }

                if (pridane)
                    break;

            }
            Count++;
        }

        public Block<T> CreateNewBlock()
        {
            Block<T> block = null;
            if (_freeBlocks.Count != 0)
                block = new Block<T>(_freeBlocks.Count - 1);
            else
                block = new Block<T>(_lastOffset);

            _lastOffset = _lastOffset + _blockSize;
            return block;
        }
        public void Add(T data)
        {
            BitArray bitHash = new BitArray(new int[] { data.GetHash() });
            Node current = _root;

            int indexOfDepth = 0;

            current = _root as TrieInternNode;
            if (current != null)
            {
                int i = 0;
                while (true)
                {
                    if (bitHash[i])
                    {
                        if (((TrieInternNode)current).Right is TrieInternNode)
                        {
                            current = ((TrieInternNode)current).Right;
                            indexOfDepth++;
                        }
                        else
                        {
                            current = ((TrieInternNode)current).Right as TrieExternNode;
                            break;
                        }
                    }
                    else
                    {
                        if (((TrieInternNode)current).Left is TrieInternNode)
                        {
                            current = ((TrieInternNode)current).Left;
                            indexOfDepth++;
                        }
                        else
                        {
                            current = ((TrieInternNode)current).Left as TrieExternNode;
                            break;
                        }
                    }

                    i++;
                }
            }
            else
                current = _root as TrieExternNode;

            Block<T> foundBlock = null;

            if (((TrieExternNode)current).BlockOffset != -1)
                foundBlock = ReadBlockFromDisk(((TrieExternNode)current).BlockOffset);
            else
            {
                foundBlock = CreateNewBlock();
                ((TrieExternNode)current).BlockOffset = foundBlock.Offset;
            }

            if (_freeBlocks.Contains(foundBlock.Offset))
            {
                foundBlock = new Block<T>(foundBlock.Offset);
                _freeBlocks.Remove(foundBlock.Offset);
            }

            if (foundBlock.ValidCount < _blockCount)
            {
                foundBlock.Add(data);
                ((TrieExternNode)current).ValidCount = foundBlock.ValidCount;
                WriteBlockOnDisk(foundBlock);

            }
            else
            {
                long offsetForLeft = foundBlock.Offset;
                long offsetForRight = _lastOffset + _blockSize;
                long newOffset = -1;
                bool first = true;
                while (true)
                {
                    var items = foundBlock.Records;

                    Block<T> blockLeft = new Block<T>(offsetForLeft);
                    Block<T> blockRight = new Block<T>(offsetForRight);

                    int i = 0;
                    foreach (T item in items)
                    {
                        BitArray hashT = new BitArray(new int[] { item.GetHash() });
                        if (indexOfDepth < hashT.Count - 1)
                            if (hashT[indexOfDepth] == false)
                            {
                                blockLeft.Add(item);
                            }
                            else
                                blockRight.Add(item);
                        else
                        {
                            VytvorPreplnovaciBlock(data, foundBlock);

                            if (current is TrieInternNode)
                                if (((TrieExternNode)((TrieInternNode)current).Left).ValidCount > _blockCount)
                                    ((TrieExternNode)((TrieInternNode)current).Left).ValidCount--;
                                else
                                    ((TrieExternNode)((TrieInternNode)current).Right).ValidCount--;
                            return;
                        }
                    }

                    if (blockLeft.ValidCount + blockRight.ValidCount < _blockCount + 1)
                    {
                        if (bitHash[indexOfDepth])
                            blockRight.Add(data);
                        else
                            blockLeft.Add(data);
                    }


                    if (indexOfDepth == _depth)
                    {
                        _depth++;
                    }

                    if (blockLeft.ValidCount <= _blockCount && blockRight.ValidCount <= _blockCount)
                    {

                        if (current == _root)
                        {
                            current = new TrieInternNode();
                            _root = current;
                        }

                        if (current is TrieExternNode)
                            current = new TrieInternNode();

                        TrieExternNode left = new TrieExternNode(blockLeft.Offset, blockLeft.ValidCount, (TrieInternNode)current, true);
                        TrieExternNode right = new TrieExternNode(blockRight.Offset, blockRight.ValidCount, (TrieInternNode)current, false);

                        ((TrieInternNode)current).Left = left;
                        ((TrieInternNode)current).Right = right;

                        WriteBlockOnDisk(blockLeft);
                        WriteBlockOnDisk(blockRight);

                        _lastOffset = blockRight.Offset;

                        break;
                    }
                    else
                    {
                        indexOfDepth++;
                        if (blockRight.ValidCount > blockLeft.ValidCount)
                        {
                            var curPom = (current as TrieInternNode)?.Right;
                            if (current is TrieExternNode)
                            {
                                curPom = current;
                            }

                            current = new TrieInternNode();
                            current.Parent = curPom.Parent;

                            if (current.Parent is TrieInternNode)
                                (current.Parent as TrieInternNode).Right = current;

                            if (first)
                            {
                                newOffset = ((TrieExternNode)curPom).BlockOffset;
                                first = false;
                            }

                            TrieExternNode left = new TrieExternNode(-1, 0, (TrieInternNode)current, true);
                            TrieExternNode right = new TrieExternNode(newOffset, blockRight.ValidCount, (TrieInternNode)current, false);

                            ((TrieInternNode)current).Left = left;
                            ((TrieInternNode)current).Right = right;

                            if (curPom == _root)
                            {
                                _root = current;
                            }

                        }
                        else
                        {
                            var curPom = (current as TrieInternNode)?.Left;
                            if (current is TrieExternNode)
                            {
                                curPom = current;
                            }
                            current = new TrieInternNode();
                            current.Parent = curPom.Parent;

                            if (current.Parent is TrieInternNode)
                                (current.Parent as TrieInternNode).Left = current;


                            if (first)
                            {
                                newOffset = ((TrieExternNode)curPom).BlockOffset;
                                first = false;
                            }

                            TrieExternNode left = new TrieExternNode(newOffset, blockLeft.ValidCount, (TrieInternNode)current, true);
                            TrieExternNode right = new TrieExternNode(-1, 0, (TrieInternNode)current, false);

                            ((TrieInternNode)current).Left = left;
                            ((TrieInternNode)current).Right = right;

                            if (curPom == _root)
                            {
                                _root = current;
                            }
                        }
                    }
                }
            }

            Count++;
        }

        public T Find(T key)
        {
            Node current = _root;
            BitArray hashT = new BitArray(new int[] { key.GetHash() });

            int i = 0;
            while (true)
            {
                if (hashT[i])
                {
                    if (((TrieInternNode)current).Right is TrieInternNode)
                    {
                        current = ((TrieInternNode)current).Right;
                    }
                    else
                    {
                        if (_freeBlocks.Contains(((TrieExternNode)((TrieInternNode)current).Right).BlockOffset))
                            current = ((TrieInternNode)current).Left as TrieExternNode;
                        else
                            current = ((TrieInternNode)current).Right as TrieExternNode;
                        break;
                    }
                }
                else
                {
                    if (((TrieInternNode)current).Left is TrieInternNode)
                    {
                        current = ((TrieInternNode)current).Left;
                    }
                    else
                    {
                        if (_freeBlocks.Contains(((TrieExternNode)((TrieInternNode)current).Left).BlockOffset))
                            current = ((TrieInternNode)current).Right as TrieExternNode;
                        else
                            current = ((TrieInternNode)current).Left as TrieExternNode;

                        break;
                    }
                }

                i++;
            }

            var foundBlock = ReadBlockFromDisk(((TrieExternNode)current).BlockOffset);
            int sizeOfChain = foundBlock.SizeOfChain;
            T found = default(T);

            for (i = 0; i < sizeOfChain + 1; i++)
            {
                found = foundBlock.Find(key);
                if (found != null)
                    break;
                if (foundBlock.NextOffset != -1)
                    foundBlock = ReadBlockFromDisk(foundBlock.NextOffset);
                else
                    break;
            }

            return found;
        }

        private long GetBlock(T key, ref TrieExternNode trieInternNode)
        {
            Node current = _root;
            BitArray hashT = new BitArray(new int[] { key.GetHash() });

            int i = 0;
            while (true)
            {
                if (hashT[i])
                {
                    if (((TrieInternNode)current).Right is TrieInternNode)
                    {
                        current = ((TrieInternNode)current).Right;
                    }
                    else if (_root is TrieInternNode)
                    {
                        if (_freeBlocks.Contains(((TrieExternNode)((TrieInternNode)current).Right).BlockOffset))
                            current = ((TrieInternNode)current).Left as TrieExternNode;
                        else
                            current = ((TrieInternNode)current).Right as TrieExternNode;
                        break;
                    }
                    else
                        break;
                }
                else
                {
                    if (((TrieInternNode)current).Left is TrieInternNode)
                    {
                        current = ((TrieInternNode)current).Left;
                    }
                    else if (_root is TrieInternNode)
                    {
                        if (_freeBlocks.Contains(((TrieExternNode)((TrieInternNode)current).Left).BlockOffset))
                            current = ((TrieInternNode)current).Right as TrieExternNode;
                        else
                            current = ((TrieInternNode)current).Left as TrieExternNode;
                        break;
                    }
                    else
                        break;
                }

                i++;
            }

            if (current is TrieExternNode)
            {
                trieInternNode = (TrieExternNode)current;
                return ((TrieExternNode)current).BlockOffset;
            }
            else
                return -1;
        }

        public bool Delete(T key)
        {
            TrieExternNode node = null;
            long offset = -1;
            if (_root is TrieExternNode)
            {
                node = (TrieExternNode)_root;
                offset = 0;
            }
            else
                offset = GetBlock(key, ref node);


            Block<T> block = ReadBlockFromDisk(offset);
            var original = block;
            do
            {
                bool vysledok = block.Delete(key);

                if (vysledok)
                {
                    Count--;
                    if (block != original)
                        original.ValidCountOfChain--;

                    Striasanie(original, block, node);

                    if (original != block)
                    {
                        if (block.Records != null)
                            WriteBlockOnDisk(block);
                    }
                    else
                        node.ValidCount--;

                    return true;
                }
                else
                {
                    if (block.NextOffset != -1)
                        block = ReadBlockFromDisk(block.NextOffset);
                    else
                        return false;
                }

            } while (true);


        }

        private void Striasanie(Block<T> original, Block<T> deleteBlock, TrieExternNode trieInternNode)
        {
            bool striasloSa = false;
            TrieInternNode parent = (TrieInternNode)trieInternNode.Parent;
            //Pre nody
            if (parent != null && (parent.Left is TrieExternNode) && (parent.Right is TrieExternNode) &&
            ((TrieExternNode)parent.Left).ValidCount + ((TrieExternNode)parent.Right).ValidCount <= _blockCount && ((TrieExternNode)parent.Left).ValidCount > 0
                    && ((TrieExternNode)parent.Right).ValidCount > 0)
            {
                Block<T> other = null;
                if (trieInternNode.IsLeft)
                {
                    other = ReadBlockFromDisk(((TrieExternNode)parent.Right).BlockOffset);
                    if (((TrieExternNode)parent.Right).BlockOffset == _lastOffset)
                    {
                        fs.SetLength(fs.Length - _blockSize);
                        _lastOffset = _lastOffset - _blockSize;
                    }
                    else
                        _freeBlocks.Add(other.Offset);

                    ((TrieExternNode)parent.Left).ValidCount = ((TrieExternNode)parent.Left).ValidCount + ((TrieExternNode)parent.Right).ValidCount;
                    ((TrieExternNode)parent.Right).ValidCount = 0;

                }
                else
                {
                    other = ReadBlockFromDisk(((TrieExternNode)parent.Left).BlockOffset);

                    if (((TrieExternNode)parent.Left).BlockOffset == _lastOffset)
                    {
                        fs.SetLength(fs.Length - _blockSize);
                        _lastOffset = _lastOffset - _blockSize;
                    }
                    else
                        _freeBlocks.Add(other.Offset);

                    ((TrieExternNode)parent.Right).ValidCount = ((TrieExternNode)parent.Left).ValidCount + ((TrieExternNode)parent.Right).ValidCount;
                    ((TrieExternNode)parent.Left).ValidCount = 0;
                }

                foreach (T item in other.Records)
                {
                    original.Add(item);

                }

                striasloSa = true;
            }
            //Pre preplnujuce bloky
            else if (((original.SizeOfChain + 1) * _blockCount) >= original.ValidCountOfChain + _blockCount)
            {
                List<Block<T>> zostavajuce = new List<Block<T>>();
                Block<T> current = deleteBlock;
                Block<T> next = ReadBlockFromDisk(current.NextOffset);

                long oneBeforeOffset = -1;
                while (current.NextOffset != -1)
                {
                    if (next.ValidCount < _blockCount)
                    {
                        while (current.Records.Count != _blockCount)
                        {
                            current.Add(next.Records[0]);
                            next.Delete(next.Records[0]);
                            current.ValidCountOfChain--;
                        }

                        if (next.ValidCount > 0)
                        {
                            if (current != original)
                                WriteBlockOnDisk(current);
                            zostavajuce.Add(next);
                            current = next;
                        }
                        else
                        {
                            if (next.Offset == _lastOffset)
                            {
                                fs.SetLength(fs.Length - _blockSize);
                                _lastOffset = _lastOffset - _blockSize;

                                //ak sa vymazava z posledneho a current je predposledny
                                if (oneBeforeOffset == -1)
                                {
                                    current.NextOffset = -1;
                                    WriteBlockOnDisk(current);
                                }
                                else
                                {
                                    var last = ReadBlockFromDisk(oneBeforeOffset);
                                    last.NextOffset = -1;

                                    WriteBlockOnDisk(last);
                                }                       
                            }
                            else
                            {
                                WriteBlockOnDisk(next);
                                current.NextOffset = next.NextOffset;
                                _freeBlocks.Add(next.Offset);
                            }
                            if (current == original)
                                original.SizeOfChain--;

                            if (current.Records != null)
                                WriteBlockOnDisk(current);

                            break;
                        }
                    }

                    oneBeforeOffset = next.Offset;
                    next = ReadBlockFromDisk(next.NextOffset);

                }

                if (current != original)
                {
                    original.SizeOfChain--;
                    WriteBlockOnDisk(original);
                }

                striasloSa = true;
            }

            if (original.ValidCount == 0)
                _freeBlocks.Add(original.Offset);

            if (!striasloSa)
                WriteBlockOnDisk(original);
        }

        public Queue<Block<T>> GetBlocksSequentionally()
        {
            Queue<Block<T>> queue = new Queue<Block<T>>();

            for (int i = 0; i < fs.Length; i += _blockSize)
            {
                Block<T> block = ReadBlockFromDisk(i);
                queue.Enqueue(block);
            }

            return queue;
        }

        public void GetBlocksSequentionallyConsole()
        {
            Console.Clear();
            Console.WriteLine("Count: " + Count);
            for (int i = 0; i < fs.Length; i += _blockSize)
            {
                Console.WriteLine("---------------------");
                Block<T> block = ReadBlockFromDisk(i);
                Console.WriteLine(block);
                foreach (var item in block.Records)
                {
                    Console.WriteLine(item);
                }

                Console.WriteLine("---------------------");
            }
        }

        public void PreOrderTrie()
        {
            // Base Case 
            if (_root == null)
                return;

            Stack<Node> nodeStack = new Stack<Node>();
            nodeStack.Push(_root);

            while (nodeStack.Count != 0)
            {
                // Pop the top item from stack and print it 
                Node curr = nodeStack.Peek();
                curr.VypisNode();
                Console.WriteLine();

                nodeStack.Pop();

                // Push right and left children of the popped node to stack 
                if ((curr as TrieInternNode)?.Right != null)
                    nodeStack.Push((curr as TrieInternNode).Right);
                if ((curr as TrieInternNode)?.Left != null)
                    nodeStack.Push((curr as TrieInternNode).Left);
            }
        }
    }
}
