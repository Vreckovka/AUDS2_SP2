using System;
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
        private SortedList<int> _freeBlocks { get; set; }
        private long lastOffset { get; set; }
        private FileStream fs;
        private int _sizeOfRecord;

        public RandomAccessFile(string pathOfFIle, bool createNew)
        {
            _freeBlocks = new SortedList<int>();

            if (createNew)
                fs = new FileStream(pathOfFIle, FileMode.Create, FileAccess.ReadWrite);
            else
                fs = new FileStream(pathOfFIle, FileMode.Open, FileAccess.ReadWrite);

            _sizeOfRecord = new T().GetSizeOfByteArray();
        }

        public long Add(T data)
        {
            fs.Seek(lastOffset, SeekOrigin.Begin);
            fs.Write(data.ToByteArray(), 0, _sizeOfRecord);
            Count++;
            lastOffset += _sizeOfRecord;
            return lastOffset - _sizeOfRecord;
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
