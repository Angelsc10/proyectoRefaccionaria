using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinUIEx;

namespace proyectoRefaccionaria
{
    public sealed partial class EditPartWindow : WindowEx
    {
        // Variable para guardar la refacción que estamos editando
        private SparePart partToEdit;

        // Constructor MODIFICADO: Acepta una 'SparePart'
        public EditPartWindow(SparePart part)
        {
            this.InitializeComponent();
            this.partToEdit = part; // Guarda la refacción
            LoadData(); // Carga sus datos en la UI
        }

        // Carga los datos de la refacción en los campos
        private void LoadData()
        {
            NombreTextBox.Text = partToEdit.Nombre;
            PrecioNumberBox.Value = partToEdit.Precio;
            StockNumberBox.Value = partToEdit.Stock;
        }

        // Botón Guardar: Valida y actualiza la BD
        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            // Validación (similar a la de RegisterPartWindow)
            if (string.IsNullOrWhiteSpace(NombreTextBox.Text))
            {
                var dialog = new ContentDialog { Title = "Nombre inválido", Content = "El nombre no puede estar vacío.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            // 1. Actualiza el objeto 'partToEdit' con los nuevos valores
            partToEdit.Nombre = NombreTextBox.Text.Trim();
            partToEdit.Precio = PrecioNumberBox.Value;
            partToEdit.Stock = (int)StockNumberBox.Value;

            // 2. Llama al MySqlHelper para guardar en la BD
            MySqlHelper.UpdatePart(partToEdit);

            // 3. Avisa al usuario y cierra
            var successDialog = new ContentDialog
            {
                Title = "Éxito",
                Content = "Refacción actualizada correctamente.",
                CloseButtonText = "Aceptar",
                XamlRoot = this.Content.XamlRoot
            };
            await successDialog.ShowAsync();

            this.Close(); // Cierra la ventana de edición
        }

        // Botón Cancelar: Simplemente cierra la ventana
        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}