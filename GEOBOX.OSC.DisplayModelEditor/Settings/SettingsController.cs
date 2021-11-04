using System;

namespace GEOBOX.OSC.DisplayModelEditor.Settings
{
    public class SettingsController
    {

        private const string DEFAULTAPPLICATIONTITLE = "GEOBOX Application";
        internal bool ConfigIsOk = false;

        internal SettingsController()
        {
        }

        #region Get Application Title
        // Return the Windows Title from Properties
        internal string GetAssemblyWindowTitle()
        {
            try
            {
                return Properties.Resources.genModulName ?? DEFAULTAPPLICATIONTITLE;
            }
            catch
            {
                return DEFAULTAPPLICATIONTITLE;
            }
        }
        #endregion


        #region Get Assembly-Version

        // Return the actual version from EXE to Display in Form
        internal string GetAssemblyVersionString()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return String.Format("Version: {0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        #endregion Get Assembly-Version
    }
}
