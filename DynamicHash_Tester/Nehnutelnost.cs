using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures.DynamicHash;

namespace DynamicHash_Tester
{
    class Nehnutelnost : IByteRecord,IHashRecord, IComparable<Nehnutelnost>
    {
        public Nehnutelnost()
        {

        }

        public Nehnutelnost(int supisneCislo, string nazovKatastra, string popis, int id)
        {
            SupisneCislo = supisneCislo;
            if (nazovKatastra.Length <= 15)
                NazovKatastra = nazovKatastra;
            else
                throw new ArgumentException("Nesprávna dĺžka názvu katastra, maximum je 15 znakov");

            if (popis.Length <= 20)
                Popis = popis;
            else
                throw new ArgumentException("Nesprávna dĺžka popisu, maximum je 20 znakov");


            Id = id;
        }

        public int SupisneCislo { get; set; }
        public string NazovKatastra { get; set; }
        public string Popis { get; set; }
        public int Id { get; set; }

        public byte[] ToByteArray()
        {
            byte[] byteArray = new byte[GetSizeOfByteArray()];

            BitConverter.GetBytes(SupisneCislo).CopyTo(byteArray, 0);

            int index = 4;

            foreach (char letter in NazovKatastra)
            {
                BitConverter.GetBytes(letter).CopyTo(byteArray, index);
                index += 2;
            }

            index = 4 + (15 * 2);
            foreach (char letter in Popis)
            {
                BitConverter.GetBytes(letter).CopyTo(byteArray, index);
                index += 2;
            }

            index = 4 + (15 * 2) + (20 * 2);
            BitConverter.GetBytes(Id).CopyTo(byteArray, index);

            return byteArray;
        }

        public void FromByteArray(byte[] byteArray)
        {
            SupisneCislo = BitConverter.ToInt32(byteArray, 0);

            NazovKatastra = "";
            for (int j = 4; j < 4 + (15 * 2); j++)
            {
                char c = BitConverter.ToChar(byteArray, j);
                if (c == 0) break;

                if (c < 2500)
                    NazovKatastra += c;
            }

            Popis = "";
            for (int j = 4 + (15 * 2); j < 4 + (15 * 2) + (20 * 2); j++)
            {
                char c = BitConverter.ToChar(byteArray, j);
                if (c == 0) break;

                if (c < 2500)
                    Popis += c;
            }

            Id = BitConverter.ToInt32(byteArray, 4 + (15 * 2) + (20 * 2));
        }

        public int GetHash()
        {
            return Id ;
        }

        public int GetSizeOfByteArray()
        {
            return 78;
        }

        public override bool Equals(object other)
        {
            if (((Nehnutelnost)other).Id == Id)
                return true;
            else
                return false;
        }

        public void VypisNehnutelnost()
        {
            Console.WriteLine($"Supisne cislo: {SupisneCislo}\n" +
                              $"Nazov katastra: {NazovKatastra}\n" +
                              $"Popis: {Popis}\n" +
                              $"ID: {Id}\n");
        }

        public int CompareTo(Nehnutelnost other)
        {
            if (Id < other.Id)
                return 1;
            else if (Id > other.Id)
                return -1;
            else
                return 0;
        }

        public override string ToString()
        {
            return $"Supisne cislo: {SupisneCislo} " +
                   $"Nazov katastra: {NazovKatastra} " +
                   $"Popis: {Popis} " +
                   $"ID: {Id} ";
        }
    }
}
