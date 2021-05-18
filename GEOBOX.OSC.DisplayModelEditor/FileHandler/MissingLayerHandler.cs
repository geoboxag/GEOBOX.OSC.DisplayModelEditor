using GEOBOX.OSC.DisplayModelEditor.DAL;
using System.Collections.Generic;

namespace GEOBOX.OSC.DisplayModelEditor.FileHandler
{
    class MissingLayerHandler
    {
        private static List<MissingLayer> list = new List<MissingLayer>();

        internal static void AddMissingLayer(MissingLayer layer)
        {
            list.Add(layer);
        }

        internal static void ClearList()
        {
            list.Clear();
        }

        internal static IEnumerable<MissingLayer> GetMissingLayers()
        {
            return list;
        }

        internal static int GetCount()
        {
            return list.Count;
        }
    }
}
