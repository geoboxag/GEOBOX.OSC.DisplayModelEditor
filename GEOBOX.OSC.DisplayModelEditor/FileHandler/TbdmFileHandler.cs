using GEOBOX.OSC.DisplayModelEditor.DAL;
using GEOBOX.OSC.DisplayModelEditor.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GEOBOX.OSC.DisplayModelEditor.FileHandler
{
    class TbdmFileHandler
    {
        private List<TbdmItemHandler> list = new List<TbdmItemHandler>();
        private readonly string filePath;
        private readonly string basePath;
        private TbdmmapFileHandler tbdmmapFileHandler;

        internal TbdmFileHandler(string basePath, string filePath)
        {
            this.filePath = filePath;
            this.basePath = basePath;
        }

        internal void Read()
        {
            LoadTbdm();
            LoadTbdmmap();
        }

        internal TbdmItemHandler GetTbdmmapItem()
        {
            return list.FirstOrDefault();
        }
        
        internal TbdmmapFileHandler GetTbdmmapController()
        {
            return tbdmmapFileHandler;
        }
        
        private void LoadTbdm()
        {
            list.Clear();
            var reader = new TbdmReader();
            list.Add(reader.ReadTbdm(filePath));
        }

        private void LoadTbdmmap()
        {
            tbdmmapFileHandler = new TbdmmapFileHandler(basePath);
            foreach (var tbdm in list)
            {
                foreach (var tbdmmap in tbdm.GetTbdmmaps())
                {
                    tbdmmapFileHandler.Load(tbdmmap);
                }
            }
        }

        internal void DeleteLayer(MissingLayer missingLayer)
        {
            if (missingLayer.LayerTag.ToString().Equals(MissingLayer.Tag.File.ToString()))
            {
                tbdmmapFileHandler.RemoveLayerFromFile(GetPath(list.First().GetTbdmmaps().First()), missingLayer.LayerPath);
            }
            else
            {
                tbdmmapFileHandler.RemoveLayerFromFolder(missingLayer.LayerPath);
            }
        }

        private string GetPath(string path)
        {
            path = path.Replace('/', '\\');
            return Path.Combine(basePath, path);
        }
    }
}
