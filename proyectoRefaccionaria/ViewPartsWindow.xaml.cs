using System;
using System.Collections.Generic;
using System.Linq;
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

        // 🔹 Load all parts from MySQL
        private void CargarRefacciones()
        {
            allParts = MySqlHelper.GetAllParts();
            PartsListView.ItemsSource = allParts;
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
                var confirmDialog = new ContentDialog
                {
                    Title = "Confirmar eliminación",
                    Content = $"¿Estás seguro de eliminar la refacción \"{selectedPart.Nombre}\" de la base de datos?",
                    PrimaryButtonText = "Sí, eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    MySqlHelper.DeletePart(selectedPart.Id);
                    CargarRefacciones();

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

        // ⬇⬇ AÑADE ESTE MÉTODO ⬇⬇
        private async void Editar_Click(object sender, RoutedEventArgs e)
        {
            if (PartsListView.SelectedItem is SparePart selectedPart)
            {
                // 1. Crea la nueva ventana y le pasa la refacción seleccionada
                var editWindow = new EditPartWindow(selectedPart);

                // 2. Suscríbete al evento 'Closed' de la ventana de edición.
                //    Este código se ejecutará DESPUÉS de que 'editWindow' se cierre.
                editWindow.Closed += (s, args) =>
                {
                    // 3. 'CargarRefacciones()' debe ejecutarse en el hilo
                    //    principal de la UI. Usamos DispatcherQueue para eso.
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        CargarRefacciones(); // ¡Refresca la lista!
                    });
                };

                // 4. Muestra la ventana de edición
                editWindow.Activate();
            }
            else
            {
                // (Esto es igual que antes)
                var warningDialog = new ContentDialog
                {
                    Title = "Ninguna selección",
                    Content = "Por favor selecciona una refacción para editar.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await warningDialog.ShowAsync();
            }
        }
    }
}
