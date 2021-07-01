using GEOBOX.OSC.DisplayModelEditor.DAL;
using GEOBOX.OSC.DisplayModelEditor.FileHandler;
using GEOBOX.OSC.DisplayModelEditor.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace GEOBOX.OSC.DisplayModelEditor.ViewModels
{
    class MergeViewModel
    {
        private readonly string basePath;
        private readonly string path1;
        private readonly string path2;
        private readonly string pathNewFile;

        internal MergeViewModel(string basePath, string path1, string path2, string pathNewFile)
        {
            this.basePath = basePath;
            this.path1 = path1;
            this.path2 = path2;
            this.pathNewFile = pathNewFile;
        }

        internal void Merge()
        {
            var controller1 = new TbdmFileHandler(basePath, path1);
            var controller2 = new TbdmFileHandler(basePath, path2);
            try
            {
                controller1.Read();
                controller2.Read();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Meldung beim lesen einer der TBDM-Dateien:{Environment.NewLine}{ex.Message}", "Hopperla...", MessageBoxButtons.OK);
                return;
            }

            var groups = MergeGroups(controller1, controller2);
            var layers = MergeLayers(controller1, controller2);

            var newController = new TbdmmapLayerHandler();
            newController.AddGroups(groups);
            newController.AddMapLayers(layers);
            newController.CoordSystem = controller1.GetTbdmmapController().GetItems().Select(item => item.CoordSystem).First();
            newController.Units = controller1.GetTbdmmapController().GetItems().Select(item => item.Units).First();

            Save(newController);
        }

        private string messageLogger;
        public string MessageLogger
        {
            get
            {
                return messageLogger;
            }
            set
            {
                messageLogger = value;
                OnPropertyChanged("MessageLogger");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private IEnumerable<TbdmmapItem> MergeLayers(TbdmFileHandler controller1, TbdmFileHandler controller2)
        {
            var list = new List<TbdmmapItem>();

            list.AddRange(GetLayers(controller1));
            list.AddRange(GetLayers(controller2));

            return CheckLayerNames(list);
        }

        private IEnumerable<TbdmmapItem> CheckLayerNames(List<TbdmmapItem> list)
        {
            var grouped = list.GroupBy(item => item.Name).Where(item => item.Count() > 1);
            foreach (var item in grouped)
            {
                foreach (var hans in item.Select(layer => layer))
                {
                    var layerItem = list[list.IndexOf(hans)];
                    if (layerItem != null)
                    {
                        layerItem.Name = $"{layerItem.Name} ({layerItem.Group})";
                    }
                }
            }

            return list;
        }

        private IEnumerable<TbdmmapItem> GetLayers(TbdmFileHandler controller)
        {
            return controller.GetTbdmmapController().GetItems().SelectMany(item => item.GetMapLayers());
        }

        private IEnumerable<LayerGroup> MergeGroups(TbdmFileHandler controller1, TbdmFileHandler controller2)
        {
            var list = new List<LayerGroup>();
            list.AddRange(GetGroups(controller1));
            list.AddRange(GetGroups(controller2));

            return GenerateNewGroupNumbers(list);
        }

        private IEnumerable<LayerGroup> GetGroups(TbdmFileHandler controller)
        {
            return controller.GetTbdmmapController().GetItems().SelectMany(item => item.GetGroups());
        }

        private IEnumerable<LayerGroup> GenerateNewGroupNumbers(IEnumerable<LayerGroup> list)
        {
            int index = 1;
            foreach (LayerGroup item in list.Where(item => item.Group == null))
            {
                item.Order = index;
                index++;
            }

            return list;
        }

        private void Save(TbdmmapLayerHandler layer)
        {
            var writer = new MapXmlWriter();
            writer.WriteXml(pathNewFile, layer);
        }
    }
}