namespace GEOBOX.OSC.DisplayModelEditor.DAL
{
    internal class LayerGroup
    {
        internal string Name { get; set; }
        internal bool Visible { get; set; }
        internal bool ShowInLegend { get; set; }
        internal bool ExpandInLegend { get; set; }
        internal string Group { get; set; }
        internal int Order { get; set; }
    }
}
