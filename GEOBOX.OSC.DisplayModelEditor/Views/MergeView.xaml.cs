using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GEOBOX.OSC.DisplayModelEditor.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GEOBOX.OSC.DisplayModelEditor.Views
{
    /// <summary>
    /// Interaction logic for JoinDisplayModelsView.xaml
    /// </summary>
    public partial class JoinDisplayModelsView : UserControl
    {
        private MergeViewModel filesMergeViewModel; 
        private string rootLayerPath;
        private string tbdmFilePathOne;
        private string tbdmFilePathTwo;
        private string saveNewMergedFilePath;

        public JoinDisplayModelsView()
        {
            InitializeComponent();
            rootLayerPath = Properties.Settings.Default.DMRootPath;
        }

        private void DisplayModelOneBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            TbdmFilePathOne.Text = "";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "DisplayModel|*.tbdm";
            openFileDialog.InitialDirectory = !string.IsNullOrEmpty(rootLayerPath) ? rootLayerPath : "C:";

            if (openFileDialog.ShowDialog() == true)
            {
                TbdmFilePathOne.Text = openFileDialog.FileName;
            }
            else
            {
                MessageBox.Show("Kein Darstellungsmodell ausgewählt, Dialog abgebrochen", "Abbruch");
            }
        }

        private void DisplayModelTwoBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            TbdmFilePathTwo.Text = "";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "DisplayModel|*.tbdm";
            openFileDialog.InitialDirectory = !string.IsNullOrEmpty(rootLayerPath) ? rootLayerPath : "C:";

            if (openFileDialog.ShowDialog() == true)
            {
                TbdmFilePathTwo.Text = openFileDialog.FileName;
            }
            else
            {
                MessageBox.Show("Kein Darstellungsmodell ausgewählt, Dialog abgebrochen", "Abbruch");
            }
        }

        private void MergeFiles_Click(object sender, RoutedEventArgs e)
        {
            if (FilesSelectedForMerge() && FilesExist() && FilePathForMergeFileSelected())
            {
                tbdmFilePathOne = TbdmFilePathOne.Text;
                tbdmFilePathTwo = TbdmFilePathTwo.Text;
                saveNewMergedFilePath = SaveMergedFilePath.Text;

                filesMergeViewModel = new MergeViewModel(rootLayerPath, tbdmFilePathOne, tbdmFilePathTwo, saveNewMergedFilePath);
                filesMergeViewModel.Merge();
                MessageBox.Show("Das Zusammenführen der Dateien war erfolgreich", "Erfolg");

                string pathOfSavedMergedFile = Path.GetDirectoryName(saveNewMergedFilePath);
                Process.Start($@"{pathOfSavedMergedFile}");

                string previousMessageLogs = MessageLoggerRun.Text;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Zusammenführung der Dateien:");
                sb.AppendLine($"{TbdmFilePathOne.Text}");
                sb.AppendLine($"{TbdmFilePathTwo.Text}");
                sb.AppendLine($"Resultat: {saveNewMergedFilePath}");
                sb.AppendLine();
                sb.AppendLine();

                MessageLoggerRun.Text = sb.ToString();
                MessageLoggerRun.Text += previousMessageLogs;
            }
        }

        private bool FilesSelectedForMerge()
        {
            if (string.IsNullOrEmpty(TbdmFilePathOne.Text) || string.IsNullOrEmpty(TbdmFilePathTwo.Text))
            {
                MessageBox.Show("Die Dateien für das Zusammenführen müssen ausgewählt werden", "Fehler");
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool FilesExist()
        {
            if(!File.Exists(TbdmFilePathOne.Text))
            {
                MessageBox.Show("Die erste ausgewählte Datei für das Zusammenführen existiert nicht", "Fehler");
                return false;
            }
            else if (!File.Exists(TbdmFilePathTwo.Text))
            {
                MessageBox.Show("Die zweite ausgewählte Datei für das Zusammenführen existiert nicht", "Fehler");
                return false;
            }
            else
            {
                return true;
            }
        }

        private void ClearDisplayModelInputFields_Click(object sender, RoutedEventArgs e)
        {
            TbdmFilePathOne.Text = "- kein Darstellungsmodell geöffnet";
            TbdmFilePathTwo.Text = "- kein Darstellungsmodell geöffnet";
        }

        private void SelectMergedFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialogNewFile = new SaveFileDialog();
            dialogNewFile.AddExtension = true;
            dialogNewFile.Filter = "TBDM Files | *.tbdm";
            if (dialogNewFile.ShowDialog() == true)
            {
                SaveMergedFilePath.Text = dialogNewFile.FileName;
                return;
            }
        }

        private bool FilePathForMergeFileSelected()
        {
            if(!string.IsNullOrEmpty(SaveMergedFilePath.Text))
            {
                string saveNewFileDirectoryPath = Path.GetDirectoryName(SaveMergedFilePath.Text);
                if (!Directory.Exists(saveNewFileDirectoryPath))
                {
                    MessageBox.Show("Der ausgewählte Speicherort existiert nicht", "Fehler");
                    return false;
                }

                if (string.IsNullOrEmpty(Path.GetFileName(SaveMergedFilePath.Text)))
                {
                    MessageBox.Show("Im ausgewählten Speicherort muss zusätzlich einen Dateinamen beinhalten", "Fehler");
                    return false;
                }

                return true;
            }
            else
            {
                MessageBox.Show("Ein Speicherort muss zwingend für die Resultat-Datei ausgewählt werden", "Fehler");
                return false;
            }
        }
    }
}