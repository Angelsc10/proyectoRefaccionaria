using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media; // ⬅️ Para el Mica
using System.Collections.Generic;
using WinUIEx;

namespace proyectoRefaccionaria
{
    // ⬇⬇ ¡ASEGÚRATE QUE DIGA 'WindowEx'! ⬇⬇
    public sealed partial class UserManagementWindow : WindowEx
    {
        public UserManagementWindow()
        {
            this.InitializeComponent();
            this.SystemBackdrop = new MicaBackdrop(); // Activa Mica

            CargarUsuarios();
        }

        /// <summary>
        /// Carga (o recarga) la lista de usuarios desde la BD al DataGrid.
        /// </summary>
        private void CargarUsuarios()
        {
            List<Usuario> usuarios = MySqlHelper.GetAllUsers();
            UsuariosDataGrid.ItemsSource = usuarios;
        }

        /// <summary>
        /// Se activa al hacer clic en 'Crear Usuario'.
        /// </summary>
        private async void AgregarUsuario_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string rol = RolComboBox.SelectedItem?.ToString();

            // Validación de campos
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(rol))
            {
                var dialog = new ContentDialog { Title = "Campos vacíos", Content = "Por favor completa todos los campos.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            // Llama al Helper para añadir el usuario
            bool exito = MySqlHelper.AddUser(username, password, rol);

            if (exito)
            {
                var dialog = new ContentDialog { Title = "Éxito", Content = "Usuario creado correctamente.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();

                // Limpia el formulario
                UsernameTextBox.Text = "";
                PasswordBox.Password = "";
                RolComboBox.SelectedIndex = -1;

                // Recarga la lista
                CargarUsuarios();
            }
            else
            {
                // Esto usualmente falla si el 'Username' ya existe
                var dialog = new ContentDialog { Title = "Error", Content = "No se pudo crear el usuario. Es posible que el 'Username' ya exista.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Se activa al hacer clic en 'Eliminar Seleccionado'.
        /// </summary>
        private async void EliminarUsuario_Click(object sender, RoutedEventArgs e)
        {
            // 1. Asegúrate de que algo esté seleccionado
            if (UsuariosDataGrid.SelectedItem is Usuario selectedUser)
            {
                // 2. No permitas que el admin se borre a sí mismo (o al primer 'admin')
                if (selectedUser.Username.ToLower() == "admin")
                {
                    var dialog = new ContentDialog { Title = "Acción no permitida", Content = "No puedes eliminar al usuario 'admin' principal.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                    await dialog.ShowAsync();
                    return;
                }

                // 3. Pide confirmación
                var confirmDialog = new ContentDialog
                {
                    Title = "Confirmar eliminación",
                    Content = $"¿Estás seguro de eliminar al usuario '{selectedUser.Username}'?",
                    PrimaryButtonText = "Sí, eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    // 4. Llama al Helper para eliminar
                    MySqlHelper.DeleteUser(selectedUser.UsuarioID);

                    // 5. Recarga la lista
                    CargarUsuarios();
                }
            }
            else
            {
                var dialog = new ContentDialog { Title = "Ninguna selección", Content = "Por favor selecciona un usuario de la lista para eliminar.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
            }
        }
    }
}