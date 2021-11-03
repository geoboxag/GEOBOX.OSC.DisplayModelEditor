using GEOBOX.OSC.DisplayModelEditor.CorrectionTasks;
using GEOBOX.OSC.DisplayModelEditor.DAL;
using GEOBOX.OSC.DisplayModelEditor.Enums;
using GEOBOX.OSC.DisplayModelEditor.FileHandler;
using GEOBOX.OSC.DisplayModelEditor.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;

namespace GEOBOX.OSC.DisplayModelEditor.IO
{
    class TbdmmapReader
    {
        internal TbdmmapLayerHandler ReadTbdmmap(string filePath, ICollection<Check> executedChecks)
        {
            var item = new TbdmmapLayerHandler();
            XmlDocument xmlReadDoc = ReadTbdmmapFile(filePath);
            if (xmlReadDoc == null)
            {
                MessageBox.Show("tbdmmap-Datei konnte nicht gelesen werden.");
                return null;
            }

            
            item.Units = GetFirstElementByTagNameOrDefault(xmlReadDoc, "Units");
            if(string.IsNullOrEmpty(item.Units))
            {
                executedChecks?.Add(GetNewCheck("Einheit", false, 0, 1));
            }
            else
            {
                executedChecks?.Add(GetNewCheck("Einheit", true, 1, 0));
            }


            item.CoordSystem = GetFirstElementByTagNameOrDefault(xmlReadDoc, "CoordinateSystem");
            if (string.IsNullOrEmpty(item.CoordSystem))
            {
                executedChecks?.Add(GetNewCheck("Koordinaten-System", false, 0, 1));
            }
            else
            {
                executedChecks?.Add(GetNewCheck("Koordinaten-System", true, 1, 0));
            }


            var mapLayerGroups = xmlReadDoc.GetElementsByTagName("MapLayerGroup");
            item.AddGroups(ReadGroups(mapLayerGroups));

            var displayModelMapLayers = xmlReadDoc.GetElementsByTagName("DisplayModelMapLayer");
            item.AddMapLayers(ReadMapLayers(filePath, displayModelMapLayers, item, executedChecks));

            //Check MapLayers
            CheckMapLayerGroups(filePath, (List<LayerGroup>)item.GetGroups(), (List<TbdmmapItem>)item.GetMapLayers(), item, executedChecks);

            //Check tbdmmap
            CheckViewPort(filePath, item, xmlReadDoc.GetElementsByTagName("LastViewport"), executedChecks);
            CheckWindowStatus(filePath, item, xmlReadDoc.GetElementsByTagName("WindowStatus"), executedChecks);
            return item;
        }

        private IEnumerable<LayerGroup> ReadGroups(XmlNodeList xmlNodesGroup)
        {
            var groups = new List<LayerGroup>();
            foreach (XmlNode xmlNode in xmlNodesGroup)
            {
                try
                {
                    var group = new LayerGroup();
                    group.Name = xmlNode["Name"].InnerText;
                    group.Visible = Convert.ToBoolean(xmlNode["Visible"].InnerXml);
                    group.ShowInLegend = Convert.ToBoolean(xmlNode["ShowInLegend"].InnerXml);
                    group.ExpandInLegend = Convert.ToBoolean(xmlNode["ExpandInLegend"].InnerXml);
                    group.Group = xmlNode["Group"]?.InnerText;
                    group.Order = Convert.ToInt16(xmlNode["Order"].InnerXml);
                    groups.Add(group);
                }
                catch
                {
                    continue;
                }
            }

            return groups;
        }

        private IEnumerable<TbdmmapItem> ReadMapLayers(string filePath, XmlNodeList xmlNodesMapLayers, TbdmmapLayerHandler mapLayer, ICollection<Check> executedChecks)
        {
            var list = new List<TbdmmapItem>();
            int taskCounter = 0;

            foreach (XmlNode xmlNode in xmlNodesMapLayers)
            {
                try
                {
                    XmlNode maplayer = xmlNode.ChildNodes[0];
                    XmlNode featureSourceResourceId = xmlNode.ChildNodes[1];

                    var item = new TbdmmapItem();
                    item.Name = maplayer["Name"].InnerText;
                    item.ResourceID = maplayer["ResourceId"].InnerText;
                    item.Selectable = Convert.ToBoolean(maplayer["Selectable"].InnerText);
                    item.ShowInLegend = Convert.ToBoolean(maplayer["ShowInLegend"].InnerText);
                    item.ExpandInLegend = Convert.ToBoolean(maplayer["ExpandInLegend"].InnerText);
                    item.Visible = Convert.ToBoolean(maplayer["Visible"].InnerText);
                    item.Group = maplayer["Group"].InnerText;
                    item.Order = Convert.ToInt16(maplayer["Order"].InnerText);
                    item.FeatureSourceResourceId = featureSourceResourceId.InnerText;

                    list.Add(item);

                    //Check resource ID
                    Task resourceCheck = CheckFeatureSourceResourceID(filePath, item.FeatureSourceResourceId);
                    if (resourceCheck != null)
                    {
                        mapLayer.AddTask(resourceCheck);
                        taskCounter++;
                    }
                }
                catch
                {
                    continue;
                }
            }

            if (taskCounter > 0)
            {
                executedChecks?.Add(GetNewCheck($"Datasource: {Properties.Settings.Default.LayerDataSourceName}", false, list.Count, taskCounter));
            }
            else
            {
                executedChecks?.Add(GetNewCheck($"Datasource: {Properties.Settings.Default.LayerDataSourceName}", true, list.Count, taskCounter));
            }

            return list;
        }

        private Task CheckFeatureSourceResourceID(string filePath, string text)
        {
            if (!text.Equals(Properties.Settings.Default.LayerDataSourceName))
            {
                return new Task(filePath, string.Format(Resources.SetDatasource, Properties.Settings.Default.LayerDataSourceName), TaskType.SetDatasource, TaskImage.ToDo);
            }

            return null;
        }

        private void CheckMapLayerGroups(string filePath, List<LayerGroup> groups, List<TbdmmapItem> mapLayers, TbdmmapLayerHandler mapLayerHandler, ICollection<Check> executedChecks)
        {
            int taskCounter = 0;
            List<string> taskKeys = new List<string>();

            foreach (var group in groups)
            {
                var childGroup = groups.Find(g => g.Group != null && g.Group.Equals(group.Name));
                var layer = mapLayers.Find(l => l.Group.Equals(group.Name));
                if(childGroup is null && layer is null)
                {
                    var task = new Task(filePath, string.Format(Resources.RemoveUnusedLayerGroup, group.Name), TaskType.RemoveUnusedGroup, TaskImage.ToDo);
                    mapLayerHandler.AddTask(task);
                    taskKeys.Add(task.TaskKey);
                    taskCounter++;
                }
            }

            if (taskCounter > 0)
            {
                executedChecks?.Add(GetNewCheck("Layer-Gruppen im Tbdmmap", false, groups.Count, taskCounter, taskKeys));
            }
            else
            {
                executedChecks?.Add(GetNewCheck("Layer-Gruppen im Tbdmmap", true, groups.Count, taskCounter, taskKeys));
            }
        }

        private void CheckViewPort(string filePath, TbdmmapLayerHandler mapLayer, XmlNodeList xmlNodesMapLayers, ICollection<Check> executedChecks)
        {
            List<string> taskKeys = new List<string>();

            if (xmlNodesMapLayers.Count > 0)
            {
                var task = new Task(filePath, Resources.RemoveViewPort, TaskType.RemoveViewPort, TaskImage.ToDo);
                mapLayer.AddTask(task);
                taskKeys.Add(task.TaskKey);
                executedChecks?.Add(GetNewCheck("Viewport", false, xmlNodesMapLayers.Count, xmlNodesMapLayers.Count, taskKeys));
            }
            else
            {
                executedChecks?.Add(GetNewCheck("Viewport", true, xmlNodesMapLayers.Count, xmlNodesMapLayers.Count, taskKeys));
            }           
        }

        private Check GetNewCheck(string name, bool isOk, int count, int countFaults, List<string> taskKeys = null)
        {
            var check = new Check()
            {
                Name = name,
                IsOk = isOk,
                Count = count,
                CountFaults = countFaults
            };

            if(taskKeys != null && taskKeys.Any())
            {
                foreach (var taskKey in taskKeys)
                {
                    check.AddTaskKey(taskKey);
                }
            }
            return check;
        }

        private void CheckWindowStatus(string filePath, TbdmmapLayerHandler mapLayer, XmlNodeList xmlNodesMapLayers, ICollection<Check> executedChecks)
        {
            var taskCounter = 0;
            List<string> taskKeys = new List<string>();

            foreach (XmlNode node in xmlNodesMapLayers)
            {
                if (node == null)
                {
                    continue;
                }

                if (!node["Left"].InnerXml.Equals("-0") ||
                   !node["Top"].InnerXml.Equals("-5") ||
                   !node["Width"].InnerXml.Equals("100") ||
                   !node["Height"].InnerXml.Equals("105"))
                {
                    var task = new Task(filePath, Resources.SetWindowState, TaskType.SetWindowState, TaskImage.ToDo);
                    mapLayer.AddTask(task);
                    taskKeys.Add(task.TaskKey);
                    taskCounter++;
                }
            }

            if(taskCounter > 0)
            {
                executedChecks?.Add(GetNewCheck("WindowStatus", false, xmlNodesMapLayers.Count, taskCounter, taskKeys));
            }
            else
            {
                executedChecks?.Add(GetNewCheck("WindowStatus", true, xmlNodesMapLayers.Count, taskCounter, taskKeys));
            }

        }

        #region 1-Click-Maintenance-Tasks
        internal void SetWindowState(Task task, string filePath)
        {
            bool isSaved = false;
            try
            {
                XmlDocument xDoc = ReadTbdmmapFile(filePath);

                if (xDoc.GetElementsByTagName("WindowStatus") != null)
                {
                    //<Left>-0</Left>
                    //<Top>-5</Top>
                    //<Width>100</Width>
                    //<Height>105</Height>
                    //<WindowState>Maximized</WindowState>
                    XmlNode xNode = xDoc.GetElementsByTagName("WindowStatus").Item(0);
                    if (xNode != null)
                    {
                        xNode["Left"].InnerXml = "-0";
                        xNode["Top"].InnerXml = "-5";
                        xNode["Width"].InnerXml = "100";
                        xNode["Height"].InnerXml = "105";
                        xNode["WindowState"].InnerXml = "Maximized";
                    }
                }

                isSaved = SaveTbdmapFile(filePath, xDoc);

                SetTaskStatus(isSaved, task);
                xDoc = null;
            }
            catch
            {
                SetTaskStatus(isSaved, task);
            }
        }

        internal void DeleteViewPort(Task task, string filePath)
        {
            bool isSaved = false;
            try
            {
                XmlDocument doc = ReadTbdmmapFile(filePath);
                XmlNodeList list = doc.GetElementsByTagName("LastViewport");
                XmlNode[] listArr = new XmlNode[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    listArr[i] = list[i];
                }

                foreach (XmlNode n in listArr)
                {
                    XmlNode parent = n.ParentNode;
                    XmlNode child = parent.SelectSingleNode("./LastViewport");
                    parent.RemoveChild(child);
                }

                isSaved = SaveTbdmapFile(filePath, doc);

                SetTaskStatus(isSaved, task);
                doc = null;
            }
            catch
            {
                SetTaskStatus(isSaved, task);
            }
        }

        internal void ResetDataSource(Task task, string filePath)
        {
            bool isSaved = false;
            try
            {
                XmlDocument doc = ReadTbdmmapFile(filePath);
                //XmlNodeList list = doc.GetElementsByTagName("FeatureSourceResourceId");

                //foreach (XmlNode n in list)
                //{
                //    string temp = "GBX_DEV_FA";
                //    if (!n.InnerText.Equals(temp))
                //    {
                //        n.InnerText = temp;
                //    }
                //}

                var result = new ResetDataSourceCorrectionTask().Apply(new CorrectionTaskContext(ReadTbdmmapFile(filePath))).DocumentAfterCorrection;

                isSaved = SaveTbdmapFile(filePath, result);

                SetTaskStatus(isSaved, task);
                //doc = null;
            }
            catch
            {
                SetTaskStatus(isSaved, task);
            }
        }
        #endregion

        private XmlDocument ReadTbdmmapFile(string filePath)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                var myReader = new XmlTextReader(filePath);
                xmlDoc.Load(myReader);
                myReader.Close();
                return xmlDoc;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"Datei {filePath} wurde nicht gefunden...");
                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool SaveTbdmapFile(string filePath, XmlDocument xDoc)
        {
            try
            {
                xDoc.Save(filePath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    "Die Datei ist mit einem Schreibschutz versehen." + Environment.NewLine +
                    "Entfernen Sie den Schreibschutz und wiederholen sie den Vorgang." + Environment.NewLine +
                    Environment.NewLine + "Datei:" + Environment.NewLine +
                    $"{filePath}",
                    "Speichern fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception) { /* Do nothing */ return false; }

        }


        internal void DeleteMapLayerGroup(Task task, string filePath)
        {
            bool isSaved = false;
            try
            {
                XmlDocument doc = ReadTbdmmapFile(filePath);
                XmlNodeList displayModelMapNodes = doc.GetElementsByTagName("DisplayModelMap");

                foreach (var displayModelMapNode in displayModelMapNodes)
                {
                    XmlNode parent = (XmlNode)displayModelMapNode;
                    XmlNodeList childs = parent?.SelectNodes("./MapLayerGroup");
                    
                    foreach(var child in childs)
                    {
                        XmlNode childToRemove = null;

                        XmlNode node = (XmlNode)child;
                        string name = node["Name"].InnerText;

                        if (task.Text.Contains(name))
                        {
                            childToRemove = node;
                        }

                        if(childToRemove != null)
                        {
                            parent.RemoveChild(childToRemove);
                        }                 
                    }                 
                }

                isSaved = SaveTbdmapFile(filePath, doc);

                SetTaskStatus(isSaved, task);
                doc = null;
            }
            catch
            {
                SetTaskStatus(isSaved, task);
            }
        }

        /// <summary>
        /// Return Inner Text or String.Empty
        /// </summary>
        /// <returns></returns>
        private string GetFirstElementByTagNameOrDefault(XmlDocument xmlReadDoc, string tagName)
        {
            var items = xmlReadDoc.GetElementsByTagName(tagName);

            if (items.Count == 0) return string.Empty;

            var firstItem = items.Item(0);

            if (firstItem is null) return string.Empty;

            if (String.IsNullOrEmpty(firstItem.InnerText)) return string.Empty;

            return firstItem.InnerText;
        }


        private void SetTaskStatus(bool isSaved, Task task)
        {
            if (isSaved)
            {
                task.IsFixed = true;
                task.IsActive = false;
                task.IsEnabled = false;
                task.SetImage(TaskImage.Done);
            }
            else
            {
                task.IsFixed = false;
                task.IsActive = true;
                task.IsEnabled = true;
                task.SetImage(TaskImage.Warning);
            }
        }

    }
}
