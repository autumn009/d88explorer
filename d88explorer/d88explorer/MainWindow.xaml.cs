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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var bin = new VDisk(File.ReadAllBytes(@"N:\delme\n-sui001.d88"));
            foreach (var item in bin.EnumFiles())
            {
                ListBoxMain.Items.Add(item.FileName);
            }
            //var bin2 = new VDisk(File.ReadAllBytes(@"N:\oldFDs\pc8001\gamepack1.d88"));
            //foreach (var item in bin2.EnumFiles())
            //{
            //ListBoxMain.Items.Add(item.FileName);
            //}
        }
    }
}
