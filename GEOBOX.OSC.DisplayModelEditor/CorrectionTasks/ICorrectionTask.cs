namespace GEOBOX.OSC.DisplayModelEditor.CorrectionTasks
{
    public interface ICorrectionTask
    {
        ICorrectionTaskResult Apply(ICorrectionTaskContext context);
    }
}