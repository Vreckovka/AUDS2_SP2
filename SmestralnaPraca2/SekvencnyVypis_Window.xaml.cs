using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using DataStructures.DynamicHash;
using SmestralnaPraca2.Core;

namespace SmestralnaPraca2
{
    /// <summary>
    /// Interaction logic for SekvencnyVypis_Window.xaml
    /// </summary>
    public partial class SekvencnyVypis_Window : Window
    {
        private object fileAccessStrucuture;
        private int IDOfStructure;
        /// <summary>
        /// 0 - RAC
        /// 1 - ID, Supisne cislo a nazov
        /// 2 - Supisne cislo a nazov
        /// </summary>
        /// <param name="fileAccessStrucuture"></param>
        /// <param name="IDOfStructure"></param>
        public SekvencnyVypis_Window(object fileAccessStrucuture, int IDOfStructure)
        {
            InitializeComponent();
            this.fileAccessStrucuture = fileAccessStrucuture;
            this.IDOfStructure = IDOfStructure;

            DrawBlocksSequentionally(fileAccessStrucuture, IDOfStructure);
        }

        public void Refresh()
        {
            DrawBlocksSequentionally(fileAccessStrucuture, IDOfStructure);
        }

        public void DrawBlocksSequentionally(object fileAccessStrucuture, int IDOfStructure)
        {
            switch (IDOfStructure)
            {
                case 0:
                    Dispatcher.Invoke(() => { Canvas_Main.Children.Clear(); });

                    var fileAccessStrucutureQueue = ((RandomAccessFile<Nehnutelnost>)fileAccessStrucuture).GetBlocksSequentionally();

                    foreach (KeyValuePair<Nehnutelnost, long> keyValuePair in fileAccessStrucutureQueue)
                    {
                        DrawElement((RandomAccessFile<Nehnutelnost>)fileAccessStrucuture, keyValuePair);
                    }

                    break;

                case 1:
                    Dispatcher.Invoke(() => { Canvas_Main.Children.Clear(); });

                    var queueID = ((DynamicHash<NehnutelnostID>)fileAccessStrucuture).GetBlocksSequentionally();

                    foreach (Block<NehnutelnostID> block in queueID)
                    {
                        DrawBlock(block, IDOfStructure, (DynamicHash<NehnutelnostID>)fileAccessStrucuture);
                    }

                    break;
                case 2:
                    Dispatcher.Invoke(() => { Canvas_Main.Children.Clear(); });

                    var queueSup = ((DynamicHash<NehnutelnostSupisneCislo>)fileAccessStrucuture).GetBlocksSequentionally();

                    foreach (Block<NehnutelnostSupisneCislo> block in queueSup)
                    {
                        DrawBlock(block, IDOfStructure, (DynamicHash<NehnutelnostSupisneCislo>)fileAccessStrucuture);
                    }

                    break;
                default:
                    throw new ArgumentException("Invalid argument");
            }
        }

        private void DrawElement(RandomAccessFile<Nehnutelnost> randomAccessFile, KeyValuePair<Nehnutelnost, long> keyValuePair)
        {
            Dispatcher.Invoke(() =>
            {
                GroupBox block = new GroupBox() { Header = $"Offset: {keyValuePair.Value}" };

                var converter = new System.Windows.Media.BrushConverter();
                var brush = (Brush)converter.ConvertFromString("#7FEEEEEE");

                block.Background = brush;

                StackPanel stackPanel = new StackPanel();
                stackPanel.Children.Add(new TextBlock { Text = $"ID: {keyValuePair.Key.Id}" });
                stackPanel.Children.Add(new TextBlock { Text = $"Supisne cislo: {keyValuePair.Key.SupisneCislo}" });
                stackPanel.Children.Add(new TextBlock { Text = $"Nazov katastra: {keyValuePair.Key.NazovKatastra}" });
                stackPanel.Children.Add(new TextBlock { Text = $"Popis: {keyValuePair.Key.Popis}" });


                block.BorderThickness = new Thickness(2);
                block.BorderBrush = Brushes.Red;
                block.Margin = new Thickness(5);
                block.Content = stackPanel;

                if (randomAccessFile._freeBlocks.Contains(keyValuePair.Value))
                {
                    block.Background = (Brush)converter.ConvertFromString("#66FF8181");
                }

                Canvas_Main.Children.Add(block);
            });
        }


        private void DrawBlock<T>(Block<T> nehnutelnosts, int IDOfStructure, DynamicHash<T> dynamicHash) where T : IHashRecord, IComparable<T>, IByteRecord, new()
        {
            Dispatcher.Invoke(() =>
            {
                GroupBox block = new GroupBox() { Header = $"Offset: {nehnutelnosts.Offset}" };

                var converter = new System.Windows.Media.BrushConverter();
                var brush = (Brush)converter.ConvertFromString("#7FEEEEEE");

                block.Background = brush;

                StackPanel stackPanelBlock = new StackPanel();
                GroupBox header = new GroupBox() { Header = $"Header of block" };


                GroupBox items = new GroupBox() { Header = $"Items" };
                stackPanelBlock.Children.Add(header);
                stackPanelBlock.Children.Add(items);


                StackPanel stackPanelHeader = new StackPanel();
                stackPanelHeader.Children.Add(new TextBlock { Text = $"Valid count: {nehnutelnosts.ValidCount}" });
                stackPanelHeader.Children.Add(new TextBlock { Text = $"Offset of next: {nehnutelnosts.NextOffset}" });
                stackPanelHeader.Children.Add(new TextBlock { Text = $"Chain size: {nehnutelnosts.SizeOfChain}" });
                stackPanelHeader.Children.Add(new TextBlock
                { Text = $"Valid count of chain: {nehnutelnosts.ValidCountOfChain}" });
                header.Content = stackPanelHeader;

                StackPanel stackPanel = new StackPanel();

                block.BorderThickness = new Thickness(2);
                block.BorderBrush = Brushes.Red;
                block.Margin = new Thickness(5);

                items.BorderBrush = Brushes.Black;
                header.BorderBrush = Brushes.Black;
                header.BorderThickness = new Thickness(2);


                foreach (T nehnutelnost in nehnutelnosts.Records)
                {
                    var item = nehnutelnost as Nehnutelnost;
                    StackPanel nehnutelnostStackPanel = new StackPanel();
                    switch (IDOfStructure)
                    {
                        case 1:
                            TextBlock id = new TextBlock() { Text = $"Id: {item.Id.ToString()}" };
                            TextBlock offsetID = new TextBlock() { Text = $"Offset: {((NehnutelnostID)item).offset.ToString()}" };

                            nehnutelnostStackPanel.Children.Add(id);
                            nehnutelnostStackPanel.Children.Add(offsetID);
                            nehnutelnostStackPanel.Children.Add(new Separator());
                            stackPanel.Children.Add(nehnutelnostStackPanel);

                            break;
                        case 2:
                            TextBlock supisneCislo = new TextBlock()
                            { Text = $"Supisne cislo: {item.SupisneCislo}" };
                            TextBlock nazovKatastra = new TextBlock()
                            { Text = $"Nazov katastra: {item.NazovKatastra}" };
                            TextBlock offsetSupp = new TextBlock() { Text = $"Offset: {((NehnutelnostSupisneCislo)item).offset.ToString()}" };

                            nehnutelnostStackPanel.Children.Add(offsetSupp);
                            nehnutelnostStackPanel.Children.Add(supisneCislo);
                            nehnutelnostStackPanel.Children.Add(nazovKatastra);
                            nehnutelnostStackPanel.Children.Add(new Separator());

                            stackPanel.Children.Add(nehnutelnostStackPanel);

                            break;
                    }
                }

                items.Content = stackPanel;
                block.Content = stackPanelBlock;

                if (dynamicHash._freeBlocks.Contains(nehnutelnosts.Offset))
                {
                    block.Background = (Brush)converter.ConvertFromString("#66FF8181");
                    header.BorderThickness = new Thickness(0);
                    items.BorderThickness = new Thickness(0);
                }

                Canvas_Main.Children.Add(block);
            });
        }
    }
}
