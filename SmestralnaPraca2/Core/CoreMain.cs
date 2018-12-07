using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures.DynamicHash;


namespace SmestralnaPraca2.Core
{
    class CoreMain
    {
        internal RandomAccessFile<Nehnutelnost> randomAccessFile;
        internal DynamicHash<NehnutelnostID> dynamicHashId;
        internal DynamicHash<NehnutelnostSupisneCislo> dynamicHashSup;

        public CoreMain()
        {
            randomAccessFile = new RandomAccessFile<Nehnutelnost>("../../Files/data.bin", true);
            dynamicHashId = new DynamicHash<NehnutelnostID>(3, 32,"../../Files/nehnutelnostiID.bin");
            dynamicHashSup = new DynamicHash<NehnutelnostSupisneCislo>(3, 32,"../../Files/nehnutelnostiSupCislo.bin");

            Generator generator = new Generator(this);
            generator.AddNehnutelnosti(10, 10);
        }

        public string[] FindNehnutelnost(int id)
        {
            NehnutelnostID nehnutelnostID = dynamicHashId.Find(new NehnutelnostID() {Id = id});

            if (nehnutelnostID == null)
                throw new ArgumentException($"Nehnuteľnosť s ID: {id} neexistuje v systéme");

            nehnutelnostID.NacitajNehnutelnost(randomAccessFile);
            return nehnutelnostID.ToStringArray();
        }

        public string[] FindNehnutelnost(int supisneCislo, string nazovKatastralnehoUzemia)
        {
            NehnutelnostSupisneCislo nehnutelnostSupisneCislo = dynamicHashSup.Find(new NehnutelnostSupisneCislo() { SupisneCislo = supisneCislo, NazovKatastra = nazovKatastralnehoUzemia });

            if (nehnutelnostSupisneCislo == null)
                throw new ArgumentException($"Nehnuteľnosť so súspiným číslom: {supisneCislo} a názvom katastrálneho územia: {nazovKatastralnehoUzemia}\nneexistuje v systéme");

            nehnutelnostSupisneCislo.NacitajNehnutelnost(randomAccessFile);
            return nehnutelnostSupisneCislo.ToStringArray();
        }

        public void PridajNehnutelnost(Nehnutelnost nehnutelnost)
        {
            var offset = randomAccessFile.Add(nehnutelnost);
            dynamicHashId.Add(new NehnutelnostID() {Id = nehnutelnost.Id, offset = offset });
            dynamicHashSup.Add(new NehnutelnostSupisneCislo() { SupisneCislo = nehnutelnost.SupisneCislo, NazovKatastra = nehnutelnost.NazovKatastra, offset = offset });
        }
        public void PridajNehnutelnost(string[] nehnutelnostArray)
        {
            Nehnutelnost nehnutelnost = new Nehnutelnost();
            nehnutelnost.FromStringArray(nehnutelnostArray);
            PridajNehnutelnost(nehnutelnost);
        }

        public void OdstranNehnutelnost(int supisneCislo, string nazovKatastra)
        {
            var nehnutelnostSup = dynamicHashSup.Delete(new NehnutelnostSupisneCislo(){SupisneCislo = supisneCislo, NazovKatastra = nazovKatastra});

            if (nehnutelnostSup == null)
                throw new ArgumentException($"Nehnuteľnosť so súspiným číslom: {supisneCislo} a názvom katastrálneho územia: {nazovKatastra}\nneexistuje v systéme");

            var nehnutelnost = randomAccessFile.ReadDataFromFile(nehnutelnostSup.offset);

            dynamicHashId.Delete(new NehnutelnostID()
                {Id = nehnutelnost.Id});

            randomAccessFile._freeBlocks.Add(nehnutelnostSup.offset);
        }

        public void ZmenUdaje(int id, string[] udaje)
        {
            var nehnutelnostId = dynamicHashId.Delete(new NehnutelnostID() { Id = id});

            if (nehnutelnostId == null)
                throw new ArgumentException($"Nehnuteľnosť s ID: {id} neexistuje v systéme");

            var nehnutelnostRAC = randomAccessFile.ReadDataFromFile(nehnutelnostId.offset);
            dynamicHashSup.Delete(new NehnutelnostSupisneCislo() { SupisneCislo = nehnutelnostRAC.SupisneCislo, NazovKatastra = nehnutelnostRAC.NazovKatastra });

            randomAccessFile._freeBlocks.Add(nehnutelnostId.offset);


            Nehnutelnost nehnutelnost = new Nehnutelnost();

            if (udaje[0] == "")
            {
                udaje[0] = nehnutelnostRAC.Id.ToString();
            }
             if (udaje[1] == "")
            {
                udaje[1] = nehnutelnostRAC.SupisneCislo.ToString();
            }
             if (udaje[2] == "")
            {
                udaje[2] = nehnutelnostRAC.NazovKatastra;
            }
             if (udaje[3] == "")
            {
                udaje[3] = nehnutelnostRAC.Popis;
            }

            nehnutelnost.FromStringArray(udaje);

            PridajNehnutelnost(nehnutelnost);
        }
    }
}
