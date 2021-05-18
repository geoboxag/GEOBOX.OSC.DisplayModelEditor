using System.Xml;

namespace GEOBOX.OSC.DisplayModelEditor.CorrectionTasks
{
    public interface ICorrectionTaskResult
    {
        bool Successful { get; }
        XmlDocument DocumentAfterCorrection { get; }

    }
}