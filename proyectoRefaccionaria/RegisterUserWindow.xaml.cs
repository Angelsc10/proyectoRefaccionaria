using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media; // Para el Mica
using WinUIEx;

namespace proyectoRefaccionaria
{
    // ⬇⬇ ¡ASEGÚRATE QUE DIGA 'WindowEx'! ⬇⬇
    public sealed partial class RegisterUserWindow : WindowEx
    {
        public RegisterUserWindow()
        {
            this.InitializeComponent();
            this.Maximize();
            this.CenterOnScreen(); 
        }

        /// <summary>
        /// Se activa al hacer clic en 'Registrarme'
        /// </summary>
        private async void ConfirmRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string pass1 = PasswordBox1.Password;
            string pass2 = PasswordBox2.Password;

            // 1. Validar campos vacíos
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pass1) || string.IsNullOrEmpty(pass2))
            {
                var dialog = new ContentDialog { Title = "Campos vacíos", Content = "Por favor completa todos los campos.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            // 2. Validar que las contraseñas coincidan
            if (pass1 != pass2)
            {
                var dialog = new ContentDialog { Title = "Error de contraseña", Content = "Las contraseñas no coinciden.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            // 3. Intentar crear el usuario (¡SIEMPRE como "usuario"!)
            bool exito = MySqlHelper.AddUser(username, pass1, "usuario");

            if (exito)
            {
                var dialog = new ContentDialog { Title = "¡Éxito!", Content = "Tu cuenta ha sido creada. Ahora puedes iniciar sesión.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();

                this.Close(); // Cierra la ventana de registro
            }
            else
            {
                var dialog = new ContentDialog { Title = "Error", Content = "No se pudo crear la cuenta. Es posible que ese nombre de usuario ya exista.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Cierra la ventana de registro
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}