using GEOBOX.OSC.DisplayModelEditor.CorrectionTasks;
using GEOBOX.OSC.DisplayModelEditor.DAL;
using GEOBOX.OSC.DisplayModelEditor.Enums;
using GEOBOX.OSC.DisplayModelEditor.FileHandler;
using GEOBOX.OSC.DisplayModelEditor.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using System.Xml;

namespace GEOBOX.OSC.DisplayModelEditor.IO
{
    class TbdmmapReader
    {
        internal TbdmmapLayerHandler ReadTbdmmap(string filePath)
        {
            var item = new TbdmmapLayerHandler();
            XmlDocument xmlReadDoc = ReadTbdmmapFile(filePath);
            if (xmlReadDoc == null)
            {
                MessageBox.Show("tbdmmap-Datei konnte nicht gelesen werden.");
                return null;
            }
            item.Units = GetFirstElementByTagNameOrDefault(xmlReadDoc, "Units");
            item.CoordSystem = GetFirstElementByTagNameOrDefault(xmlReadDoc, "CoordinateSystem");
            item.AddGroups(ReadGroups(xmlReadDoc.GetElementsByTagName("MapLayerGroup")));
            item.AddMapLayers(ReadMapLayers(filePath, xmlReadDoc.GetElementsByTagName("DisplayModelMapLayer"), item));

            //Check tbdmmap
            CheckViewPort(filePath, item, xmlReadDoc.GetElementsByTagName("LastViewport"));
            CheckWindowStatus(filePath, item, xmlReadDoc.GetElementsByTagName("WindowStatus"));
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

        private IEnumerable<TbdmmapItem> ReadMapLayers(string filePath, XmlNodeList xmlNodesMapLayers, TbdmmapLayerHandler mapLayer)
        {
            var list = new List<TbdmmapItem>();
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
                    }
                }
                catch
                {
                    continue;
                }
            }

            return list;
        }

        private Task CheckFeatureSourceResourceID(string filePath, string text)
        {
            if (!text.Equals("GBX_DEV_FA"))
            {
                return new Task(filePath, Resources.SetDatasource, TaskType.SetDatasource, TaskImage.ToDo);
            }

            return null;
        }

        private void CheckViewPort(string filePath, TbdmmapLayerHandler mapLayer, XmlNodeList xmlNodesMapLayers)
        {
            if (xmlNodesMapLayers.Count > 0)
            {
                mapLayer.AddTask(new Task(filePath, Resources.RemoveViewPort, TaskType.RemoveViewPort, TaskImage.ToDo));
            }
        }

        private void CheckWindowStatus(string filePath, TbdmmapLayerHandler mapLayer, XmlNodeList xmlNodesMapLayers)
        {
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
                    mapLayer.AddTask(new Task(filePath, Resources.SetWindowState, TaskType.SetWindowState, TaskImage.ToDo));
                }
            }
        }

        #region 1-Click-Maintenance-Tasks
        internal void SetWindowState(Task task, string filePath)
        {
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
                        SaveTbdmapFile(filePath, xDoc);
                    }
                }
                task.SetImage(TaskImage.Done);
            }
            catch
            {
                task.SetImage(TaskImage.Warning);
            }
        }

        internal void DeleteViewPort(Task task, string filePath)
        {
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

                SaveTbdmapFile(filePath, doc);
                doc = null;

                task.SetImage(TaskImage.Done);
            }
            catch
            {
                task.SetImage(TaskImage.Warning);
            }
        }

        internal void ResetDataSource(Task task, string filePath)
        {
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

                SaveTbdmapFile(filePath, result);
                //doc = null;

                task.SetImage(TaskImage.Done);
            }
            catch
            {
                task.SetImage(TaskImage.Warning);
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

        private void SaveTbdmapFile(string filePath, XmlDocument xDoc)
        {
            xDoc.Save(filePath);
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
    }
}
