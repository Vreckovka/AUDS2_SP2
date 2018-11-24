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
        public int Count { get; set; }
        public DynamicHash(int blockCount, string pathOfFile)
        {
            _blockCount = blockCount;
            _sizeOfRecord = new T().GetSizeOfByteArray();
            fs = new FileStream(pathOfFile, FileMode.Create, FileAccess.ReadWrite);
            _root = new TrieExternNode(0, null);
            _blockSize = (blockCount * _sizeOfRecord) + Block<T>.HeadSize;
            WriteBlockOnDisk(new Block<T>(0));
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
            prvy.Delete(data);
            while (true)
            {
                while (aktualny.NextOffset != -1)
                {
                    aktualny = ReadBlockFromDisk(aktualny.NextOffset);
                }

                if (aktualny.ValidCount < _blockCount)
                {
                    aktualny.Add(data);
                    prvy.ValidCountOfChain++;

                    WriteBlockOnDisk(aktualny);
                    WriteBlockOnDisk(prvy);
                    break;
                }
                else
                {
                    _lastOffset = _lastOffset + (_blockCount * _sizeOfRecord) + Block<T>.HeadSize;
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


            }
            Count++;
        }
        public void Add(T data)
        {
            BitArray bitHash = new BitArray(new int[] { data.GetHash() });
            Node current = _root;

            int indexOfDepth = 0;

            current = _root as TrieInternNode;
            if (current != null)
            {
                for (int i = 0; i < _depth; i++)
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
                }
            }
            else
                current = _root as TrieExternNode;

            Block<T> foundBlock = ReadBlockFromDisk(((TrieExternNode)current).BlockOffset);

            if (foundBlock.ValidCount < _blockCount)
            {
                foundBlock.Add(data);
                WriteBlockOnDisk(foundBlock);
            }
            else
            {
                long offsetForLeft = foundBlock.Offset;
                long offsetForRight = _lastOffset + (_blockCount * _sizeOfRecord) + Block<T>.HeadSize;
                while (true)
                {
                    var items = foundBlock.Records;

                    Block<T> blockLeft = new Block<T>(offsetForLeft);
                    Block<T> blockRight = new Block<T>(offsetForRight);

                    int i = 0;
                    foreach (T item in items)
                    {
                        BitArray hashT = new BitArray(new int[] { item.GetHash() });
                        if (indexOfDepth < hashT.Count)
                            if (hashT[indexOfDepth] == false)
                            {
                                blockLeft.Add(item);
                            }
                            else
                                blockRight.Add(item);
                        else
                        {
                            VytvorPreplnovaciBlock(data, foundBlock);
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

                    var curPom = current;
                    current = new TrieInternNode();
                    if (curPom == _root)
                        _root = current;

                    TrieExternNode left = new TrieExternNode(blockLeft.Offset, (TrieInternNode)current);
                    TrieExternNode right = new TrieExternNode(blockRight.Offset, (TrieInternNode)current);

                    ((TrieInternNode)current).Left = left;
                    ((TrieInternNode)current).Right = right;

                    if (indexOfDepth == _depth)
                        _depth++;

                    if (blockLeft.ValidCount <= _blockCount && blockRight.ValidCount <= _blockCount)
                    {
                        if (blockLeft.ValidCount != 0)
                            WriteBlockOnDisk(blockLeft);

                        if (blockRight.ValidCount != 0)
                            WriteBlockOnDisk(blockRight);

                        _lastOffset = blockRight.Offset;

                        break;
                    }
                    else
                    {
                        indexOfDepth++;
                        if (blockRight.ValidCount > blockLeft.ValidCount)
                            current = ((TrieInternNode)current).Right;
                        else
                            current = ((TrieInternNode)current).Left;
                    }
                }
            }

            Count++;
        }

        public T Find(T key)
        {
            Node current = _root;
            BitArray hashT = new BitArray(new int[] { key.GetHash() });

            for (int i = 0; i < _depth; i++)
            {
                if (hashT[i])
                {
                    if (((TrieInternNode)current).Right is TrieInternNode)
                    {
                        current = ((TrieInternNode)current).Right;
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
                    }
                    else
                    {
                        current = ((TrieInternNode)current).Left as TrieExternNode;
                        break;
                    }
                }
            }

            var foundBlock = ReadBlockFromDisk(((TrieExternNode)current).BlockOffset);
            int sizeOfChain = foundBlock.SizeOfChain;
            T found = default(T);

            for (int i = 0; i < sizeOfChain + 1; i++)
            {
                found = foundBlock.Find(key);
                if (found != null)
                    break;
                foundBlock = ReadBlockFromDisk(foundBlock.NextOffset);
            }

            return found;
        }

        private long GetBlock(T key)
        {
            Node current = _root;
            BitArray hashT = new BitArray(new int[] { key.GetHash() });

            for (int i = 0; i < _depth; i++)
            {
                if (hashT[i])
                {
                    if (((TrieInternNode)current).Right is TrieInternNode)
                    {
                        current = ((TrieInternNode)current).Right;
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
                    }
                    else
                    {
                        current = ((TrieInternNode)current).Left as TrieExternNode;
                        break;
                    }
                }
            }

            if (current is TrieExternNode)
                return ((TrieExternNode)current).BlockOffset;
            else
                return -1;
        }

        public bool Delete(T key)
        {
            long offset = GetBlock(key);
            Block<T> block = ReadBlockFromDisk(offset);
            var original = block;
            do
            {             
                bool vysledok = block.Delete(key);

                if (vysledok)
                {
                    original.ValidCountOfChain--;

                    WriteBlockOnDisk(original);
                    WriteBlockOnDisk(block);
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

        public Queue<Block<T>> GetBlocksSequentionally()
        {
            Queue<Block<T>> queue = new Queue<Block<T>>();

            for (int i = 0; i < _lastOffset + _blockSize; i += _blockSize)
            {
                Block<T> block = ReadBlockFromDisk(i);
                queue.Enqueue(block);
            }

            return queue;
        }
    }
}
