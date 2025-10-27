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

        public SparePartsWindow()
        {
            this.InitializeComponent();

            availableParts = new List<SparePart>
            {
                new SparePart { Id = 1, Nombre = "Filtro de aceite", Precio = 150 },
                new SparePart { Id = 2, Nombre = "Bujía", Precio = 80 },
                new SparePart { Id = 3, Nombre = "Balata delantera", Precio = 350 },
                new SparePart { Id = 4, Nombre = "Aceite sintético", Precio = 450 }
            };

            PartsListView.ItemsSource = availableParts;
            CartListView.ItemsSource = cart;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (PartsListView.SelectedItem is SparePart part && !cart.Contains(part))
            {
                cart.Add(part);
                RefreshCart();
                SaveCartToJson();
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (CartListView.SelectedItem is SparePart part)
            {
                cart.Remove(part);
                RefreshCart();
                SaveCartToJson();
            }
        }

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

            string dataFolder = Path.Combine(AppContext.BaseDirectory, "data");
            Directory.CreateDirectory(dataFolder);
            string filePath = Path.Combine(dataFolder, "ticket.json");
            File.WriteAllText(filePath, json);

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

        private void RefreshCart()
        {
            CartListView.ItemsSource = null;
            CartListView.ItemsSource = cart;
        }

        private void SaveCartToJson()
        {
            string dataFolder = Path.Combine(AppContext.BaseDirectory, "data");
            Directory.CreateDirectory(dataFolder);
            string cartPath = Path.Combine(dataFolder, "cart.json");
            string json = JsonSerializer.Serialize(cart, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(cartPath, json);
        }
    }

    public class SparePart
    {
        public int Id
        {
            get; set;
        }
        public string Nombre
        {
            get; set;
        }
        public double Precio
        {
            get; set;
        }
    }
}
