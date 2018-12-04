using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures.DynamicHash;

namespace SmestralnaPraca2.Core
{
    class NehnutelnostSupisneCislo : Nehnutelnost, IHashRecord, IComparable<NehnutelnostSupisneCislo>
    {
        public long offset;
        public NehnutelnostSupisneCislo()
        {
        }

        public override void FromByteArray(byte[] byteArray)
        {
            offset = BitConverter.ToInt64(byteArray, 0);
            SupisneCislo = BitConverter.ToInt32(byteArray, 8);

            NazovKatastra = "";
            for (int j = 12; j < 12 + (15 * 2); j++)
            {
                char c = BitConverter.ToChar(byteArray, j);
                if (c == 0) break;

                if (c < 2500)
                    NazovKatastra += c;
            }

        }

        public override byte[] ToByteArray()
        {
            byte[] byteArray = new byte[GetSizeOfByteArray()];

            BitConverter.GetBytes(offset).CopyTo(byteArray, 0);
            BitConverter.GetBytes(SupisneCislo).CopyTo(byteArray, 8);

            int index = 12;

            foreach (char letter in NazovKatastra)
            {
                BitConverter.GetBytes(letter).CopyTo(byteArray, index);
                index += 2;
            }

            return byteArray;
        }

        public override int GetSizeOfByteArray()
        {
            return 42;
        }


        public int GetHash()
        {
            byte[] byteArray = new byte[4];
            var supByte = BitConverter.GetBytes(SupisneCislo % 256);
            byte[] pom = (Encoding.ASCII.GetBytes(NazovKatastra));

            Array.Copy(supByte, 0, byteArray, 3, 1);
            Array.Copy(pom, 0, byteArray, 0, 3);
            int hash = BitConverter.ToInt32(byteArray, 0);

            return hash;
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
            if (((Nehnutelnost)other).SupisneCislo == SupisneCislo && ((Nehnutelnost)other).NazovKatastra == NazovKatastra)
                return true;
            else
                return false;
        }

        public int CompareTo(NehnutelnostSupisneCislo other)
        {
            if (SupisneCislo < other.SupisneCislo)
                return 1;
            else if (SupisneCislo > other.SupisneCislo)
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
