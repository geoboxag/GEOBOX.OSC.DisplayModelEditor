using System.Xml;

namespace GEOBOX.OSC.DisplayModelEditor.CorrectionTasks
{
    public sealed class CorrectionTaskContext : ICorrectionTaskContext
    {
        public CorrectionTaskContext(XmlDocument currentXmlDocument)
        {
            CurrentXmlDocument = currentXmlDocument;
        }

        public XmlDocument CurrentXmlDocument
        {
            get; set;
        }
    }
}
