using GEOBOX.OSC.DisplayModelEditor.DAL;
using GEOBOX.OSC.DisplayModelEditor.FileHandler;
using GEOBOX.OSC.DisplayModelEditor.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Forms;

namespace GEOBOX.OSC.DisplayModelEditor.ViewModels
{
    public sealed class OneClickViewModel : INotifyPropertyChanged
    {
        private SettingsController settingsController = new SettingsController();
        private TbdmFileHandler tbdmFileHandler;
        private List<MissingLayer> layersToDelete;
        private string sortColumnName = string.Empty;
        private ListSortDirection sortDirection;
        public ICollectionView MaintenanceTasksView { get; private set; }

        private ObservableCollection<Task> oneClickTasks;
        public ObservableCollection<Task> OneClickTasks
        {
            get
            {
                return oneClickTasks;
            }
            private set
            {
                oneClickTasks = value;
                OnPropertyChanged("OneClickTasks");
            }
        }

        private ObservableCollection<MissingLayer> comparedFiles;
        public ObservableCollection<MissingLayer> ComparedFiles
        {
            get
            {
                return comparedFiles;
            }
            private set
            {
                comparedFiles = value;
                OnPropertyChanged("ComparedFiles");
            }
        }

        private string basePath;
        public string BasePath
        {
            get
            {
                return basePath;
            }
            set
            {
                basePath = value;
                OnPropertyChanged("BasePath");
            }
        }

        private string tbdmFileName;
        public string TbdmFileName
        {
            get
            {
                return tbdmFileName;
            }
            set
            {
                tbdmFileName = value;
                OnPropertyChanged("TbdmFileName");
            }
        }

        private string tbdmFilePath;
        public string TbdmFilePath
        {
            get
            {
                return tbdmFilePath;
            }
            set
            {
                tbdmFilePath = value;
                OnPropertyChanged("TbdmFilePath");
            }
        }

        private string tbdmmapFileName;
        public string TbdmmapFileName
        {
            get
            {
                return tbdmmapFileName;
            }
            set
            {
                tbdmmapFileName = value;
                OnPropertyChanged("TbdmmapFileName");
            }
        }

        private string countGroup;
        public string CountGroup
        {
            get
            {
                return countGroup;
            }
            set
            {
                countGroup = value;
                OnPropertyChanged("CountGroup");
            }
        }

        private string countLayer;
        public string CountLayer
        {
            get
            {
                return countLayer;
            }
            set
            {
                countLayer = value;
                OnPropertyChanged("CountLayer");
            }
        }

        private string coordSys;
        public string CoordSys
        {
            get
            {
                return coordSys;
            }
            set
            {
                coordSys = value;
                OnPropertyChanged("CoordSys");
            }
        }

        private string unitsValue;
        public string UnitsValue
        {
            get
            {
                return unitsValue;
            }
            set
            {
                unitsValue = value;
                OnPropertyChanged("UnitsValue");
            }
        }

        private string compareCount;
        public string CompareCount
        {
            get
            {
                return compareCount;
            }
            set
            {
                compareCount = value;
                OnPropertyChanged("CompareCount");
            }
        }

        private bool checkOnlyTbdmmap;
        public bool CheckOnlyTbdmmap
        {
            get
            {
                return checkOnlyTbdmmap;
            }
            set
            {
                checkOnlyTbdmmap = value;
                OnPropertyChanged("CheckOnlyTbdmmap");
            }
        }

        private Button oneClickRunButton;
        public Button OneClickRunButton
        {
            get
            {
                return oneClickRunButton;
            }
            set
            {
                oneClickRunButton = value;
                OnPropertyChanged("OneClickRunButton");
            }
        }

        private Button removeMissingLayersButton;
        public Button RemoveMissingLayersButton
        {
            get
            {
                return removeMissingLayersButton;
            }
            set
            {
                removeMissingLayersButton = value;
                OnPropertyChanged("RemoveMissingLayersButton");
            }
        }

        public void ReadTbdmFile()
        {
            try
            {
                tbdmFileHandler = new TbdmFileHandler(BasePath, TbdmFilePath);
                tbdmFileHandler.Read();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Meldung beim lesen der TBDM-Datei:{Environment.NewLine}{ex.Message}", "Hopperla...", MessageBoxButtons.OK);
                return;
            }

            InsertTasks(tbdmFileHandler.GetTbdmmapController());

            TbdmFileName = Path.GetFileName(TbdmFilePath);
            TbdmmapFileName = tbdmFileHandler.GetTbdmmapItem().GetName();
            UnitsValue = tbdmFileHandler.GetTbdmmapController().GetItems().Select(item => item.Units).FirstOrDefault();
            CoordSys = tbdmFileHandler.GetTbdmmapController().GetItems().Select(item => item.CoordSystem).FirstOrDefault();
            CountGroup = tbdmFileHandler.GetTbdmmapController().GetItems().SelectMany(item => item.GetGroups()).Count(item => item.Group != null).ToString();
            CountLayer = tbdmFileHandler.GetTbdmmapController().GetItems().SelectMany(item => item.GetMapLayers()).Count().ToString();

            if (!CheckOnlyTbdmmap)
            {
                ComparedFiles = new ObservableCollection<MissingLayer>();

                foreach (MissingLayer missingLayer in MissingLayerHandler.GetMissingLayers())
                {
                    if (missingLayer.LayerTag != MissingLayer.Tag.Folder)
                    {
                        missingLayer.IsActive = true;
                    }
                    ComparedFiles.Add(missingLayer);
                }
                CompareCount = MissingLayerHandler.GetCount().ToString();
                SetButtonState();
            }
        }
        
        public void CreateCsvFile(string filename)
        {
            tbdmFileHandler.GetTbdmmapController().CreateCsv(filename);
        }

        public void SelectAllTasksInOneClickListView()
        {
            if(OneClickTasks != null)
            {
                foreach (var oneClickTask in OneClickTasks)
                {
                    if (oneClickTask.isEnabled)
                    {
                        oneClickTask.IsActive = true;
                    }
                }
            }
        }

        public void DeselectAllTasksInOneClickListView()
        {
            if (OneClickTasks != null)
            {
                foreach (var oneClickTask in OneClickTasks)
                {
                    oneClickTask.IsActive = false;
                }
            }
        }

        public void Sort(string columnName)
        {
            if (sortColumnName == columnName)
            {
                if (sortDirection == ListSortDirection.Ascending)
                {
                    sortDirection = ListSortDirection.Descending;
                }
                else
                {
                    sortDirection = ListSortDirection.Ascending;
                }
            }
            else
            {
                sortColumnName = columnName;
                sortDirection = ListSortDirection.Ascending;
            }

            MaintenanceTasksView.SortDescriptions.Clear();
            SortDescription sortDescription = new SortDescription(sortColumnName, sortDirection);
            MaintenanceTasksView.SortDescriptions.Add(sortDescription);
            MaintenanceTasksView.Refresh();
            OnPropertyChanged("MaintenanceTasksView");
        }

        private void SetButtonState()
        {
            OneClickRunButton = new Button();
            removeMissingLayersButton = new Button();

            if (MissingLayerHandler.GetCount() > 0 && !CheckOnlyTbdmmap)
            {
                OneClickRunButton.Enabled = false;
                removeMissingLayersButton.Enabled = true;
            }
            else
            {
                OneClickRunButton.Enabled = true;
                removeMissingLayersButton.Enabled = false;
            }
        }

        private void InsertTasks(TbdmmapFileHandler controller)
        {
            OneClickTasks = new ObservableCollection<Task>();

            var tasklist = controller.GetItems();
            foreach (var tasks in tasklist)
            {
                foreach (Task task in tasks.GetTasks())
                {
                    oneClickTasks.Add(task);
                }
            }

            if (!CheckOnlyTbdmmap)
            {
                var layertasks = controller.GetLayerController().GetLayerTasks();
                foreach (Task task in layertasks)
                {
                    oneClickTasks.Add(task);
                }
            }
            MaintenanceTasksView = CollectionViewSource.GetDefaultView(OneClickTasks);
        }
        
        public TbdmmapFileHandler GetTbdmmapController()
        {
            return tbdmFileHandler.GetTbdmmapController();
        }

        public void DeleteSelectedLayers()
        {
            layersToDelete = new List<MissingLayer>();
            foreach (MissingLayer missingLayer in ComparedFiles)
            {
                if (missingLayer.IsActive)
                {
                    tbdmFileHandler.DeleteLayer(missingLayer);
                    layersToDelete.Add(missingLayer);
                }
            }
            RemoveLayersToDelete();
        }

        private void RemoveLayersToDelete()
        {
            foreach(MissingLayer missingLayer in layersToDelete)
            {
                ComparedFiles.Remove(missingLayer);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}