using GEOBOX.OSC.DisplayModelEditor.DAL;
using GEOBOX.OSC.DisplayModelEditor.FileHandler;
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
        private TbdmFileHandler tbdmFileHandler;
        private List<MissingLayer> layersToDelete;
        private string sortColumnName = string.Empty;
        private ListSortDirection sortDirection;
        public ICollectionView MaintenanceTasksView { get; private set; }

        private ObservableCollection<Check> executedChecks = new ObservableCollection<Check>();
        public ObservableCollection<Check> ExecutedChecks
        {
            get
            {
                return executedChecks;
            }
            private set
            {
                executedChecks = value;
                OnPropertyChanged(nameof(ExecutedChecks));
            }
        }

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
                OnPropertyChanged(nameof(OneClickTasks));
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
                OnPropertyChanged(nameof(ComparedFiles));
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
                OnPropertyChanged(nameof(BasePath));
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
                OnPropertyChanged(nameof(TbdmFileName));
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
                OnPropertyChanged(nameof(TbdmFilePath));
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
                OnPropertyChanged(nameof(TbdmmapFileName));
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
                OnPropertyChanged(nameof(CountGroup));
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
                OnPropertyChanged(nameof(CountLayer));
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
                OnPropertyChanged(nameof(CoordSys));
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
                OnPropertyChanged(nameof(UnitsValue));
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
                OnPropertyChanged(nameof(CompareCount));
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
                OnPropertyChanged(nameof(CheckOnlyTbdmmap));
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
                OnPropertyChanged(nameof(OneClickRunButton));
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
                OnPropertyChanged(nameof(RemoveMissingLayersButton));
            }
        }

        public void ReadTbdmFile()
        {
            ExecutedChecks.Clear();
            try
            {
                tbdmFileHandler = new TbdmFileHandler(BasePath, TbdmFilePath);
                tbdmFileHandler.Read(ExecutedChecks);
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
            CountGroup = tbdmFileHandler.GetTbdmmapController().GetItems().SelectMany(item => item.GetGroups()).Count().ToString();
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
            OnPropertyChanged(nameof(MaintenanceTasksView));
        }

        private void SetButtonState()
        {
            OneClickRunButton = new Button();
            RemoveMissingLayersButton = new Button();

            if (MissingLayerHandler.GetCount() > 0 && !CheckOnlyTbdmmap)
            {
                OneClickRunButton.Enabled = false;
                RemoveMissingLayersButton.Enabled = true;
            }
            else
            {
                OneClickRunButton.Enabled = true;
                RemoveMissingLayersButton.Enabled = false;
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

        internal void UpdateCheckCollection(string taskKey)
        {
            foreach (Check check in ExecutedChecks)
            {
                if (check.TaskKeys.Contains(taskKey))
                {
                    check.RemoveTaskKey(taskKey);
                }
            }
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