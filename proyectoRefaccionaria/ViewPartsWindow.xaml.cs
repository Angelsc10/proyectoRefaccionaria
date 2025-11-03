using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace proyectoRefaccionaria
{
    public sealed partial class ViewPartsWindow : WindowEx
    {
        private List<SparePart> allParts = new();

        public ViewPartsWindow()
        {
            this.InitializeComponent();
            CargarRefacciones();
        }

        // 🔹 Load all parts from JSON
        private void CargarRefacciones()
        {
            string dataFolder = Path.Combine(AppContext.BaseDirectory, "data");
            string partsFile = Path.Combine(dataFolder, "parts.json");

            if (File.Exists(partsFile))
            {
                string json = File.ReadAllText(partsFile);
                allParts = JsonSerializer.Deserialize<List<SparePart>>(json) ?? new List<SparePart>();
                PartsListView.ItemsSource = allParts;
            }
            else
            {
                allParts = new List<SparePart>();
                PartsListView.ItemsSource = allParts;
            }
        }

        // 🔹 Filter by name or price
        private void Filtrar_Click(object sender, RoutedEventArgs e)
        {
            string filtroNombre = FiltroNombre.Text.Trim().ToLower();
            double.TryParse(FiltroPrecio.Text, out double precioMax);

            var filtrado = allParts.Where(p =>
                (string.IsNullOrEmpty(filtroNombre) || p.Nombre.ToLower().Contains(filtroNombre)) &&
                (precioMax <= 0 || p.Precio <= precioMax)
            ).ToList();

            PartsListView.ItemsSource = filtrado;
        }

        // 🔹 Reset filters and show all
        private void MostrarTodo_Click(object sender, RoutedEventArgs e)
        {
            FiltroNombre.Text = "";
            FiltroPrecio.Text = "";
            PartsListView.ItemsSource = allParts;
        }

        // 🔹 Delete a selected record with confirmation
        private async void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            if (PartsListView.SelectedItem is SparePart selectedPart)
            {
                // Confirmation dialog
                var confirmDialog = new ContentDialog
                {
                    Title = "Confirmar eliminación",
                    Content = $"¿Estás seguro de eliminar la refacción \"{selectedPart.Nombre}\" de la lista?",
                    PrimaryButtonText = "Sí, eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    allParts.Remove(selectedPart);

                    string dataFolder = Path.Combine(AppContext.BaseDirectory, "data");
                    string partsFile = Path.Combine(dataFolder, "parts.json");
                    string json = JsonSerializer.Serialize(allParts, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(partsFile, json);

                    PartsListView.ItemsSource = null;
                    PartsListView.ItemsSource = allParts;

                    // Informational dialog
                    var infoDialog = new ContentDialog
                    {
                        Title = "Eliminación completada",
                        Content = $"La refacción \"{selectedPart.Nombre}\" fue eliminada correctamente.",
                        CloseButtonText = "Aceptar",
                        XamlRoot = this.Content.XamlRoot
                    };

                    await infoDialog.ShowAsync();
                }
            }
            else
            {
                // No item selected
                var warningDialog = new ContentDialog
                {
                    Title = "Ninguna selección",
                    Content = "Por favor selecciona una refacción para eliminar.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await warningDialog.ShowAsync();
            }
        }
    }
}
