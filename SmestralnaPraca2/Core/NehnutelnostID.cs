using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures.DynamicHash;


namespace SmestralnaPraca2.Core
{
    class NehnutelnostID : Nehnutelnost, IHashRecord ,IComparable<NehnutelnostID>
    {
        public long offset;
        public NehnutelnostID()
        {
        }

        public override void FromByteArray(byte[] byteArray)
        {
            offset = BitConverter.ToInt64(byteArray, 0);
            Id = BitConverter.ToInt32(byteArray, 8);

        }

        public override byte[] ToByteArray()
        {
            byte[] byteArray = new byte[GetSizeOfByteArray()];

            BitConverter.GetBytes(offset).CopyTo(byteArray, 0);
            BitConverter.GetBytes(Id).CopyTo(byteArray, 8);

            return byteArray;
        }

        public override int GetSizeOfByteArray()
        {
            return 12;
        }


        public int GetHash()
        {
            return Id;
        }

        public void NacitajNehnutelnost(RandomAccessFile<Nehnutelnost> randomAccessFile)
        {
            Nehnutelnost nehnutelnost = randomAccessFile.ReadDataFromFile(offset);
            Id = nehnutelnost.Id;
            NazovKatastra = nehnutelnost.NazovKatastra;
            Popis = nehnutelnost.Popis;
            SupisneCislo = nehnutelnost.SupisneCislo;
        }

        public override bool Equals(object other)
        {
            if (((Nehnutelnost)other).Id == Id)
                return true;
            else
                return false;
        }

        public int CompareTo(NehnutelnostID other)
        {
            if (Id < other.Id)
                return 1;
            else if (Id > other.Id)
                return -1;
            else
                return 0;
        }

        public void VypisNehnutelnost(RandomAccessFile<Nehnutelnost> randomAccessFile)
        {
            NacitajNehnutelnost(randomAccessFile);
            Console.WriteLine(offset);
        }

        public override string ToString()
        {
            return "Offset: " + offset.ToString();
        }
    }
}
