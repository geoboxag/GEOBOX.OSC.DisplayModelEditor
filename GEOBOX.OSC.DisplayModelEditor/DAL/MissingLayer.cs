using GEOBOX.OSC.DisplayModelEditor.Properties;
using System.Collections.Generic;
using System.Linq;

namespace GEOBOX.OSC.DisplayModelEditor.DAL
{
    public class MissingLayer
    {
        public string TextFile { get; set; }
        public string TextFolder { get; set; }
        public string LayerPath { get; set; }
        public Tag LayerTag { get; set; }
        public bool IsActive { get; set; }

        public MissingLayer(string path, Tag tag)
        {
            if (tag == Tag.File)
            {
                TextFile = GetPathToDisplay(path);
                TextFolder = Resources.NotAvailable;
                LayerPath = path;
                LayerTag = tag;
            }
            else
            {
                TextFile = Resources.NotAvailable;
                TextFolder = GetPathToDisplay(path);
                LayerPath = path;
                LayerTag = tag;
            }
        }

        public enum Tag
        {
            File,
            Folder
        };

        private string GetPathToDisplay(string filePath)
        {
            List<string> split = filePath.Split('\\').ToList();
            split.RemoveRange(0, 2);

            return string.Join("\\", split);
        }
    }
}
