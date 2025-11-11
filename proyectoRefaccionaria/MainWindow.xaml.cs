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
            this.Maximize(); // ⬅️ Mantenemos esto para que se vea grande

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
            LoginButton.IsEnabled = false;

            string username = UserTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                var dialog = new ContentDialog { Title = "Campos vacíos", Content = "Por favor ingresa usuario y contraseña.", CloseButtonText = "Aceptar", XamlRoot = (this.Content as FrameworkElement)?.XamlRoot };
                await dialog.ShowAsync();
                LoginButton.IsEnabled = true;
                return;
            }

            string rol = MySqlHelper.ValidarUsuario(username, password);

            if (rol == "admin")
            {
                var dialog = new ContentDialog { Title = "Inicio de sesión", Content = "Bienvenido, administrador. Accediendo al panel de gestión...", CloseButtonText = "Aceptar", XamlRoot = (this.Content as FrameworkElement)?.XamlRoot };
                await dialog.ShowAsync();
                var registerWindow = new RegisterPartWindow();
                registerWindow.Activate();
                this.Close();
            }
            else if (rol == "usuario")
            {
                var dialog = new ContentDialog { Title = "Inicio de sesión", Content = "Bienvenido, usuario. Accediendo al catálogo...", CloseButtonText = "Aceptar", XamlRoot = (this.Content as FrameworkElement)?.XamlRoot };
                await dialog.ShowAsync();
                var sparePartsWindow = new SparePartsWindow();
                sparePartsWindow.Activate();
                this.Close();
            }
            else
            {
                var dialog = new ContentDialog { Title = "Error", Content = "Usuario o contraseña incorrectos.", CloseButtonText = "Intentar de nuevo", XamlRoot = (this.Content as FrameworkElement)?.XamlRoot };
                await dialog.ShowAsync();
                LoginButton.IsEnabled = true;
            }
        }

        // ⬇⬇ MÉTODO NUEVO ⬇⬇
        /// <summary>
        /// Abre la ventana de auto-registro de usuario.
        /// </summary>
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerUserWindow = new RegisterUserWindow();
            registerUserWindow.Activate();
            // No cerramos esta ventana, el usuario puede querer
            // registrarse y luego iniciar sesión.
        }
    }
}