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
using System.IO;
using d88lib;

namespace d88explorer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string currentFileName;
        private VDisk currentVDisk;
        private void loadD88(string fileName)
        {
            try
            {
                currentVDisk = new VDisk(File.ReadAllBytes(fileName));
                currentFileName = fileName;
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.Message);
            }
        }
        private void fillScreen()
        {
            if (currentVDisk == null) return;
            ListBoxMain.Items.Clear();
            foreach (var item in currentVDisk.EnumFiles())
            {
                ListBoxMain.Items.Add(item.FileName);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if( !CmdLnPaser.Parse() )
            {
                MessageBox.Show(this, CmdLnPaser.ErrorMessage);
                return;
            }
            if (string.IsNullOrWhiteSpace(CmdLnPaser.FileName)) return;
            loadD88(CmdLnPaser.FileName);
            fillScreen();
        }
    }
}
