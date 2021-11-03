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

        internal void ReadLayers(IEnumerable<string> layers, ICollection<Check> executedChecks)
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
            List<string> layersInFolder = new List<string>();
            if (Directory.Exists(dirPath))
            {
                layersInFolder.AddRange(Directory.GetFiles(dirPath).ToList());
                foreach (var missingLayer in layersInFolder.Except(layers))
                {
                    MissingLayerHandler.AddMissingLayer(new MissingLayer(missingLayer, MissingLayer.Tag.Folder));
                }
            }

            if(MissingLayerHandler.GetMissingLayers().Count() > 0)
            {
                executedChecks?.Add(new Check() { Name = "Layer-Dateien", Count = layersInFolder.Count(), IsOk = false, CountFaults = MissingLayerHandler.GetMissingLayers().Count() });
            }
            else
            {
                executedChecks?.Add(new Check() { Name = "Layer-Dateien", Count = layersInFolder.Count(), IsOk = true, CountFaults = MissingLayerHandler.GetMissingLayers().Count() });
            }

        }

        internal IEnumerable<Task> GetLayerTasks()
        {
            return tasks;
        }
    }
}