using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinUIEx;

namespace proyectoRefaccionaria
{
    public sealed partial class RegisterPartWindow : WindowEx
    {
        public RegisterPartWindow()
        {
            this.InitializeComponent();
        }

        // ✅ Guardar una nueva refacción (ACTUALIZADO CON STOCK)
        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            // Validación de campos (sin cambios)
            if (string.IsNullOrWhiteSpace(NombreTextBox.Text) || string.IsNullOrWhiteSpace(PrecioTextBox.Text))
            {
                var dialog = new ContentDialog
                {
                    Title = "Campos incompletos",
                    Content = "Por favor llena todos los campos.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            if (!double.TryParse(PrecioTextBox.Text, out double precio))
            {
                var dialog = new ContentDialog
                {
                    Title = "Precio inválido",
                    Content = "El precio debe ser un número.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            // ⬇⬇ CAMBIO AQUÍ: Leer el stock del NumberBox ⬇⬇
            int stock = (int)StockTextBox.Value;

            var newPart = new SparePart
            {
                Nombre = NombreTextBox.Text.Trim(),
                Precio = precio,
                Stock = stock // ⬅️ AÑADIDO
            };

            MySqlHelper.AddPart(newPart);

            var successDialog = new ContentDialog
            {
                Title = "Éxito",
                Content = "Refacción registrada correctamente.",
                CloseButtonText = "Aceptar",
                XamlRoot = this.Content.XamlRoot
            };
            await successDialog.ShowAsync();

            // Limpiar los campos
            NombreTextBox.Text = "";
            PrecioTextBox.Text = "";
            StockTextBox.Value = 0; // ⬅️ AÑADIDO
        }

        // ✅ Abrir ventana de visualización (Sin cambios)
        private void VerRefacciones_Click(object sender, RoutedEventArgs e)
        {
            var viewWindow = new ViewPartsWindow();
            viewWindow.Activate();
        }

        // ✅ Logout (Sin cambios)
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