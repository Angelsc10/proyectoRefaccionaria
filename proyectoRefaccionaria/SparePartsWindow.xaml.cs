using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq; // ⬅️ MUY IMPORTANTE AÑADIR ESTO
using WinUIEx;
using Microsoft.UI.Xaml.Media; // ⬅️ 1. AÑADE ESTA LÍNEA 'USING'

namespace proyectoRefaccionaria
{
    public sealed partial class SparePartsWindow : WindowEx
    {
        private List<SparePart> allParts = new();
        private List<CartItem> cart = new();

        public SparePartsWindow()
        {
            this.InitializeComponent();

            // ⬇️ 2. AÑADE ESTA LÍNEA
            // Esta es la forma nativa de WinUI 3 de activar Mica
            this.SystemBackdrop = new MicaBackdrop();

            CargarRefacciones();
        }

        private void CargarRefacciones()
        {
            allParts = MySqlHelper.GetAllParts();
            PartsListView.ItemsSource = allParts;
        }

        // ⬇⬇ LÓGICA DE AGREGAR AL CARRITO (ACTUALIZADA CON VERIFICACIÓN DE STOCK) ⬇⬇
        //
        private async void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (PartsListView.SelectedItem is SparePart selectedPart)
            {
                int quantityToAdd = (int)QuantityNumberBox.Value;

                // 1. Buscar si el item YA existe en el carrito
                var existingItem = cart.FirstOrDefault(item => item.Part.Id == selectedPart.Id);
                int currentQuantityInCart = existingItem?.Quantity ?? 0;

                // 2. Calcular el total que *habría* en el carrito
                int potentialNewTotal = currentQuantityInCart + quantityToAdd;

                // 3. 🛑 ¡LA VERIFICACIÓN DE STOCK! 🛑
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
                    return; // Detiene la función
                }

                // 4. Si hay stock, continúa como antes
                if (existingItem != null)
                {
                    existingItem.Quantity += quantityToAdd;
                }
                else
                {
                    cart.Add(new CartItem { Part = selectedPart, Quantity = quantityToAdd });
                }

                // 5. Actualiza el ListView del carrito
                ActualizarCartListView();
            }
        }

        // LÓGICA DE QUITAR DEL CARRITO (Sin cambios)
        //
        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (CartListView.SelectedItem is CartItem selectedCartItem)
            {
                cart.Remove(selectedCartItem);
                ActualizarCartListView();
            }
        }

        // ⬇⬇ LÓGICA DE CONFIRMAR COMPRA (¡ACTUALIZADA PARA REDUCIR STOCK!) ⬇⬇
        //
        private async void ConfirmPurchase_Click(object sender, RoutedEventArgs e)
        {
            if (cart.Count == 0)
            {
                var dialog = new ContentDialog { Title = "Carrito vacío", Content = "No hay nada que comprar.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            // 1. 🛑 VERIFICACIÓN FINAL (¿Alguien compró o ajustó el stock mientras elegías?)
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
                    CargarRefacciones(); // Recarga la lista principal con el stock real
                    return; // Detiene la compra
                }
            }

            // 2. Si toda la validación pasa, "Confirmar" la venta (reducir el stock)
            double total = 0;
            foreach (var itemInCart in cart)
            {
                // Actualiza el objeto en memoria
                itemInCart.Part.Stock -= itemInCart.Quantity;

                // Manda la actualización a la Base de Datos
                MySqlHelper.UpdatePart(itemInCart.Part);

                // Suma al total
                total += itemInCart.Subtotal;
            }

            // 3. Diálogo de éxito
            var successDialog = new ContentDialog
            {
                Title = "Compra confirmada",
                Content = $"La compra se ha registrado con éxito. Total: ${total:F2}",
                CloseButtonText = "Aceptar",
                XamlRoot = this.Content.XamlRoot
            };
            await successDialog.ShowAsync();

            // 4. Limpia el carrito y recarga la lista principal (para ver el nuevo stock)
            cart.Clear();
            ActualizarCartListView();
            CargarRefacciones();
        }

        // LÓGICA DE LOGOUT (Sin cambios)
        //
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

        // MÉTODO HELPER (Sin cambios)
        //
        private void ActualizarCartListView()
        {
            CartListView.ItemsSource = new List<CartItem>(cart);
        }
    }

    // CLASE CartItem (Sin cambios)
    //
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
        public string DisplayName
        {
            get
            {
                return $"{Part.Nombre} (x{Quantity})";
            }
        }
        public double Subtotal
        {
            get
            {
                return Part.Precio * Quantity;
            }
        }
    }
}