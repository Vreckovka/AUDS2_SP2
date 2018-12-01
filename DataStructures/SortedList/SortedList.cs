using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures.SortedList
{
    public class SortedList<T> where T : IComparable<T>
    {
        public int Count { get; set; }
        public T Last { get; set; }
        public T Peek { get; set; }
        private List<T> _records = new List<T>();

        public void Add(T data)
        {
            int indexOfElement = FindIndexForAdd(data);
            _records.Add(default(T));

            for (int i = Count; i > indexOfElement; i--)
            {
                _records[i] = _records[i - 1];
            }

            _records[indexOfElement] = data;
            Count++;

            Last = _records[_records.Count - 1];
            Peek = _records[0];
        }

        public T Pop()
        {
            if (Count > 0)
            {
                var item = _records[0];
                _records.RemoveAt(0);

                Count--;
                return item;
            }

            else
                throw new ArgumentException("List is empty");
        }

        public bool Contains(T data)
        {
            if (_records.Count == 0)
            {
                return false;
            }
            else
            {
                int max = _records.Count - 1;
                int min = 0;

                while (true)
                {
                    int acutalIndex = (max + min) / 2;
                    T current = _records[acutalIndex];

                    int vysledok = current.CompareTo(data);

                    if (vysledok == 0)
                        return true;

                    if (max == min)
                        return false;

                    if (vysledok == -1)
                    {
                        if (acutalIndex < _records.Count - 1)
                            min = acutalIndex + 1;
                        else
                            min = _records.Count - 1;
                    }
                    else
                    {
                        max--;
                    }
                }
            }
        }

        private int FindIndexForAdd(T value)
        {
            if (Count == 0)
            {
                return 0;
            }
            else
            {
                int max = _records.Count;
                int min = 0;

                while (true)
                {
                    int acutalIndex = (max + min) / 2;
                    T current = _records[acutalIndex];

                    int vysledok = current.CompareTo(value);

                    if (vysledok == -1)
                    {
                        if (acutalIndex < _records.Count - 1)
                            min = acutalIndex + 1;
                        else
                            return _records.Count;
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
    }
}
