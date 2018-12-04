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

            dynamicHashSup.GetBlocksSequentionallyConsole();
             var pom = dynamicHashSup.Find(new NehnutelnostSupisneCislo() { SupisneCislo = 0, NazovKatastra = "Chynorany" });

            //dynamicHashSup.preOrder();
           // pom.NacitajNehnutelnost(randomAccessFile);
           // pom.VypisNehnutelnost();
            //randomAccessFile.GetBlocksSequentionallyConsole();

        }
    }
}
