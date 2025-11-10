using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using WinUIEx;
using Microsoft.UI.Xaml.Media;

namespace proyectoRefaccionaria
{
    public sealed partial class SparePartsWindow : WindowEx
    {
        private List<SparePart> allParts = new();
        private List<CartItem> cart = new();
        private const string _mostrarTodas = "Mostrar Todas";

        public SparePartsWindow()
        {
            this.InitializeComponent();
            this.SystemBackdrop = new MicaBackdrop();

            CargarRefacciones();
            PoblarFiltroCategorias();
        }

        private void CargarRefacciones()
        {
            allParts = MySqlHelper.GetAllParts();
            AplicarFiltroCatalogo();
        }

        private void PoblarFiltroCategorias()
        {
            var categorias = allParts
                .Select(p => p.Categoria)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            CategoriaFilterComboBox.Items.Clear();
            CategoriaFilterComboBox.Items.Add(_mostrarTodas);

            foreach (var cat in categorias)
            {
                CategoriaFilterComboBox.Items.Add(cat);
            }

            CategoriaFilterComboBox.SelectedItem = _mostrarTodas;
        }

        private void CategoriaFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltroCatalogo();
        }

        private void AplicarFiltroCatalogo()
        {
            string filtroCategoria = CategoriaFilterComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(filtroCategoria) || filtroCategoria == _mostrarTodas)
            {
                PartsListView.ItemsSource = allParts;
            }
            else
            {
                var filtrado = allParts.Where(p => p.Categoria == filtroCategoria).ToList();
                PartsListView.ItemsSource = filtrado;
            }
        }

        private async void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            // (Este método no cambia)
            if (PartsListView.SelectedItem is SparePart selectedPart)
            {
                int quantityToAdd = (int)QuantityNumberBox.Value;
                var existingItem = cart.FirstOrDefault(item => item.Part.Id == selectedPart.Id);
                int currentQuantityInCart = existingItem?.Quantity ?? 0;
                int potentialNewTotal = currentQuantityInCart + quantityToAdd;

                if (potentialNewTotal > selectedPart.Stock)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Stock insuficiente",
                        Content = $"No puedes agregar {quantityToAdd} más.\n" +
                                  $"Solo quedan {selectedPart.Stock} en inventario, y ya tienes {currentQuantityInCart} en tu carrito.",
                        CloseButtonText = "Aceptar",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                    return;
                }

                if (existingItem != null)
                {
                    existingItem.Quantity += quantityToAdd;
                }
                else
                {
                    cart.Add(new CartItem { Part = selectedPart, Quantity = quantityToAdd });
                }
                ActualizarCartListView();
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            // (Este método no cambia)
            if (CartListView.SelectedItem is CartItem selectedCartItem)
            {
                cart.Remove(selectedCartItem);
                ActualizarCartListView();
            }
        }

        // ⬇⬇ --- MÉTODO 'CONFIRMAR COMPRA' ACTUALIZADO --- ⬇⬇
        private async void ConfirmPurchase_Click(object sender, RoutedEventArgs e)
        {
            if (cart.Count == 0)
            {
                var dialog = new ContentDialog { Title = "Carrito vacío", Content = "No hay nada que comprar.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            // 1. 🛑 VERIFICACIÓN FINAL (Sin cambios)
            // Sigue siendo importante para evitar que 2 personas compren lo mismo
            var freshPartsList = MySqlHelper.GetAllParts();
            foreach (var itemInCart in cart)
            {
                var freshPart = freshPartsList.FirstOrDefault(p => p.Id == itemInCart.Part.Id);
                if (freshPart == null || itemInCart.Quantity > freshPart.Stock)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "¡Venta Fallida! Stock modificado",
                        Content = $"Lo sentimos, mientras comprabas, el stock de '{itemInCart.Part.Nombre}' cambió. " +
                                  $"Solo quedan {freshPart?.Stock ?? 0}. \n\nSe vaciará tu carrito. Por favor, vuelve a intentarlo.",
                        CloseButtonText = "Aceptar",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    cart.Clear();
                    ActualizarCartListView();
                    CargarRefacciones();
                    PoblarFiltroCategorias();
                    return;
                }
            }

            // 2. 🚀 Llama al Helper para que haga TODO en una transacción
            // (Ya no actualizamos el stock aquí, el helper lo hace)
            bool ventaRegistrada = MySqlHelper.RegistrarVenta(cart);

            if (ventaRegistrada)
            {
                // 3. Diálogo de éxito
                double total = cart.Sum(item => item.Subtotal); // Calcula el total solo para mostrarlo
                var successDialog = new ContentDialog
                {
                    Title = "Compra confirmada",
                    Content = $"La compra se ha registrado con éxito. Total: ${total:F2}",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await successDialog.ShowAsync();

                // 4. Limpia el carrito y recarga
                cart.Clear();
                ActualizarCartListView();
                CargarRefacciones();
                PoblarFiltroCategorias();
            }
            else
            {
                // 5. ¡Error! La transacción falló.
                var errorDialog = new ContentDialog
                {
                    Title = "Error en la Venta",
                    Content = "No se pudo registrar la venta. La base de datos revirtió los cambios. El stock no ha sido modificado. Por favor, intenta de nuevo.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await errorDialog.ShowAsync();
                // No limpiamos el carrito, el usuario puede reintentar.
            }
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            // (Este método no cambia)
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

        private void ActualizarCartListView()
        {
            // (Este método no cambia)
            CartListView.ItemsSource = new List<CartItem>(cart);
        }
    }

    // (La clase CartItem no cambia)
    public class CartItem
    {
        public SparePart Part
        {
            get; set;
        }
        public int Quantity
        {
            get; set;
        }
        public string DisplayName => $"{Part.Nombre} (x{Quantity})";
        public double Subtotal => Part.Precio * Quantity;
    }
}