using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DataStructures.SortedList;

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
        public SortedList<long> _freeBlocks;
        public int Count { get; set; }
        public DynamicHash(int blockCount, string pathOfFile)
        {
            _blockCount = blockCount;
            _sizeOfRecord = new T().GetSizeOfByteArray();
            fs = new FileStream(pathOfFile, FileMode.Create, FileAccess.ReadWrite);
            _root = new TrieExternNode(0, 0, null, false);
            _blockSize = (blockCount * _sizeOfRecord) + Block<T>.HeadSize;
            WriteBlockOnDisk(new Block<T>(0));
            _freeBlocks = new SortedList<long>();
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
                    Block<T> preplnovaciBlock = CreateNewBlock();
                    preplnovaciBlock.Add(data);
                    preplnovaciBlock.NextOffset = -1;

                    WriteBlockOnDisk(preplnovaciBlock);
                    aktualny.NextOffset = preplnovaciBlock.Offset;
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
            {
                block = new Block<T>(_freeBlocks.Pop());
            }
            else
            {
                _lastOffset = _lastOffset + _blockSize;
                block = new Block<T>(_lastOffset);
            }

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
                Console.WriteLine("NIECO SA MI NEZDA, TAM KDE JE BOL FREEBLOCKS");
            }

            if (foundBlock.ValidCount < _blockCount)
            {
                foundBlock.Add(data);
                foundBlock.ValidCountOfChain++;

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
                            ((TrieExternNode) current).BlockOffset = foundBlock.Offset;
                            ((TrieExternNode)current).ValidCount = foundBlock.ValidCount;

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
                        {
                            current = (current as TrieExternNode).Parent as TrieInternNode;
                        }


                        TrieExternNode left = new TrieExternNode(blockLeft.Offset, blockLeft.ValidCount, (TrieInternNode)current, true);
                        TrieExternNode right = new TrieExternNode(blockRight.Offset, blockRight.ValidCount, (TrieInternNode)current, false);

                        ((TrieInternNode)current).Left = left;
                        ((TrieInternNode)current).Right = right;

                        blockLeft.ValidCountOfChain += blockLeft.ValidCount;
                        blockRight.ValidCountOfChain += blockRight.ValidCount;

                        //Ak rozdelujem, ale uz ma zretazene
                        if (foundBlock.NextOffset != -1)
                        {
                            var pom = ReadBlockFromDisk(foundBlock.NextOffset);
                            var pomHash = new BitArray(new int[] { pom.Records[0].GetHash() });
                            var rightHash = new BitArray(new int[] { blockRight.Records[0].GetHash() });

                            if (pomHash[indexOfDepth] == rightHash[indexOfDepth])
                            {
                                blockRight.NextOffset = foundBlock.NextOffset;
                                blockRight.ValidCountOfChain = foundBlock.ValidCountOfChain;
                                blockRight.SizeOfChain = foundBlock.SizeOfChain;
                            }
                            else
                            {
                                blockLeft.NextOffset = foundBlock.NextOffset;
                                blockLeft.ValidCountOfChain = foundBlock.ValidCountOfChain;
                                blockLeft.SizeOfChain = foundBlock.SizeOfChain;
                            }                     
                        }
                           

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
                            TrieExternNode right = new TrieExternNode(-1,0, (TrieInternNode)current, false);

                            ((TrieInternNode)current).Left = left;
                            ((TrieInternNode)current).Right = right;

                            if (curPom == _root)
                            {
                                _root = current;
                            }

                            current = right;
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

                            TrieExternNode left = new TrieExternNode(-1, 0, (TrieInternNode)current, true);
                            TrieExternNode right = new TrieExternNode(-1, 0, (TrieInternNode)current, false);

                            ((TrieInternNode)current).Left = left;
                            ((TrieInternNode)current).Right = right;

                            
                            if (curPom == _root)
                            {
                                _root = current;
                            }

                            current = left;
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

            current = current as TrieInternNode;
            int i = 0;
            if (current != null)
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
            else
                current = _root as TrieExternNode;

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
            current = current as TrieInternNode;

            if (current != null)
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
            else
                current = _root as TrieExternNode;

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

            //TODO: Osetrit ked uzivatel zada zle data a dany block neni platny shodou okolnosti
            Block<T> block = ReadBlockFromDisk(offset);
            var original = block;

            do
            {
                bool vysledok = block.Delete(key);

                if (vysledok)
                {
                    //Vseobecne podmienky
                    Count--;
                    original.ValidCountOfChain--;

                    Striasanie(original, block,ref node);
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

        private List<Block<T>> NacitajRetazec(Block<T> original, Block<T> deleteBlock)
        {
            List<Block<T>> retazec = new List<Block<T>>();
            //Nacitat cely retazec 
            if (original.NextOffset != -1)
            {
                Block<T> current = ReadBlockFromDisk(original.NextOffset);
                retazec.Add(original);

                Block<T> next = null;

                for (int i = 0; i < original.SizeOfChain; i++)
                {
                    if (current.Offset != deleteBlock.Offset)
                    {
                        if (current.NextOffset != -1)
                            next = ReadBlockFromDisk(current.NextOffset);
                        retazec.Add(current);
                        current = next;
                    }
                    else if (current.NextOffset != -1)
                    {
                        current = ReadBlockFromDisk(current.NextOffset);
                        retazec.Add(deleteBlock);
                    }
                    else
                        retazec.Add(deleteBlock);
                }
            }

            return retazec;
        }

        private void Striasanie(Block<T> original, Block<T> deleteBlock,ref TrieExternNode trieInternNode)
        {
            Block<T> current = deleteBlock;
            Block<T> poslednyBlok = null;
            bool striasaloSa = false;
            //Ak je mozne usetrit jeden blok
            if ((original.SizeOfChain + 1) * _blockCount >= original.ValidCountOfChain + _blockCount)
            {
                striasaloSa = true;
                int acutalIndex = 1;
                List<Block<T>> retazec = NacitajRetazec(original, deleteBlock);

                //ak sa vymazal posledny prvok z celeho retazca
                if (retazec.Count > 0)
                {
                    current = retazec[0];
                    Block<T> next = null;

                    //Ci je posledny v retazci
                    if (deleteBlock != retazec[retazec.Count - 1] || retazec[retazec.Count - 1].ValidCount > 0)
                    {
                        while (acutalIndex <= original.SizeOfChain)
                        {
                            //Presypat vsetky prvky do otca
                            bool change = false;
                            next = retazec[acutalIndex];

                            while (current.Records.Count < _blockCount && next.Records.Count > 0)
                            {
                                current.Add(next.Records[0]);
                                next.Delete(next.Records[0]);
                                change = true;
                            }

                            Block<T> beforeNext = current;
                            current = next;

                            //Ak uz ten dalsi je posledny, tak bude vymazany tak nastavime na -1
                            if (current.NextOffset == -1)
                            {
                                beforeNext.NextOffset = -1;
                            }

                            if (beforeNext != original && change)
                            {
                                WriteBlockOnDisk(beforeNext);
                            }

                            acutalIndex++;
                        }
                    }
                    else
                    {
                        retazec[retazec.Count - 2].NextOffset = -1;
                        if (retazec[retazec.Count - 2] != original)
                        {
                            WriteBlockOnDisk(retazec[retazec.Count - 2]);
                        }

                        poslednyBlok = retazec[retazec.Count - 1];
                    }


                    original.SizeOfChain--;
                }
                else
                {
                    trieInternNode.ValidCount = 0;
                    trieInternNode.BlockOffset = -1;
                }

            }


            //Ak sa neztriasalo
            if (current.Records.Count > 0 && deleteBlock != original && !striasaloSa)
            {
                WriteBlockOnDisk(deleteBlock);
            }
            //Ak sa striaslo tak count je 0, lebo sa to presypalo aby bol posledny przadny
            else if (poslednyBlok == null && striasaloSa)
            {
                if (current.Offset != _lastOffset)
                    _freeBlocks.Add(current.Offset);
                else
                    DeleteLastBlock();
            }
            else if (striasaloSa)
            {

                if (poslednyBlok.Offset != _lastOffset)
                    _freeBlocks.Add(deleteBlock.Offset);
                else
                    DeleteLastBlock();
            }

            //Zapisat original ak nie je prazdny retazec
            if (trieInternNode.BlockOffset != -1)
            {
                trieInternNode.ValidCount = original.ValidCount;
                WriteBlockOnDisk(original);
            }
        }

        public void DeleteLastBlock()
        {
            long sizeToCut = _blockSize;
            _lastOffset -= _blockSize;
            while (_freeBlocks.Count > 0 && _freeBlocks.Last == _lastOffset)
            {
                sizeToCut += _blockSize;
                _freeBlocks.Cut();
                _lastOffset -= _blockSize;
            }


            fs.SetLength(fs.Length - sizeToCut);

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

        public void Clear()
        {
            fs.SetLength(0);
            Count = 0;
            _root = new TrieExternNode(0, 0, null, false);
            WriteBlockOnDisk(new Block<T>(0));
            _freeBlocks = new SortedList<long>();
        }
    }
}
