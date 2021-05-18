namespace GEOBOX.OSC.DisplayModelEditor.DAL
{
    internal class TbdmmapItem
    {
        internal string Name { get; set; }
        internal string ResourceID { get; set; }
        internal bool Selectable { get; set; }
        internal bool ShowInLegend { get; set; }
        internal bool ExpandInLegend { get; set; }
        internal bool Visible { get; set; }
        internal string Group { get; set; }
        internal int Order { get; set; }
        internal string FeatureSourceResourceId { get; set; }
    }
}
