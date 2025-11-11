using Microsoft;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinUIEx;
using Microsoft.UI.Xaml.Media;

namespace proyectoRefaccionaria
{
    public sealed partial class EditPartWindow : WindowEx
    {
        private SparePart partToEdit;

        public EditPartWindow(SparePart part)
        {
            this.InitializeComponent();
            this.Maximize();
            this.partToEdit = part;
            LoadData();
        }

        private void LoadData()
        {
            NombreTextBox.Text = partToEdit.Nombre;
            PrecioNumberBox.Value = partToEdit.Precio;
            StockNumberBox.Value = partToEdit.Stock;
            // ⬇⬇ LÍNEA NUEVA: Selecciona la categoría guardada ⬇⬇
            CategoriaComboBox.SelectedItem = partToEdit.Categoria;
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NombreTextBox.Text))
            {
                var dialog = new ContentDialog { Title = "Nombre inválido", Content = "El nombre no puede estar vacío.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            // 1. Actualiza el objeto 'partToEdit' con los nuevos valores
            partToEdit.Nombre = NombreTextBox.Text.Trim();
            partToEdit.Precio = PrecioNumberBox.Value;
            // ⬇⬇ LÍNEA NUEVA: Lee la categoría del ComboBox ⬇⬇
            partToEdit.Categoria = CategoriaComboBox.SelectedItem?.ToString();
            partToEdit.Stock = (int)StockNumberBox.Value;

            // 2. Llama al MySqlHelper para guardar en la BD
            MySqlHelper.UpdatePart(partToEdit);

            // 3. Avisa al usuario y cierra
            var successDialog = new ContentDialog { Title = "Éxito", Content = "Refacción actualizada correctamente.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
            await successDialog.ShowAsync();
            this.Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}