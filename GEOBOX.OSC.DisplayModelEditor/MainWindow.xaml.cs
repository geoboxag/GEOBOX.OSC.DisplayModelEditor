using GEOBOX.OSC.DisplayModelEditor.Settings;
using GEOBOX.OSC.DisplayModelEditor.ViewModels;
using GEOBOX.OSC.DisplayModelEditor.Views;
using System.Windows;
using System.Windows.Input;
using AppResources = GEOBOX.OSC.DisplayModelEditor.Properties;

namespace GEOBOX.OSC.DisplayModelEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private SettingsController settingsContoller;

        public MainWindow()
        {
            InitializeComponent();
            settingsContoller = new SettingsController();

            // Set Windows Title and Version from Properties and Settings
            WindowTitleLabel.Content = settingsContoller.GetAssemblyWindowTitle();
            InfoVersionLabel.Content = settingsContoller.GetAssemblyVersionString();

            ShowStartUpView();
        }

        private void MainWindow_Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            OnClosed(e);
        }
        // Minimize Window
        private void MainWindow_Minimize(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
        // Drag Window (relocate)
        private void MainWindow_Drag(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        #region Navigation Panel Click Events
        private void ShowStartUpView()
        {
            HandleBorderNavigationLabels(NaviagtionOneClickMaintenanceLabel.Name);
            ShowOneClickMaintenanceView();
        }

        private void NavigationOneClickMaintenance_Click(object sender, MouseButtonEventArgs e)
        {
            HandleBorderNavigationLabels(NaviagtionOneClickMaintenanceLabel.Name);
            ShowOneClickMaintenanceView();
        }

        private void NavigationDisplayModelInfo_Click(object sender, MouseButtonEventArgs e)
        {
            //HandleBorderNavigationLabels(NavigationDisplayModelInfoLabel.Name);
            ShowDisplayModelInfoView();
        }

        private void NavigationJoinDisplayModelInfo_Click(object sender, MouseButtonEventArgs e)
        {
            HandleBorderNavigationLabels(NavigationJoinDisplayModelInfoLabel.Name);
            ShowJoinDisplayModelsView();
        }

        private void HandleBorderNavigationLabels(string name)
        {
            NaviagtionOneClickMaintenanceLabel.BorderThickness = GetBorderThickness(NaviagtionOneClickMaintenanceLabel.Name == name);
            //NavigationDisplayModelInfoLabel.BorderThickness = GetBorderThickness(NavigationDisplayModelInfoLabel.Name == name);
            NavigationJoinDisplayModelInfoLabel.BorderThickness = GetBorderThickness(NavigationJoinDisplayModelInfoLabel.Name == name);
        }

        private Thickness GetBorderThickness(/* true = active */bool isActiv)
        {
            if (isActiv)
            {
                return new Thickness(0, 0, 0, 3);
            }

            return new Thickness(0, 0, 0, 0);
        }
        private void ShowOneClickMaintenanceView()
        {
            OneClickMaintenanceView oneClickMaintenanceView = new OneClickMaintenanceView();
            ShowDetailsContentControl.Content = oneClickMaintenanceView;
        }

        private void ShowDisplayModelInfoView()
        {
            DisplayModelInfoView displayModelInfoView = new DisplayModelInfoView();
            ShowDetailsContentControl.Content = displayModelInfoView;
        }

        private void ShowJoinDisplayModelsView()
        {
            JoinDisplayModelsView joinDisplayModelsView = new JoinDisplayModelsView();
            ShowDetailsContentControl.Content = joinDisplayModelsView;
        }
        #endregion
    }
}