using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using proyectoRefaccionaria.Helpers;
using Windows.UI.ViewManagement;
using WinUIEx;

namespace proyectoRefaccionaria
{
    public sealed partial class MainWindow : WindowEx
    {
        private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;
        private UISettings settings;

        public MainWindow()
        {
            this.InitializeComponent();

            dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            settings = new UISettings();
            settings.ColorValuesChanged += Settings_ColorValuesChanged;
        }

        private void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                TitleBarHelper.ApplySystemThemeToCaptionButtons();
            });
        }

        private async void LogButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Inicio de sesión",
                Content = "Has iniciado sesión con éxito.",
                CloseButtonText = "Aceptar",
                XamlRoot = (this.Content as FrameworkElement)?.XamlRoot
            };

            await dialog.ShowAsync();

            var sparePartsWindow = new SparePartsWindow();
            sparePartsWindow.Activate();

            this.Close();
        }
    }
}
