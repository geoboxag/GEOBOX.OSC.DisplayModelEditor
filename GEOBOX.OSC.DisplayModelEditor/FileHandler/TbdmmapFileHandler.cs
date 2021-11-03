using GEOBOX.OSC.DisplayModelEditor.DAL;
using GEOBOX.OSC.DisplayModelEditor.IO;
using GEOBOX.OSC.DisplayModelEditor.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace GEOBOX.OSC.DisplayModelEditor.FileHandler
{
    public class TbdmmapFileHandler
    {
        private List<TbdmmapLayerHandler> list = new List<TbdmmapLayerHandler>();
        private LayerHandler controller;
        private readonly string basePath;

        public TbdmmapFileHandler(string basePath)
        {
            this.basePath = basePath;
        }

        internal IEnumerable<TbdmmapLayerHandler> GetItems()
        {
            return list;
        }

        internal void Load(string path, ICollection<Check> executedChecks)
        {
            LoadTbdmmap(path, executedChecks);
            LoadLayers(executedChecks);
        }

        private void LoadTbdmmap(string path, ICollection<Check> executedChecks)
        {
            list.Clear();
            string filePath = Path.Combine(basePath, path);
            var reader = new TbdmmapReader();
            var maplayer = reader.ReadTbdmmap(filePath, executedChecks);
            if (maplayer != null)
            {
                list.Add(maplayer);
            }
        }

        private void LoadLayers(ICollection<Check> executedChecks)
        {
            controller = new LayerHandler();
            controller.ReadLayers(GetAllLayerFilePaths(), executedChecks);
        }

        private string GetPath(string path)
        {
            if (path[0] == '/')
            {
                path = path.Substring(1);
            }
            path = path.Replace('/', '\\');
            return Path.Combine(basePath, path);
        }

        internal LayerHandler GetLayerController()
        {
            return controller;
        }

        internal IEnumerable<Task> GetAllTasks()
        {
            List<Task> tasklist = list.SelectMany(item => item.GetTasks()).ToList();
            tasklist.AddRange(controller.GetLayerTasks());
            return tasklist;
        }

        internal void Run1ClickTask(string taskName, string filename)
        {
            var tasks = GetAllTasks().ToList().FindAll(item => item.FileName.Equals(filename) && item.Tag.ToString().Equals(taskName));

            if (tasks == null || !tasks.Any())
            {
                return;
            }

            var tbdmmapReader = new TbdmmapReader();
            var layerReader = new LayerReader();

            foreach(var task in tasks)
            {
                if(!task.IsActive)
                {
                    continue;
                }

                switch (taskName)
                {
                    case "SetDatasource":
                        tbdmmapReader.ResetDataSource(task, filename);
                        break;
                    case "SetWindowState":
                        tbdmmapReader.SetWindowState(task, filename);
                        break;
                    case "RemoveViewPort":
                        tbdmmapReader.DeleteViewPort(task, filename);
                        break;
                    case "RemoveUnusedGroup":
                        tbdmmapReader.DeleteMapLayerGroup(task, filename);
                        break;
                    case "RemoveExtendedData":
                        layerReader.DeleteExtendedData(task, filename);
                        break;
                    case "RemoveFilter":
                        layerReader.DeleteFilterNodes(task, filename);
                        break;
                    case "SetAttributes":
                        layerReader.ChangeLayerAttributes(task, filename);
                        break;
                }
            }
        }

        internal void CreateCsv(string path)
        {
            try
            {
                List<CsvItem> items = GetCsvItems();

                using (var writer = new StreamWriter(path, false, Encoding.GetEncoding(1252))) //ANSI encoding
                {
                    writer.WriteLine("Layername;Caption;Item");
                    foreach (CsvItem item in items)
                    {
                        writer.WriteLine(item.GetCsvString());
                    }
                    writer.Close();
                }
                MessageBox.Show(Resources.CSVCreationSuccess);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        internal IEnumerable<string> GetAllLayerFilePaths()
        {
            return list.SelectMany(item => item.GetMapLayers()).Select(layer => GetPath(layer.ResourceID));
        }

        internal List<CsvItem> GetCsvItems()
        {
            List<CsvItem> csvItems = new List<CsvItem>();

            foreach (string layerPath in GetAllLayerFilePaths())
            {
                csvItems.Add(GetCsvItem(layerPath));
            }

            return csvItems;
        }

        private CsvItem GetCsvItem(string layerPath)
        {
            var doc = new XmlDocument();
            doc.Load(layerPath);

            XmlNodeList list = doc.GetElementsByTagName("SimpleSymbolDefinition");

            var item = new CsvItem();
            item.Caption = GetLayerCaption(layerPath);
            item.Layername = layerPath;

            foreach (XmlNode node in list)
            {
                string symbole = node.ChildNodes.Item(0).InnerText;
                if (!item.Items.Any(csvItem => csvItem == symbole))
                {
                    item.Items.Add(symbole);
                }
            }

            return item;
        }

        private string GetLayerCaption(string layerPath)
        {
            string filename = Path.GetFileName(layerPath);
            var items = list.SelectMany(item => item.GetMapLayers()).Select(item => item);
            var layerObj = items.First(item => Path.GetFileName(item.ResourceID).Equals(filename));
            if (layerObj != null)
            {
                return layerObj.Name;
            }
            else
            {
                return string.Empty;
            }
        }

        internal void RemoveLayerFromFolder(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        internal void RemoveLayerFromFile(string tbdmmap, string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            var invalidLayers = list.Select(item => new
            {
                Parent = item,
                InvalidLayers = item.GetMapLayers()
                            .Where(mapItem => mapItem.ResourceID.Contains(fileName))
            }).Where(invalidLayerItem => invalidLayerItem.InvalidLayers.Any()).ToList();

            foreach (var invalidLayer in invalidLayers)
            {
                invalidLayer.Parent.RemoveMapLayer(invalidLayer.InvalidLayers.First());
            }

            var writer = new MapXmlWriter();
            IEnumerable<LayerGroup> groups = list.SelectMany(item => item.GetGroups());
            IEnumerable<TbdmmapItem> layers = list.SelectMany(item => item.GetMapLayers());
            var mapLayer = new TbdmmapLayerHandler();
            mapLayer.AddGroups(groups);
            mapLayer.AddMapLayers(layers);
            mapLayer.CoordSystem = GetItems().Select(item => item.CoordSystem).First();
            mapLayer.Units = GetItems().Select(item => item.Units).First();
            writer.WriteTbdmmap(tbdmmap, mapLayer);
        }
    }
}
