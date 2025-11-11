using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media; // ⬅️ Para el Mica
using System.Collections.Generic;
using WinUIEx;

namespace proyectoRefaccionaria
{
    // ⬇⬇ ¡ASEGÚRATE QUE DIGA 'WindowEx'! ⬇⬇
    public sealed partial class CustomerManagementWindow : WindowEx
    {
        public CustomerManagementWindow()
        {
            this.InitializeComponent();
            this.Maximize();
            CargarClientes();
        }

        /// <summary>
        /// Carga (o recarga) la lista de clientes desde la BD al DataGrid.
        /// </summary>
        private void CargarClientes()
        {
            List<Cliente> clientes = MySqlHelper.GetAllClientes();
            ClientesDataGrid.ItemsSource = clientes;
        }

        /// <summary>
        /// Se activa al hacer clic en 'Crear Cliente'.
        /// </summary>
        private async void AgregarCliente_Click(object sender, RoutedEventArgs e)
        {
            string nombre = NombreTextBox.Text.Trim();
            string telefono = TelefonoTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string rfc = RfcTextBox.Text.Trim();

            // Validación (solo el nombre es obligatorio)
            if (string.IsNullOrEmpty(nombre))
            {
                var dialog = new ContentDialog { Title = "Campo obligatorio", Content = "El nombre del cliente no puede estar vacío.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            // Llama al Helper para añadir el cliente
            bool exito = MySqlHelper.AddCliente(nombre, telefono, email, rfc);

            if (exito)
            {
                var dialog = new ContentDialog { Title = "Éxito", Content = "Cliente creado correctamente.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();

                // Limpia el formulario
                NombreTextBox.Text = "";
                TelefonoTextBox.Text = "";
                EmailTextBox.Text = "";
                RfcTextBox.Text = "";

                // Recarga la lista
                CargarClientes();
            }
            else
            {
                var dialog = new ContentDialog { Title = "Error", Content = "No se pudo crear el cliente.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Se activa al hacer clic en 'Eliminar Seleccionado'.
        /// </summary>
        private async void EliminarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (ClientesDataGrid.SelectedItem is Cliente selectedCliente)
            {
                // Pide confirmación
                var confirmDialog = new ContentDialog
                {
                    Title = "Confirmar eliminación",
                    Content = $"¿Estás seguro de eliminar al cliente '{selectedCliente.Nombre}'?\n\nADVERTENCIA: No podrás eliminar clientes que ya tengan ventas registradas.",
                    PrimaryButtonText = "Sí, eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    // Llama al Helper para eliminar
                    bool exito = MySqlHelper.DeleteCliente(selectedCliente.ClienteID);

                    if (exito)
                    {
                        CargarClientes(); // Recarga la lista si fue exitoso
                    }
                    else
                    {
                        // Esto falla si el cliente tiene ventas
                        var dialog = new ContentDialog { Title = "Error", Content = "No se pudo eliminar al cliente. Asegúrate de que no tenga ventas asociadas.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                        await dialog.ShowAsync();
                    }
                }
            }
            else
            {
                var dialog = new ContentDialog { Title = "Ninguna selección", Content = "Por favor selecciona un cliente de la lista para eliminar.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
            }
        }
    }
}