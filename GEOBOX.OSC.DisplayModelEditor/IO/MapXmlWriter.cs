using GEOBOX.OSC.DisplayModelEditor.DAL;
using GEOBOX.OSC.DisplayModelEditor.FileHandler;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace GEOBOX.OSC.DisplayModelEditor.IO
{
    internal class MapXmlWriter
    {
        internal void WriteXml(string fileName, TbdmmapLayerHandler layer)
        {
            try
            {
                WriteTbdm(fileName);
                WriteTbdmmap(fileName, layer);
            }
            catch
            {
                MessageBox.Show("Dateien konnten nicht erzeugt werden");
            }
        }

        internal void WriteTbdm(string fileName)
        {
            string file = Path.GetFileNameWithoutExtension(fileName);
            var root = new XElement("DisplayModel");
            //Namespaces
            XNamespace x1 = "http://www.w3.org/2001/XMLSchema";
            XNamespace x2 = "http://www.w3.org/2001/XMLSchema-instance";
            root.Add(new XAttribute(XNamespace.Xmlns + "xsd", x1));
            root.Add(new XAttribute(XNamespace.Xmlns + "xsi", x2));
            root.Add(new XAttribute("version", "1.0.0"));

            root.Add(new XElement("Name", file));
            root.Add(new XElement("DisplayModelMapResourceId", $"/{file}.tbdmmap"));

            SaveFile(fileName, new XDocument(root));
        }

        internal void WriteTbdmmap(string fileName, TbdmmapLayerHandler tbdmmapMapLayer)
        {
            string file = Path.Combine(Path.GetDirectoryName(fileName), $"{Path.GetFileNameWithoutExtension(fileName)}.tbdmmap");
            XElement root = GetTbdmmapHead(fileName, tbdmmapMapLayer.Units, tbdmmapMapLayer.CoordSystem);

            foreach (TbdmmapItem layer in tbdmmapMapLayer.GetMapLayers())
            {
                var displayModelMapLayer = new XElement("DisplayModelMapLayer");
                var mapLayer = new XElement("MapLayer");
                mapLayer.Add(new XElement("Name", layer.Name));
                mapLayer.Add(new XElement("ResourceId", layer.ResourceID));
                mapLayer.Add(new XElement("Selectable", layer.Selectable));
                mapLayer.Add(new XElement("ShowInLegend", layer.ShowInLegend));
                mapLayer.Add(new XElement("ExpandInLegend", layer.ExpandInLegend));
                mapLayer.Add(new XElement("Visible", layer.Visible));
                mapLayer.Add(new XElement("Group", layer.Group));
                mapLayer.Add(new XElement("Order", layer.Order));
                var resource = new XElement("FeatureSourceResourceId", layer.FeatureSourceResourceId);
                displayModelMapLayer.Add(mapLayer);
                displayModelMapLayer.Add(resource);

                root.Add(displayModelMapLayer);
            }

            foreach (LayerGroup group in tbdmmapMapLayer.GetGroups())
            {
                var mapLayerGroup = new XElement("MapLayerGroup");
                mapLayerGroup.Add(new XElement("Name", group.Name));
                mapLayerGroup.Add(new XElement("Visible", group.Visible));
                mapLayerGroup.Add(new XElement("ShowInLegend", group.ShowInLegend));
                mapLayerGroup.Add(new XElement("ExpandInLegend", group.ExpandInLegend));
                mapLayerGroup.Add(new XElement("LegendLabel"));
                if (group.Group != null)
                {
                    mapLayerGroup.Add(new XElement("Group", group.Group));
                }
                mapLayerGroup.Add(new XElement("Order", group.Order));

                root.Add(mapLayerGroup);
            }

            var doc = new XDocument(root);

            SaveFile(file, doc);
        }

        private static void SaveFile(string file, XDocument doc)
        {
            var setting = new XmlWriterSettings();
            setting.Indent = true;
            using (var xmlWriter = XmlWriter.Create(file, setting))
            {
                doc.Save(xmlWriter);
                xmlWriter.Flush();
            }
        }

        private XElement GetTbdmmapHead(string fileName, string units, string coordSys)
        {
            var root = new XElement("DisplayModelMap");
            //Namespaces
            XNamespace x1 = "http://www.w3.org/2001/XMLSchema";
            XNamespace x2 = "http://www.w3.org/2001/XMLSchema-instance";
            root.Add(new XAttribute(XNamespace.Xmlns + "xsd", x1));
            root.Add(new XAttribute(XNamespace.Xmlns + "xsi", x2));

            root.Add(new XAttribute("version", "1.3.0"));
            root.Add(new XElement("Name", Path.GetFileNameWithoutExtension(fileName)));
            root.Add(new XElement("Units", units));
            root.Add(new XElement("CoordinateSystem", coordSys));
            var windowStatus = new XElement("WindowStatus");
            windowStatus.Add(new XElement("Left", "-0"));
            windowStatus.Add(new XElement("Top", "-5"));
            windowStatus.Add(new XElement("Width", "100"));
            windowStatus.Add(new XElement("Height", "105"));
            windowStatus.Add(new XElement("WindowState", "Maximized"));
            root.Add(windowStatus);
            return root;
        }
    }
}
