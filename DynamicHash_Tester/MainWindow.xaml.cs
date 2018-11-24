using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataStructures.DynamicHash;

namespace DynamicHash_Tester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Random random = new Random(1);
        static Stopwatch stopwatch = new Stopwatch();
        private static List<Nehnutelnost> nehnutelnosts = new List<Nehnutelnost>();
        private static DynamicHash<Nehnutelnost> dynamicHash;
        public MainWindow()
        {
            InitializeComponent();

            dynamicHash = new DynamicHash<Nehnutelnost>(3, "Nehnutelnosti.bin");
            int count = 10;
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < count; i++)
            {
                RandomInsert();
            }
            stopwatch.Stop();

            Console.WriteLine($"Added {count:N0} in: {stopwatch.Elapsed}");
           

            Console.WriteLine(dynamicHash.Delete(nehnutelnosts[8]));
            Console.WriteLine(dynamicHash.Delete(nehnutelnosts[9]));

            DrawBlocksSequentionally();
        }

        private static string[] mesta = new string[]
            {"Chynorany", "Topolcany", "Prievidza", "Kosice", "Ruzomberok", "Brezno", "Zilina"};
        public static void RandomInsert()
        {
            int supCislo = random.Next(0, 1000);
            Nehnutelnost nehnutelnost = new Nehnutelnost(supCislo, mesta[random.Next(0, mesta.Length)], "POPIS: " + supCislo, nehnutelnosts.Count);

            dynamicHash.Add(nehnutelnost);
            nehnutelnosts.Add(nehnutelnost);
        }

        public void DrawBlocksSequentionally()
        {
            var queue = dynamicHash.GetBlocksSequentionally();
            foreach (Block<Nehnutelnost> block in queue)
            {
                DrawBlock(block);
            }
        }

        private void DrawBlock(Block<Nehnutelnost> nehnutelnosts)
        {
            GroupBox block = new GroupBox() { Header = $"Offset: {nehnutelnosts.Offset}" };

            StackPanel stackPanelBlock = new StackPanel();
            GroupBox header = new GroupBox() { Header = $"Header of block" };

            GroupBox items = new GroupBox() { Header = $"Items" };
            stackPanelBlock.Children.Add(header);
            stackPanelBlock.Children.Add(items);


            StackPanel stackPanelHeader = new StackPanel();
            stackPanelHeader.Children.Add(new TextBlock { Text = $"Valid count: {nehnutelnosts.ValidCount}" });
            stackPanelHeader.Children.Add(new TextBlock { Text = $"Offset of next: {nehnutelnosts.NextOffset}" });
            stackPanelHeader.Children.Add(new TextBlock { Text = $"Chain size: {nehnutelnosts.SizeOfChain}" });
            stackPanelHeader.Children.Add(new TextBlock { Text = $"Valid count of chain: {nehnutelnosts.ValidCountOfChain}" });
            header.Content = stackPanelHeader;

            StackPanel stackPanel = new StackPanel();

            block.BorderThickness = new Thickness(2);
            block.BorderBrush = Brushes.Red;
            block.Margin = new Thickness(5);

            items.BorderBrush = Brushes.Black;
            header.BorderBrush = Brushes.Black;
            header.BorderThickness = new Thickness(2);


            foreach (Nehnutelnost nehnutelnost in nehnutelnosts.Records)
            {
                StackPanel nehnutelnostStackPanel = new StackPanel();
                TextBlock id = new TextBlock() { Text = $"Id: {nehnutelnost.Id.ToString()}" };
                TextBlock supisneCislo = new TextBlock() { Text = $"Supisne cislo: {nehnutelnost.SupisneCislo}" };
                TextBlock nazovKatastra = new TextBlock() { Text = $"Nazov katastra: {nehnutelnost.NazovKatastra}" };
                TextBlock popis = new TextBlock() { Text = $"Popis: {nehnutelnost.Popis}" };

                nehnutelnostStackPanel.Children.Add(id);
                nehnutelnostStackPanel.Children.Add(supisneCislo);
                nehnutelnostStackPanel.Children.Add(nazovKatastra);
                nehnutelnostStackPanel.Children.Add(popis);
                nehnutelnostStackPanel.Children.Add(new Separator());

                stackPanel.Children.Add(nehnutelnostStackPanel);
            }

            items.Content = stackPanel;

            block.Content = stackPanelBlock;
            Canvas_Main.Children.Add(block);
        }
    }
}
