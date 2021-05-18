using System.Xml;

namespace GEOBOX.OSC.DisplayModelEditor.CorrectionTasks
{
    public interface ICorrectionTaskContext
    {
        XmlDocument CurrentXmlDocument { get; set; }
    }
}