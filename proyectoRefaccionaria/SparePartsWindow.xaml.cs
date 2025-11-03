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
    public sealed partial class SparePartsWindow : WindowEx
    {
        private List<SparePart> availableParts = new();
        private List<SparePart> cart = new();
        private string dataFolder = Path.Combine(AppContext.BaseDirectory, "data");
        private string partsFile = "";

        public SparePartsWindow()
        {
            this.InitializeComponent();

            partsFile = Path.Combine(dataFolder, "parts.json");
            Directory.CreateDirectory(dataFolder);
            LoadAvailableParts();

            CartListView.ItemsSource = cart;
        }

        // Load parts from JSON file
        private void LoadAvailableParts()
        {
            if (File.Exists(partsFile))
            {
                string json = File.ReadAllText(partsFile);
                availableParts = JsonSerializer.Deserialize<List<SparePart>>(json) ?? new List<SparePart>();
            }
            else
            {
                availableParts = new List<SparePart>();
            }

            PartsListView.ItemsSource = availableParts;
        }

        //  Add selected item to cart
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (PartsListView.SelectedItem is SparePart part && !cart.Contains(part))
            {
                cart.Add(part);
                RefreshCart();
                SaveCartToJson();
            }
        }

        // Remove selected item from cart
        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (CartListView.SelectedItem is SparePart part)
            {
                cart.Remove(part);
                RefreshCart();
                SaveCartToJson();
            }
        }

        // Confirm purchase and save ticket
        private async void ConfirmPurchase_Click(object sender, RoutedEventArgs e)
        {
            if (cart.Count == 0)
            {
                var emptyDialog = new ContentDialog
                {
                    Title = "Carrito vacío",
                    Content = "No hay refacciones en el carrito.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await emptyDialog.ShowAsync();
                return;
            }

            var ticket = new
            {
                Fecha = DateTime.Now,
                Items = cart,
                Total = cart.Sum(p => p.Precio)
            };

            string json = JsonSerializer.Serialize(ticket, new JsonSerializerOptions { WriteIndented = true });

            string ticketPath = Path.Combine(dataFolder, "ticket.json");
            File.WriteAllText(ticketPath, json);

            string resumen = string.Join(Environment.NewLine, cart.Select(p => $"- {p.Nombre}: ${p.Precio:F2}"));

            var dialog = new ContentDialog
            {
                Title = "Compra completada",
                Content = $"Fecha: {ticket.Fecha}\n\nArtículos:\n{resumen}\n\nTotal: ${ticket.Total:F2}",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();

            cart.Clear();
            RefreshCart();
            SaveCartToJson();
        }

        // Delete selected product from catalog (and update JSON)
        private async void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (PartsListView.SelectedItem is SparePart selectedPart)
            {
                var confirmDialog = new ContentDialog
                {
                    Title = "Confirmar eliminación",
                    Content = $"¿Estás seguro de eliminar la refacción \"{selectedPart.Nombre}\" del catálogo?",
                    PrimaryButtonText = "Sí, eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    availableParts.Remove(selectedPart);

                    string json = JsonSerializer.Serialize(availableParts, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(partsFile, json);

                    LoadAvailableParts();

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


        // Refresh cart display
        private void RefreshCart()
        {
            CartListView.ItemsSource = null;
            CartListView.ItemsSource = cart;
        }

        // Save current cart to JSON (cart.json)
        private void SaveCartToJson()
        {
            string cartPath = Path.Combine(dataFolder, "cart.json");
            string json = JsonSerializer.Serialize(cart, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(cartPath, json);
        }
    }
}
