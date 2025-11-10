using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using proyectoRefaccionaria.Helpers;
using Windows.UI.ViewManagement;
using WinUIEx;
using Microsoft.UI.Xaml.Media; // ⬅️ 1. AÑADE ESTA LÍNEA 'USING'

namespace proyectoRefaccionaria
{
    public sealed partial class MainWindow : WindowEx
    {
        private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;
        private UISettings settings;

        public MainWindow()
        {
            this.InitializeComponent();

            // ⬇️ 2. AÑADE ESTA LÍNEA
            // Esta es la forma nativa de WinUI 3 de activar Mica
            this.SystemBackdrop = new MicaBackdrop();


            // El resto de tu código original
            // AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));

            dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            settings = new UISettings();
            settings.ColorValuesChanged += Settings_ColorValuesChanged;
        }

        // El resto de tu archivo (Settings_ColorValuesChanged, LogButton_Click)
        // permanece exactamente igual que en tu archivo original.

        private void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                TitleBarHelper.ApplySystemThemeToCaptionButtons();
            });
        }

        private async void LogButton_Click(object sender, RoutedEventArgs e)
        {
            // Obtener usuario y contraseña desde los TextBox del XAML
            string username = UserTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            // Validación básica
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
                return;
            }

            // Verificar credenciales
            if (username == "usuario" && password == "123")
            {
                var dialog = new ContentDialog
                {
                    Title = "Inicio de sesión",
                    Content = "Bienvenido, usuario. Accediendo al catálogo...",
                    CloseButtonText = "Aceptar",
                    XamlRoot = (this.Content as FrameworkElement)?.XamlRoot
                };
                await dialog.ShowAsync();

                // Abrir ventana del catálogo
                var sparePartsWindow = new SparePartsWindow();
                sparePartsWindow.Activate();
                this.Close();
            }
            else if (username == "admin" && password == "123")
            {
                var dialog = new ContentDialog
                {
                    Title = "Inicio de sesión",
                    Content = "Bienvenido, administrador. Accediendo al registro de información...",
                    CloseButtonText = "Aceptar",
                    XamlRoot = (this.Content as FrameworkElement)?.XamlRoot
                };
                await dialog.ShowAsync();

                // Abrir ventana de registro
                var registerWindow = new RegisterPartWindow();
                registerWindow.Activate();
                this.Close();
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
            }
        }
    }
}