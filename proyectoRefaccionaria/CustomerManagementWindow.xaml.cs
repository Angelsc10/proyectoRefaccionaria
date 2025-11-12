using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using WinUIEx;

namespace proyectoRefaccionaria
{
    public sealed partial class CustomerManagementWindow : WindowEx
    {
        // Constructor MODIFICADO: Acepta un parámetro opcional
        public CustomerManagementWindow(bool modoVendedor = false)
        {
            this.InitializeComponent();
            this.Maximize();

            // Si es modo vendedor, ocultamos el botón de eliminar
            if (modoVendedor)
            {
                EliminarButton.Visibility = Visibility.Collapsed;
                this.Title = "Refaccionaria El Gallito - Seleccionar Cliente";
            }

            CargarClientes();
        }

        private void CargarClientes()
        {
            List<Cliente> clientes = MySqlHelper.GetAllClientes();
            ClientesDataGrid.ItemsSource = clientes;
        }

        private async void AgregarCliente_Click(object sender, RoutedEventArgs e)
        {
            string nombre = NombreTextBox.Text.Trim();
            string telefono = TelefonoTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string rfc = RfcTextBox.Text.Trim();

            if (string.IsNullOrEmpty(nombre))
            {
                var dialog = new ContentDialog { Title = "Campo obligatorio", Content = "El nombre del cliente no puede estar vacío.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            bool exito = MySqlHelper.AddCliente(nombre, telefono, email, rfc);

            if (exito)
            {
                var dialog = new ContentDialog { Title = "Éxito", Content = "Cliente creado correctamente.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();

                NombreTextBox.Text = "";
                TelefonoTextBox.Text = "";
                EmailTextBox.Text = "";
                RfcTextBox.Text = "";

                CargarClientes();
            }
            else
            {
                var dialog = new ContentDialog { Title = "Error", Content = "No se pudo crear el cliente.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
            }
        }

        private async void EliminarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (ClientesDataGrid.SelectedItem is Cliente selectedCliente)
            {
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
                    bool exito = MySqlHelper.DeleteCliente(selectedCliente.ClienteID);

                    if (exito)
                    {
                        CargarClientes();
                    }
                    else
                    {
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