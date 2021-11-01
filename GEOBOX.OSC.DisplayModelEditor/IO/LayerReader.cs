using GEOBOX.OSC.DisplayModelEditor.DAL;
using GEOBOX.OSC.DisplayModelEditor.Enums;
using GEOBOX.OSC.DisplayModelEditor.FileHandler;
using GEOBOX.OSC.DisplayModelEditor.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace GEOBOX.OSC.DisplayModelEditor.IO
{
    class LayerReader
    {
        internal List<Task> Read(string filePath)
        {
            var cursorBefore = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            List<Task> list = new List<Task>();
            try
            {
                CheckAttributes(filePath, list);
                CheckFilter(filePath, list);
                CheckExtendedData(filePath, list);
            }
            catch
            {
                list.Add(new Task(string.Empty, Resources.LayersNotFound, TaskType.LayerNotFound, TaskImage.ToDo) { IsEnabled = false });
                MissingLayerHandler.AddMissingLayer(new MissingLayer(filePath, MissingLayer.Tag.File));
            }

            Cursor.Current = cursorBefore;
            return list;
        }

        private void CheckAttributes(string layerPath, List<Task> taskList)
        {
            var doc = new XmlDocument();
            doc.Load(layerPath);

            XmlNodeList list = doc.GetElementsByTagName("Name");
            XmlNode service = GetXmlNodeValue(list, "Service");
            XmlNode username = GetXmlNodeValue(list, "Username");
            XmlNode dataStore = GetXmlNodeValue(list, "DataStore");
            
            if (
                !service.InnerText.Equals("topobase") ||
                !username.InnerText.Equals("MAPSYS") ||
                !dataStore.InnerText.Equals("GBX_DEV_FA")
              )
            {
                taskList.Add(new Task(layerPath, Resources.SetAttributes, TaskType.SetAttributes, TaskImage.ToDo));
            }
        }

        private void CheckFilter(string layerPath, List<Task> list)
        {
            XDocument doc = XDocument.Load(layerPath, LoadOptions.SetLineInfo);
            IEnumerable<XElement> vectorLayerDefList = doc.Descendants("VectorLayerDefinition"); //VectorLayerDefinition exists only once

            var filterNode = vectorLayerDefList?.Nodes()?.Where(n => ((XElement)n).Name == "Filter").FirstOrDefault();

            if (filterNode != null)
            {
                IXmlLineInfo info = filterNode;
                int xmlFileLineNumber = info.LineNumber;

                list.Add(new Task(layerPath, $"#{xmlFileLineNumber}: {Resources.RemoveFilter}", TaskType.RemoveFilter, TaskImage.ToDo));
            }
        }

        private void CheckExtendedData(string layerPath, List<Task> taskList)
        {
            XDocument doc = XDocument.Load(layerPath, LoadOptions.SetLineInfo);
            IEnumerable<XElement> extendedData1List = doc.Descendants("ExtendedData1"); //VectorLayerDefinition exists only once

            foreach (XElement e in extendedData1List)
            {
                IXmlLineInfo info = e;
                int xmlFileLineNumber = info.LineNumber;

                taskList.Add(new Task(layerPath, $"#{xmlFileLineNumber}: {Resources.RemoveExtendedData}", TaskType.RemoveExtendedData, TaskImage.ToDo));
                return;
            }
        }

        private XmlNode GetXmlNodeValue(XmlNodeList list, string name)
        {
            foreach (XmlNode n in list)
            {
                if (n.InnerText.Equals(name))
                {
                    return n.ParentNode.LastChild;
                }
            }

            return null;
        }

        #region 1-Click-Maintenance-Tasks
        internal void ChangeLayerAttributes(Task task, string layerName)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(layerName);

                XmlNodeList list = doc.GetElementsByTagName("Name");
                XmlNode service = GetXmlNodeValue(list, "Service");
                service.InnerText = "topobase";

                XmlNode username = GetXmlNodeValue(list, "Username");
                username.InnerText = "MAPSYS";

                XmlNode dataStore = GetXmlNodeValue(list, "DataStore");
                string name = dataStore.InnerText;
                ReplaceDataSource(doc.GetElementsByTagName("rdf:Description"), name);
                dataStore.InnerText = "GBX_DEV_FA";

                XmlNodeList featureName = doc.GetElementsByTagName("FeatureName");
                featureName[0].InnerText = featureName[0].InnerText.Replace(name, "GBX_DEV_FA");

                doc.Save(layerName);
                doc = null;

                task.SetImage(TaskImage.Done);
            }
            catch (Exception)
            {
                task.SetImage(TaskImage.Warning);
            }
        }

        private void ReplaceDataSource(XmlNodeList list, string name)
        {
            foreach (XmlNode node in list)
            {
                string value = node.Attributes[0].Value;
                if (value.Contains(name))
                {
                    node.Attributes[0].Value = value.Replace(name, "GBX_DEV_FA");
                }
            }
        }

        internal void DeleteFilterNodes(Task task, string layerName)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(layerName);

                XmlNodeList list = doc.GetElementsByTagName("VectorLayerDefinition");
                XmlNode vectorLayerDef = list[0];
                XmlNode node = null;

                foreach (XmlNode filterNode in vectorLayerDef.ChildNodes)
                {
                    if (filterNode.Name.Equals("Filter"))
                    {
                        node = filterNode;
                        break;
                    }
                }

                XmlNode parent = node.ParentNode;
                XmlNode child = parent.SelectSingleNode("./Filter");
                parent.RemoveChild(child);

                doc.Save(layerName);
                doc = null;

                task.SetImage(TaskImage.Done);
            }
            catch (Exception)
            {
                task.SetImage(TaskImage.Warning);
            }
        }

        internal void DeleteExtendedData(Task task, string layerName)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(layerName);

                XmlNodeList list = doc.GetElementsByTagName("ExtendedData1");
                List<XmlNode> tempList = new List<XmlNode>();
                foreach (XmlNode node in list)
                {
                    tempList.Add(node);
                }

                foreach (XmlNode node in tempList)
                {
                    XmlNode parent = node.ParentNode;
                    XmlNode child = parent.SelectSingleNode("./" + node.Name);
                    parent.RemoveChild(child);
                }

                doc.Save(layerName);
                doc = null;
                task.SetImage(TaskImage.Done);
            }
            catch
            {
                task.SetImage(TaskImage.Warning);
            }
        }
        #endregion
    }
}
