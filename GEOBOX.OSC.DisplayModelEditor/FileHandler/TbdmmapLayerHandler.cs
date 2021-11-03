using GEOBOX.OSC.DisplayModelEditor.DAL;
using System.Collections.Generic;
using System.Linq;

namespace GEOBOX.OSC.DisplayModelEditor.FileHandler
{
    class TbdmmapLayerHandler
    {
        private List<LayerGroup> groups = new List<LayerGroup>();
        private List<TbdmmapItem> items = new List<TbdmmapItem>();
        private List<Task> tasks = new List<Task>();
        internal string Units { get; set; }
        internal string CoordSystem { get; set; }

        internal void AddGroups(IEnumerable<LayerGroup> groups)
        {
            this.groups.Clear();
            this.groups.AddRange(groups);
        }

        internal void AddMapLayers(IEnumerable<TbdmmapItem> items)
        {
            this.items.Clear();
            this.items.AddRange(items);
        }

        internal void AddTask(Task task)
        {
            if (!tasks.Any(item => item.TaskKey == task.TaskKey))
            {
                tasks.Add(task);
            }
        }

        internal void RemoveMapLayer(TbdmmapItem layer)
        {
            items.Remove(layer);
        }

        internal void RemoveGroup(LayerGroup layerGroup)
        {
            groups.Remove(layerGroup);
        }

        internal IEnumerable<LayerGroup> GetGroups()
        {
            return groups;
        }

        internal IEnumerable<TbdmmapItem> GetMapLayers()
        {
            return items;
        }

        internal IEnumerable<Task> GetTasks()
        {
            return tasks;
        }
    }
}
