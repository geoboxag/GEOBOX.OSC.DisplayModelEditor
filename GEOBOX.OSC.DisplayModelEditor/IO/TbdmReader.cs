using GEOBOX.OSC.DisplayModelEditor.FileHandler;
using System.Xml;

namespace GEOBOX.OSC.DisplayModelEditor.IO
{
    internal class TbdmReader
    {
        internal TbdmItemHandler ReadTbdm(string file)
        {
            var tbdm = new TbdmItemHandler();

            var myReader = new XmlTextReader(@file);
            var xmlReadDoc = new XmlDocument();
            xmlReadDoc.Load(myReader);

            XmlNodeList nameList = xmlReadDoc.GetElementsByTagName("Name");

            if (nameList.Count > 0)
            {
                tbdm.SetName(nameList[0].InnerText);
            }

            foreach (XmlNode item in xmlReadDoc.GetElementsByTagName("DisplayModelMapResourceId"))
            {
                string path = item.InnerText;
                if (path[0] == '/')
                {
                    path = path.Substring(1);
                }
                path = path.Replace('/', '\\');
                tbdm.AddPath(path);
            }

            return tbdm;
        }
    }
}
