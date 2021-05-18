using GEOBOX.OSC.DisplayModelEditor.DAL;
using GEOBOX.OSC.DisplayModelEditor.ViewModels;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GEOBOX.OSC.DisplayModelEditor.Views
{
    /// <summary>
    /// Interaction logic for UserControl.xaml
    /// </summary>
    public partial class OneClickMaintenanceView : UserControl
    {
        private OneClickViewModel oneClickMaintenanceInfoViewModel;
        private string tbdmFilePath;

        public OneClickMaintenanceView()
        {
            oneClickMaintenanceInfoViewModel = new OneClickViewModel();
            DataContext = oneClickMaintenanceInfoViewModel;
            
            InitializeComponent();
            RootLayerPath.Text = Properties.Settings.Default.DMRootPath;
        }
        
        private void DisplayModelBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            TbdmFilePath.Text = "";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "DisplayModel|*.tbdm";
            openFileDialog.InitialDirectory = !string.IsNullOrEmpty(RootLayerPath.Text) ? RootLayerPath.Text : "C:";

            if (openFileDialog.ShowDialog() == true)
            {
                tbdmFilePath = openFileDialog.FileName;
                TbdmFilePath.Text = tbdmFilePath;
                ReadDataAndShowInfos();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Kein Darstellungsmodell ausgewählt, Dialog abgebrochen", "Abbruch");
            }
        }

        private void SelectRepoPathButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fldrDlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (fldrDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    RootLayerPath.Text = fldrDlg.SelectedPath;
                }
            }
        }

        private void ReadDataAndShowInfos()
        {
            oneClickMaintenanceInfoViewModel.BasePath = RootLayerPath.Text;
            oneClickMaintenanceInfoViewModel.TbdmFilePath = tbdmFilePath;
            oneClickMaintenanceInfoViewModel.ReadTbdmFile();
        }

        private void OneClickRunButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Task task in OneClickListView.Items)
            {
                if (task.IsActive)
                {
                    oneClickMaintenanceInfoViewModel.GetTbdmmapController().Run1ClickTask(task.Tag.ToString(), task.FileName);
                    task.IsFixed = true;
                }
            }
        }

        private void CleanComparedFilesButton_Click(object sender, RoutedEventArgs e)
        {
            oneClickMaintenanceInfoViewModel.DeleteSelectedLayers();
        }

        private void Image_IconImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            string dummyIcon = $"/GEOBOX.OSC.DisplayModelEditor;component/Includes/packageDummy128.png";

            ((Image)sender).Source = new BitmapImage(new System.Uri(dummyIcon, System.UriKind.Relative));
        }

        private void RefreshFileResultButton_Click(object sender, RoutedEventArgs e)
        {
            string basePath = oneClickMaintenanceInfoViewModel.BasePath;
            string tbdmFilePath = oneClickMaintenanceInfoViewModel.TbdmFilePath;

            if (!string.IsNullOrEmpty(basePath) && !string.IsNullOrEmpty(tbdmFilePath))
            {
                oneClickMaintenanceInfoViewModel.ReadTbdmFile();
            }
        }

        private void CreateCsvButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "Export"; // Default file name
            saveFileDialog.DefaultExt = ".csv"; // Default file extension
            Nullable<bool> result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                oneClickMaintenanceInfoViewModel.CreateCsvFile(saveFileDialog.FileName);
            }
        }

        private void SelectAllImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            oneClickMaintenanceInfoViewModel.SelectAllTasksInOneClickListView();
        }

        private void DeselectAllImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            oneClickMaintenanceInfoViewModel.DeselectAllTasksInOneClickListView();
        }

        private void OneClickListViewSort_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;

            if (headerClicked != null && headerClicked.Column.DisplayMemberBinding != null)
            {
                oneClickMaintenanceInfoViewModel.Sort(((Binding)headerClicked.Column.DisplayMemberBinding).Path.Path);
            }
        }
    }
}
