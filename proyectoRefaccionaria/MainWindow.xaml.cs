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

            // Si tienes un helper para AppWindow o para el icono, reinstálalo aquí cuando exista.
            // AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));

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
            // Mostrar diálogo de confirmación de inicio de sesión
            var dialog = new ContentDialog
            {
                Title = "Inicio de sesión",
                Content = "Has iniciado sesión con éxito.",
                CloseButtonText = "Aceptar",
                XamlRoot = (this.Content as FrameworkElement)?.XamlRoot
            };

            await dialog.ShowAsync();

            // Abrir la siguiente ventana (menú de refacciones)
            var sparePartsWindow = new SparePartsWindow();
            sparePartsWindow.Activate();

            // Cerrar la ventana actual (login)
            this.Close();
        }
    }
}
