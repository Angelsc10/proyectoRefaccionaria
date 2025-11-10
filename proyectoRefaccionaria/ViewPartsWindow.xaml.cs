using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;
using Microsoft.UI.Xaml.Media;

namespace proyectoRefaccionaria
{
    public sealed partial class ViewPartsWindow : WindowEx
    {
        private List<SparePart> allParts = new();
        private const string _mostrarTodas = "Mostrar Todas"; // Constante para filtros

        public ViewPartsWindow()
        {
            this.InitializeComponent();
            this.SystemBackdrop = new MicaBackdrop();

            CargarRefacciones();
            PoblarFiltroCategorias(); // ⬅️ NUEVA LLAMADA
        }

        // 🔹 Carga todas las partes de MySQL (Sin cambios en lógica)
        private void CargarRefacciones()
        {
            allParts = MySqlHelper.GetAllParts();
            PartsDataGrid.ItemsSource = allParts;
        }

        // ⬇⬇ MÉTODO NUEVO ⬇⬇
        // 🔹 Llena el ComboBox con las categorías de la BD
        private void PoblarFiltroCategorias()
        {
            // Obtiene todas las categorías únicas de la lista, ignora nulas/vacías
            var categorias = allParts
                .Select(p => p.Categoria)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            CategoriaFilterComboBox.Items.Clear();
            // Añade la opción "Mostrar Todas" al inicio
            CategoriaFilterComboBox.Items.Add(_mostrarTodas);

            // Añade el resto de categorías
            foreach (var cat in categorias)
            {
                CategoriaFilterComboBox.Items.Add(cat);
            }

            CategoriaFilterComboBox.SelectedItem = _mostrarTodas;
        }

        // 🔹 Filtra por nombre, precio Y/O categoría
        private void Filtrar_Click(object sender, RoutedEventArgs e)
        {
            string filtroNombre = FiltroNombre.Text.Trim().ToLower();
            double.TryParse(FiltroPrecio.Text, out double precioMax);

            // ⬇⬇ LÓGICA NUEVA: Obtener filtro de categoría ⬇⬇
            string filtroCategoria = CategoriaFilterComboBox.SelectedItem?.ToString();

            var filtrado = allParts.Where(p =>
            {
                // Condición de Nombre
                bool nombrePasa = string.IsNullOrEmpty(filtroNombre) || p.Nombre.ToLower().Contains(filtroNombre);

                // Condición de Precio
                bool precioPasa = precioMax <= 0 || p.Precio <= precioMax;

                // ⬇⬇ Condición de Categoría ⬇⬇
                bool categoriaPasa = string.IsNullOrEmpty(filtroCategoria) ||
                                     filtroCategoria == _mostrarTodas ||
                                     p.Categoria == filtroCategoria;

                return nombrePasa && precioPasa && categoriaPasa;
            }).ToList();

            PartsDataGrid.ItemsSource = filtrado;
        }

        // 🔹 Resetea todos los filtros
        private void MostrarTodo_Click(object sender, RoutedEventArgs e)
        {
            FiltroNombre.Text = "";
            FiltroPrecio.Text = "";
            CategoriaFilterComboBox.SelectedItem = _mostrarTodas; // ⬅️ LÍNEA NUEVA
            PartsDataGrid.ItemsSource = allParts;
        }

        //
        // --- El resto de tus métodos (Eliminar_Click, Editar_Click) ---
        // --- NO NECESITAN CAMBIOS ---
        //

        private async void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            if (PartsDataGrid.SelectedItem is SparePart selectedPart)
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
                var warningDialog = new ContentDialog { Title = "Ninguna selección", Content = "Por favor selecciona una refacción para eliminar.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await warningDialog.ShowAsync();
            }
        }

        private async void Editar_Click(object sender, RoutedEventArgs e)
        {
            if (PartsDataGrid.SelectedItem is SparePart selectedPart)
            {
                var editWindow = new EditPartWindow(selectedPart);
                editWindow.Closed += (s, args) =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        CargarRefacciones();
                        // ❗️ Recargamos el ComboBox por si se editó/añadió una categoría nueva
                        PoblarFiltroCategorias();
                    });
                };
                editWindow.Activate();
            }
            else
            {
                var warningDialog = new ContentDialog { Title = "Ninguna selección", Content = "Por favor selecciona una refacción para editar.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await warningDialog.ShowAsync();
            }
        }
    }
}