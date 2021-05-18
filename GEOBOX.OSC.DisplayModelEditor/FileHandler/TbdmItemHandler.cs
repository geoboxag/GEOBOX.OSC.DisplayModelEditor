using System.Collections.Generic;

namespace GEOBOX.OSC.DisplayModelEditor.FileHandler
{
    class TbdmItemHandler
    {
        private string name;
            private List<string> tbdmmapFilePaths = new List<string>();

            internal string GetName()
            {
                return name;
            }

            internal IEnumerable<string> GetTbdmmaps()
            {
                return tbdmmapFilePaths;
            }

            internal void AddPath(string path)
            {
                tbdmmapFilePaths.Add(path);
            }

            internal void SetName(string name)
            {
                this.name = name;
            }
    }
}
