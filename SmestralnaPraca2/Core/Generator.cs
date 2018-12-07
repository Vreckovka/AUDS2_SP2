using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmestralnaPraca2.Core
{
    class Generator
    {
        CoreMain coreMain;
        static Random random = new Random(4);

        public Generator(CoreMain coreMain)
        {
            this.coreMain = coreMain;
        }


        private static string[] mesta = new string[]
          {"Chynorany", "Topolcany", "Prievidza", "Kosice", "Ruzomberok", "Brezno", "Zilina", "Bosany", "Pezinok", "Bratislava", "Kosice"};
        public void AddNehnutelnosti(int pocetNehnutelnost, int pocetKatastrov)
        {
            for (int i = 0; i < pocetKatastrov; i++)
            {
                for (int j = 0; j < pocetNehnutelnost; j++)
                {
                    Nehnutelnost nehnutelnost = new Nehnutelnost(j,
                        mesta[i], "POPIS: " + RandomString(5), coreMain.randomAccessFile.Count);

                    coreMain.PridajNehnutelnost(nehnutelnost);
                }
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
