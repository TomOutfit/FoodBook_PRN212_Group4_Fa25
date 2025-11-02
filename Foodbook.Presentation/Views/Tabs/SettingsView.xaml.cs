using System.Windows;
using System.Windows.Controls;
using Foodbook.Presentation.ViewModels;

namespace Foodbook.Presentation.Views.Tabs
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
            {
                // Ensure the VM's Save command has completed if bound
                if (vm.SaveSettingsCommand != null && vm.SaveSettingsCommand.CanExecute(null))
                {
                    // Try to call the async method directly for certainty
                    await vm.SaveSettingsAsync();
                }

                // Apply theme and language only after saving
                var window = Window.GetWindow(this) as MainWindow;
                if (window != null)
                {
                    window.ApplyTheme(vm.SelectedTheme);
                    window.ApplyLanguage(vm.SelectedLanguage);
                }
            }
        }
    }
}


