using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures.DynamicHash
{
    public class Block<T> where T : IHashRecord, IComparable<T>, new()
    {
        public bool IsValid { get; set; }
        public int ValidCount { get; set; }
        public int ValidCountOfChain { get; set; }
        public int SizeOfChain { get; set; }
        public long Offset { get; set; }
        public long NextOffset { get; set; }
        public List<T> Records { get; set; }

        public static int HeadSize = 20;

        public Block(long offset)
        {
            Offset = offset;
            NextOffset = -1;
            Records = new List<T>();
            ValidCountOfChain = 0;
            SizeOfChain = 0;

        }

        public void Add(T data)
        {
            int indexOfElement = FindIndexForAdd(data);
            Records.Add(default(T));

            for (int i = ValidCount; i > indexOfElement; i--)
            {
                Records[i] = Records[i - 1];
            }

            Records[indexOfElement] = data;
            ValidCount++;
        }

        private int FindIndexForAdd(T value)
        {
            if (ValidCount == 0)
            {
                return 0;
            }
            else
            {
                int max = Records.Count;
                int min = 0;

                while (true)
                {
                    int acutalIndex = (max + min) / 2;
                    T current = Records[acutalIndex];

                    int vysledok = current.CompareTo(value);

                    if (vysledok == 0 || vysledok == 1)
                    {
                        if (acutalIndex < Records.Count - 1)
                            min = acutalIndex + 1;
                        else
                            return Records.Count;
                    }
                    else
                    {
                        if (acutalIndex > 0)
                            max = acutalIndex;
                        else
                            max--;
                    }

                    if (max == min)
                        return max;
                }
            }
        }

        public T Find(T key, ref int index)
        {
            if (Records.Count == 0)
            {
                return default(T);
            }
            else
            {
                int max = Records.Count - 1;
                int min = 0;

                while (true)
                {
                    int acutalIndex = (max + min) / 2;
                    T current = Records[acutalIndex];

                    int vysledok = current.CompareTo(key);

                    if (current.Equals(key))
                    {
                        index = acutalIndex;
                        return Records[acutalIndex];
                    }
                       

                    if (max == min)
                        return default(T);

                    if (vysledok == 0 || vysledok == 1)
                    {
                        if (acutalIndex < Records.Count - 1)
                            min = acutalIndex + 1;
                        else
                            min = Records.Count - 1;
                    }
                    else
                    {
                        if (acutalIndex > 0)
                            max = acutalIndex - 1;
                        else
                            max--;
                    }
                }
            }
        }

        public T Delete(T key)
        {
            int index = -1;
            T najdene = Find(key, ref index);
            if (najdene != null && najdene.Equals(key))
            {
                ValidCount--;
                Records.RemoveAt(index);

                return najdene;
            }
            else
                return default(T);
        }

        public override string ToString()
        {
            return $"Offset: {Offset} \n" +
                   $"Next offset {NextOffset} \n" +
                   $"Valid count  {ValidCount} \n" +
                   $"Valid count of chain {ValidCountOfChain} \n" +
                   $"Size of chain {SizeOfChain} \n";
        }
    }
}
