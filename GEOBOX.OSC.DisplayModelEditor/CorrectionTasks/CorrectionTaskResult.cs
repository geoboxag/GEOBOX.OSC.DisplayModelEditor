using System.Xml;

namespace GEOBOX.OSC.DisplayModelEditor.CorrectionTasks
{
    public sealed class CorrectionTaskResult : ICorrectionTaskResult
    {
        private XmlDocument documentAfterCorrection;
        private bool successful;

        public CorrectionTaskResult(bool successful, XmlDocument documentAfterCorrection)
        {
            this.documentAfterCorrection = documentAfterCorrection;
            this.successful = successful;
        }

        public XmlDocument DocumentAfterCorrection
        {
            get
            {
                return documentAfterCorrection;
            }
        }

        public bool Successful
        {
            get
            {
                return successful;
            }
        }
    }
}