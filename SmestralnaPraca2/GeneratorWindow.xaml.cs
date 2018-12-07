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
using SmestralnaPraca2.Core;

namespace SmestralnaPraca2
{
    /// <summary>
    /// Interaction logic for GeneratorWindow.xaml
    /// </summary>
    public partial class GeneratorWindow : Window
    {
        private CoreMain coreMain;
        public GeneratorWindow(CoreMain coreMain)
        {
            InitializeComponent();
            this.coreMain = coreMain;
        }

        private void GenerujBUtt_Click(object sender, RoutedEventArgs e)
        {
            Generator generator = new Generator(coreMain);
            generator.AddNehnutelnosti(Convert.ToInt32(PocetKatastrov.Text), Convert.ToInt32(PocetNehnutelnosti.Text));
            MessageBox.Show("Údaje boli úspešne vygenerované");
        }
    }
}
