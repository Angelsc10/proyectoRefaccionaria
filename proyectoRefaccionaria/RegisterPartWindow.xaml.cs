using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace proyectoRefaccionaria
{
    public sealed partial class RegisterPartWindow : WindowEx
    {
        private string dataFolder = Path.Combine(AppContext.BaseDirectory, "data");
        private string partsFile = "";

        public RegisterPartWindow()
        {
            this.InitializeComponent();
            partsFile = Path.Combine(dataFolder, "parts.json");
            Directory.CreateDirectory(dataFolder);
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            // Validar campos
            if (string.IsNullOrWhiteSpace(IdTextBox.Text) ||
                string.IsNullOrWhiteSpace(NombreTextBox.Text) ||
                string.IsNullOrWhiteSpace(PrecioTextBox.Text))
            {
                await MostrarDialogo("Error de validación", "Todos los campos son obligatorios.");
                return;
            }

            if (!int.TryParse(IdTextBox.Text, out int id))
            {
                await MostrarDialogo("Error", "El ID debe ser un número entero.");
                return;
            }

            if (!double.TryParse(PrecioTextBox.Text, out double precio))
            {
                await MostrarDialogo("Error", "El precio debe ser un número válido.");
                return;
            }

            // Cargar lista existente (si existe)
            List<SparePart> parts = new();
            if (File.Exists(partsFile))
            {
                string existingJson = File.ReadAllText(partsFile);
                if (!string.IsNullOrEmpty(existingJson))
                {
                    parts = JsonSerializer.Deserialize<List<SparePart>>(existingJson) ?? new List<SparePart>();
                }
            }

            // Agregar nueva refacción
            parts.Add(new SparePart
            {
                Id = id,
                Nombre = NombreTextBox.Text.Trim(),
                Precio = precio
            });

            // Guardar en JSON
            string json = JsonSerializer.Serialize(parts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(partsFile, json);

            StatusText.Text = "Refacción registrada correctamente";

            // Limpiar campos
            IdTextBox.Text = "";
            NombreTextBox.Text = "";
            PrecioTextBox.Text = "";
        }

        private async System.Threading.Tasks.Task MostrarDialogo(string titulo, string mensaje)
        {
            var dialog = new ContentDialog
            {
                Title = titulo,
                Content = mensaje,
                CloseButtonText = "Aceptar",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void VerRefacciones_Click(object sender, RoutedEventArgs e)
        {
            var viewWindow = new ViewPartsWindow();
            viewWindow.Activate();
        }

    }
}
