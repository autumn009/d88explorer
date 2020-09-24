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
        private TempDir currentTempDir;
        private FileDetails[] currentImage;
        private void loadD88(string fileName)
        {
#if false
            try
            {
#endif
            currentVDisk = new VDisk(File.ReadAllBytes(fileName));
                currentFileName = fileName;
                currentTempDir = new TempDir();
                currentImage = currentTempDir.CreateImage(currentVDisk);
#if false
            }
            catch (Exception e)
            {
                currentFileName = null;
                currentVDisk = null;
                currentTempDir = null;
                currentImage = null;
                MessageBox.Show(this, e.Message);
            }
#endif
            }
        private void fillScreen()
        {
            if (currentImage == null) return;
            WrapPanelMain.Children.Clear();
            foreach (var item in currentImage)
            {
                var checkbox = new CheckBox();
                checkbox.Content = item.OriginalEntry.FileName;
                checkbox.MouseDown += (sender, e) =>
                {
                    string[] files = { item.HostFileSystemFullPath };
                    var dataObject = new DataObject(DataFormats.FileDrop, files.ToArray());
                    dataObject.SetData(DataFormats.StringFormat, dataObject);
                    DragDrop.DoDragDrop(checkbox, dataObject, DragDropEffects.Copy);
                };
                WrapPanelMain.Children.Add(checkbox);
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

        private void checkAllCommon(bool mode)
        {
            foreach (var item in WrapPanelMain.Children)
            {
                CheckBox ch = item as CheckBox;
                if (ch != null) ch.IsChecked = mode;
            }
        }

        private void ButtonSelectAll_Click(object sender, RoutedEventArgs e) => checkAllCommon(true);
        private void ButtonDeselectAll_Click(object sender, RoutedEventArgs e) => checkAllCommon(false);
        private void ButtonCopyToFolder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonShowImageFolder_Click(object sender, RoutedEventArgs e) => currentTempDir.ShowTempFolder();

        private void Window_Closed(object sender, EventArgs e)
        {
            currentTempDir.ClearImage();
        }
    }
}
