using GEOBOX.OSC.DisplayModelEditor.DAL;
using GEOBOX.OSC.DisplayModelEditor.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GEOBOX.OSC.DisplayModelEditor.FileHandler
{
    internal class LayerHandler
    {
        private List<Task> tasks = new List<Task>();
        private LayerReader reader = new LayerReader();

        internal void ReadLayers(IEnumerable<string> layers)
        {
            MissingLayerHandler.ClearList();

            if (layers.Count() < 1)
            {
                return;
            }

            //Read layers from tbdmmap
            foreach (string layer in layers)
            {
                tasks.AddRange(reader.Read(layer));
            }

            //Read layers from folder
            string dirPath = Path.GetDirectoryName(layers.First());
            if (Directory.Exists(dirPath))
            {
                List<string> layersInFolder = Directory.GetFiles(dirPath).ToList();
                foreach (var missingLayer in layersInFolder.Except(layers))
                {
                    MissingLayerHandler.AddMissingLayer(new MissingLayer(missingLayer, MissingLayer.Tag.Folder));
                }
            }
        }

        internal IEnumerable<Task> GetLayerTasks()
        {
            return tasks;
        }
    }
}