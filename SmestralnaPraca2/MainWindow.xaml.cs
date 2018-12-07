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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SmestralnaPraca2.Core;


namespace SmestralnaPraca2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CoreMain coreMain = new CoreMain();
        private SekvencnyVypis_Window sekvencnyVypis_WindowRAC;
        private SekvencnyVypis_Window sekvencnyVypis_WindowID;
        private SekvencnyVypis_Window sekvencnyVypis_WindowSUP;
        private GeneratorWindow generatorWindow;
        string[] VyhladanaNehnutelnost;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SekvencnyVypisID_Click(object sender, RoutedEventArgs e)
        {
            if (sekvencnyVypis_WindowID == null || !sekvencnyVypis_WindowID.IsLoaded)
                sekvencnyVypis_WindowID = new SekvencnyVypis_Window(coreMain.dynamicHashId, 1);
            sekvencnyVypis_WindowID.Show();
        }

        private void SekvencnyVypisSupCislo_Click(object sender, RoutedEventArgs e)
        {
            if (sekvencnyVypis_WindowSUP == null || !sekvencnyVypis_WindowSUP.IsActive)
                sekvencnyVypis_WindowSUP = new SekvencnyVypis_Window(coreMain.dynamicHashSup, 2);
            sekvencnyVypis_WindowSUP.Show();
        }

        private void SekvencnyVypisRAC_Click(object sender, RoutedEventArgs e)
        {
            if (sekvencnyVypis_WindowRAC == null || !sekvencnyVypis_WindowRAC.IsActive)
                sekvencnyVypis_WindowRAC = new SekvencnyVypis_Window(coreMain.randomAccessFile, 0);
            sekvencnyVypis_WindowRAC.Show();
        }

        private void VyhladajNehnutelnost()
        {
            try
            {
                if (TextBox_NazovKatastra.Text == "" && TextBox_SupisneCislo.Text == "")
                {
                    VyhladanaNehnutelnost = coreMain.FindNehnutelnost(Convert.ToInt32(TextBox_ID.Text));
                }

                else if (TextBox_NazovKatastra.Text != "" && TextBox_SupisneCislo.Text != "" && TextBox_ID.Text == "")
                {
                    VyhladanaNehnutelnost = coreMain.FindNehnutelnost(Convert.ToInt32(TextBox_SupisneCislo.Text),
                        TextBox_NazovKatastra.Text);
                }
                else
                    throw new Exception(
                        "Nehnuteľnost je možné vyhladávať s ID, alebo s kombináciou supísného čísla a názvu katastrálneho územia");

                Content_VyhladanaNehnutelnost.Content = VyhladanaNehnutelnost;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void Button_Vyhaladaj_Click(object sender, RoutedEventArgs e)
        {
            VyhladajNehnutelnost();
        }

        private void RefreshSekevencneVypisy()
        {
            if (sekvencnyVypis_WindowID != null && sekvencnyVypis_WindowID.IsLoaded)
            {
                sekvencnyVypis_WindowID.Refresh();
            }

            if (sekvencnyVypis_WindowSUP != null && sekvencnyVypis_WindowSUP.IsLoaded)
            {
                sekvencnyVypis_WindowSUP.Refresh();
            }

            if (sekvencnyVypis_WindowRAC != null && sekvencnyVypis_WindowRAC.IsLoaded)
            {
                sekvencnyVypis_WindowRAC.Refresh();
            }
        }

        private void Button_Pridaj_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TextBox_IDZmena.Text != "" && TextBox_KatZmena.Text != "" && TextBox_PopisZmena.Text != "" &&
                    TextBox_SupZmena.Text != "")
                {
                    string[] nehnutelnost = new string[4];
                    nehnutelnost[0] = TextBox_IDZmena.Text;
                    nehnutelnost[1] = TextBox_SupZmena.Text;
                    nehnutelnost[2] = TextBox_KatZmena.Text;
                    nehnutelnost[3] = TextBox_PopisZmena.Text;

                    coreMain.PridajNehnutelnost(nehnutelnost);

                    RefreshSekevencneVypisy();
                    MessageBox.Show("Nehnutelnosť bola úspešne pridaná");
                }
                else
                    throw new Exception("Neboli zadané všetky údaje");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Zmen_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string[] nehnutelnost = new string[4];
                nehnutelnost[0] = TextBox_IDZmena.Text;
                nehnutelnost[1] = TextBox_SupZmena.Text;
                nehnutelnost[2] = TextBox_KatZmena.Text;
                nehnutelnost[3] = TextBox_PopisZmena.Text;

                coreMain.ZmenUdaje(Convert.ToInt32(VyhladanaNehnutelnost[0]), nehnutelnost);

                VyhladanaNehnutelnost = coreMain.FindNehnutelnost(Convert.ToInt32(nehnutelnost[0]));
                Content_VyhladanaNehnutelnost.Content = VyhladanaNehnutelnost;

                RefreshSekevencneVypisy();
                MessageBox.Show("Nehnutelnosť bola úspešne zmenená");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Odstan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VyhladanaNehnutelnost == null)
                    throw new Exception("Nebola vyhladaná žiadna nehnuteľnosť");

                coreMain.OdstranNehnutelnost(Convert.ToInt32(VyhladanaNehnutelnost[1]), VyhladanaNehnutelnost[2]);
                RefreshSekevencneVypisy();

                VyhladanaNehnutelnost = null;
                Content_VyhladanaNehnutelnost.Content = null;
                MessageBox.Show("Nehnuteľnosť bola úspešne odstránená");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            coreMain.SaveFiles();
            sekvencnyVypis_WindowID?.Close();
            sekvencnyVypis_WindowRAC?.Close();
            sekvencnyVypis_WindowSUP?.Close();
            generatorWindow?.Close();
        }

        private void VygenerujUdaje_Click(object sender, RoutedEventArgs e)
        {
            if (generatorWindow == null || !generatorWindow.IsLoaded)
                generatorWindow = new GeneratorWindow(coreMain);
            generatorWindow.Show();
        }

        private void NacitajSubor_Click(object sender, RoutedEventArgs e)
        {
            coreMain.LoadFiles();
        }

        private void NacitajSuborNovy_Click(object sender, RoutedEventArgs e)
        {
            coreMain.CreateFiles();
        }
    }
}
