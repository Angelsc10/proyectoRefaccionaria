using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinUIEx;

namespace proyectoRefaccionaria
{
    public sealed partial class EditCustomerWindow : WindowEx
    {
        private Cliente _cliente;

        public EditCustomerWindow(Cliente cliente)
        {
            this.InitializeComponent();
            this.Maximize();
            this.CenterOnScreen();
            _cliente = cliente;
            CargarDatos();
        }

        private void CargarDatos()
        {
            NombreTextBox.Text = _cliente.Nombre;
            TelefonoTextBox.Text = _cliente.Telefono;
            EmailTextBox.Text = _cliente.Email;
            RfcTextBox.Text = _cliente.RFC;
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NombreTextBox.Text))
            {
                var dialog = new ContentDialog { Title = "Error", Content = "El nombre es obligatorio.", CloseButtonText = "OK", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            // Actualizar objeto
            _cliente.Nombre = NombreTextBox.Text.Trim();
            _cliente.Telefono = TelefonoTextBox.Text.Trim();
            _cliente.Email = EmailTextBox.Text.Trim();
            _cliente.RFC = RfcTextBox.Text.Trim();

            // Guardar en BD
            MySqlHelper.UpdateCliente(_cliente);

            var successDialog = new ContentDialog { Title = "Éxito", Content = "Cliente actualizado.", CloseButtonText = "OK", XamlRoot = this.Content.XamlRoot };
            await successDialog.ShowAsync();
            this.Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}