using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using proyectoRefaccionaria.Helpers;
using Windows.UI.ViewManagement;
using WinUIEx;
using Microsoft.UI.Xaml.Media;

namespace proyectoRefaccionaria
{
    public sealed partial class MainWindow : WindowEx
    {
        private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;
        private UISettings settings;

        public MainWindow()
        {
            this.InitializeComponent();
            this.SystemBackdrop = new MicaBackdrop();

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
            // ⬇⬇ ¡AQUÍ ESTÁ LA SOLUCIÓN! ⬇⬇
            // 1. Deshabilitamos el botón para prevenir dobles clics
            LoginButton.IsEnabled = false;

            string username = UserTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                var dialog = new ContentDialog
                {
                    Title = "Campos vacíos",
                    Content = "Por favor ingresa usuario y contraseña.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = (this.Content as FrameworkElement)?.XamlRoot
                };
                await dialog.ShowAsync();

                // 2. Si falla, lo volvemos a habilitar
                LoginButton.IsEnabled = true;
                return;
            }

            string rol = MySqlHelper.ValidarUsuario(username, password);

            if (rol == "admin")
            {
                var dialog = new ContentDialog
                {
                    Title = "Inicio de sesión",
                    Content = "Bienvenido, administrador. Accediendo al panel de gestión...",
                    CloseButtonText = "Aceptar",
                    XamlRoot = (this.Content as FrameworkElement)?.XamlRoot
                };
                await dialog.ShowAsync();

                var registerWindow = new RegisterPartWindow();
                registerWindow.Activate();
                this.Close();
                // 3. Si tiene éxito, no necesitamos habilitarlo (la ventana se cierra)
            }
            else if (rol == "usuario")
            {
                var dialog = new ContentDialog
                {
                    Title = "Inicio de sesión",
                    Content = "Bienvenido, usuario. Accediendo al catálogo...",
                    CloseButtonText = "Aceptar",
                    XamlRoot = (this.Content as FrameworkElement)?.XamlRoot
                };
                await dialog.ShowAsync();

                var sparePartsWindow = new SparePartsWindow();
                sparePartsWindow.Activate();
                this.Close();
                // 3. Si tiene éxito, no necesitamos habilitarlo (la ventana se cierra)
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Usuario o contraseña incorrectos.",
                    CloseButtonText = "Intentar de nuevo",
                    XamlRoot = (this.Content as FrameworkElement)?.XamlRoot
                };
                await dialog.ShowAsync();

                // 2. Si falla, lo volvemos a habilitar
                LoginButton.IsEnabled = true;
            }
        }
    }
}