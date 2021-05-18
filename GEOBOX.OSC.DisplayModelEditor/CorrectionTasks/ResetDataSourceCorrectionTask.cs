using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace GEOBOX.OSC.DisplayModelEditor.CorrectionTasks
{
    /// <summary>
    /// Corrects Tags with name <FeatureSourceResourceId>not GBX_DEV_FA</FeatureSourceResourceId>
    /// to <FeatureSourceResourceId>GBX_DEV_FA</FeatureSourceResourceId>
    /// </summary>
    public sealed class ResetDataSourceCorrectionTask : ICorrectionTask
    {
        public ICorrectionTaskResult Apply(ICorrectionTaskContext correctionTaskContext)
        {
            if (correctionTaskContext == null)
            {
                throw new ArgumentNullException(nameof(correctionTaskContext));
            }
            Contract.EndContractBlock();

            var correctionXmlDocument = (XmlDocument)correctionTaskContext.CurrentXmlDocument.CloneNode(/*deep*/true);
            var list = correctionXmlDocument.GetElementsByTagName("FeatureSourceResourceId").OfType<XmlNode>();

            foreach (var node in list)
            {
                string temp = "GBX_DEV_FA";
                if (!node.InnerText.Equals(temp))
                {
                    node.InnerText = temp;
                }
            }
            return new CorrectionTaskResult(/*successful*/true, correctionXmlDocument);
        }

    }
}