using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinUIEx;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic; // ⬅️ Necesario para List
using System.Linq; // ⬅️ Necesario por si usamos OrderBy

namespace proyectoRefaccionaria
{
    public sealed partial class RegisterPartWindow : WindowEx
    {
        public RegisterPartWindow()
        {
            this.InitializeComponent();
            this.Maximize();
            CargarInventario(); // ⬅️ Carga inicial
        }

        // ⬇⬇ MÉTODO NUEVO ⬇⬇
        private void CargarInventario()
        {
            // Obtenemos todas las partes y las mostramos en el grid
            var lista = MySqlHelper.GetAllParts();
            // Opcional: Mostrar las últimas agregadas primero (si el ID es autoincremental)
            // lista = lista.OrderByDescending(p => p.Id).ToList(); 
            MiniInventarioGrid.ItemsSource = lista;
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NombreTextBox.Text) || string.IsNullOrWhiteSpace(PrecioTextBox.Text))
            {
                var dialog = new ContentDialog { Title = "Campos incompletos", Content = "Por favor llena todos los campos.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            if (!double.TryParse(PrecioTextBox.Text, out double precio))
            {
                var dialog = new ContentDialog { Title = "Precio inválido", Content = "El precio debe ser un número.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            int stock = (int)StockTextBox.Value;
            string categoria = CategoriaComboBox.SelectedItem?.ToString();

            var newPart = new SparePart
            {
                Nombre = NombreTextBox.Text.Trim(),
                Precio = precio,
                Categoria = categoria,
                Stock = stock
            };

            MySqlHelper.AddPart(newPart);

            var successDialog = new ContentDialog { Title = "Éxito", Content = "Refacción registrada correctamente.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
            await successDialog.ShowAsync();

            // Limpiar campos
            NombreTextBox.Text = "";
            PrecioTextBox.Text = "";
            StockTextBox.Value = 0;
            CategoriaComboBox.SelectedIndex = -1;

            CargarInventario(); // ⬅️ Recarga la tabla para mostrar el nuevo producto
        }

        private void VerRefacciones_Click(object sender, RoutedEventArgs e)
        {
            var viewWindow = new ViewPartsWindow();
            viewWindow.Activate();
        }

        private void VerReportes_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new SalesReportWindow();
            reportWindow.Activate();
        }

        private void GestionarUsuarios_Click(object sender, RoutedEventArgs e)
        {
            var userWindow = new UserManagementWindow();
            userWindow.Activate();
        }

        private void GestionarClientes_Click(object sender, RoutedEventArgs e)
        {
            var customerWindow = new CustomerManagementWindow();
            customerWindow.Activate();
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Cerrar sesión",
                Content = "¿Seguro que quieres cerrar sesión?",
                PrimaryButtonText = "Sí",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var login = new MainWindow();
                login.Activate();
                this.Close();
            }
        }
    }
}