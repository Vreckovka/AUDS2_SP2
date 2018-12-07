using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures.DynamicHash;
using DataStructures.SortedList;
using System.IO;

namespace SmestralnaPraca2.Core
{
    class RandomAccessFile<T> where T : IByteRecord, new()
    {
        public int Count { get; set; }
        public SortedList<long> _freeBlocks { get; set; }
        private long lastOffset { get; set; }
        private FileStream fs;
        private int _sizeOfRecord;
        private string _pathOfFIle;

        public RandomAccessFile(string pathOfFIle, bool createNew)
        {
            _freeBlocks = new SortedList<long>();
            _sizeOfRecord = new T().GetSizeOfByteArray();
            _pathOfFIle = pathOfFIle;

            if (createNew)
                fs = new FileStream(pathOfFIle, FileMode.Create, FileAccess.ReadWrite);
            else
            {
                fs = new FileStream(pathOfFIle, FileMode.Open, FileAccess.ReadWrite);
                Load();
            }
        }

        public Queue<KeyValuePair<T, long>> GetBlocksSequentionally()
        {
            Queue<KeyValuePair<T, long>> queue = new Queue<KeyValuePair<T, long>>();

            for (int i = 0; i < fs.Length; i += _sizeOfRecord)
            {
                T data = ReadDataFromFile(i);
                queue.Enqueue(new KeyValuePair<T, long>(data, i));
            }

            return queue;
        }

        public void Save()
        {
            WriteControlBlock();
            fs.Flush();
            fs.Close();
        }

        public void Load()
        {
            ReadControlBlock();
        }
        private void ReadControlBlock()
        {
            string name = Path.GetFileName(_pathOfFIle).Replace(".bin", string.Empty);
            StreamReader fileStream = new StreamReader(name + "_Data.txt");
            Count = Convert.ToInt32(fileStream.ReadLine());
            lastOffset = Convert.ToInt64(fileStream.ReadLine());

            var _freeBlocksArray = fileStream.ReadLine()?.Split(';');

            if (_freeBlocksArray != null)
            {
                foreach (string offset in _freeBlocksArray)
                {
                    if (offset != "")
                        _freeBlocks.Add(Convert.ToInt64(offset));
                }
            }

            fileStream.Close();
        }

        public void Delete(long offset)
        {
            if (offset != lastOffset - _sizeOfRecord)
                _freeBlocks.Add(offset);
            else
                DeleteLastBlock();
        }

        public void DeleteLastBlock()
        {
            long sizeToCut = _sizeOfRecord;
            lastOffset -= _sizeOfRecord;
            while (_freeBlocks.Count > 0 && _freeBlocks.Last == lastOffset - _sizeOfRecord)
            {
                sizeToCut += _sizeOfRecord;
                _freeBlocks.Cut();
                lastOffset -= _sizeOfRecord;
            }


            fs.SetLength(fs.Length - sizeToCut);

        }

        private void WriteControlBlock()
        {
            string name = Path.GetFileName(_pathOfFIle).Replace(".bin", string.Empty);
            StreamWriter fileStream = new StreamWriter(name + "_Data.txt");
            fileStream.WriteLine(Count);
            fileStream.WriteLine(lastOffset);
            foreach (long offset in _freeBlocks._records)
            {
                fileStream.Write(offset + ";");
            }

            fileStream.Flush();
            fileStream.Close();

        }

        public long Add(T data)
        {
            if (_freeBlocks.Count == 0)
            {
                fs.Seek(lastOffset, SeekOrigin.Begin);
                fs.Write(data.ToByteArray(), 0, _sizeOfRecord);
                Count++;
                lastOffset += _sizeOfRecord;
                return lastOffset - _sizeOfRecord;
            }
            else
            {
                var offset = _freeBlocks.Pop();
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Write(data.ToByteArray(), 0, _sizeOfRecord);
                Count++;
                return offset;
            }
        }

        public T ReadDataFromFile(long offset)
        {
            T data = new T();

            byte[] byteArray = new byte[_sizeOfRecord];

            fs.Seek(offset, SeekOrigin.Begin);
            fs.Read(byteArray, 0, _sizeOfRecord);

            data.FromByteArray(byteArray);
            return data;
        }

        public void GetBlocksSequentionallyConsole()
        {
            Console.Clear();
            Console.WriteLine("Count: " + Count);
            for (int i = 0; i < fs.Length; i += _sizeOfRecord)
            {
                T data = ReadDataFromFile(i);
                Console.WriteLine(data);
                Console.WriteLine("---------------------");
            }
        }
    }
}
