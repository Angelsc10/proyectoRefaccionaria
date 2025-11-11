using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using WinUIEx;
using Microsoft.UI.Xaml.Media;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace proyectoRefaccionaria
{
    public sealed partial class SparePartsWindow : WindowEx
    {
        private List<SparePart> allParts = new();
        private List<CartItem> cart = new();
        private const string _mostrarTodas = "Mostrar Todas";
        private readonly Cliente _clienteAnonimo = new Cliente { ClienteID = -1, Nombre = "Cliente Anónimo (Mostrador)" };

        public SparePartsWindow()
        {
            this.InitializeComponent();
            this.Maximize();
            CargarRefacciones();
            PoblarFiltroCategorias();
            PoblarClientesComboBox();
        }

        // --- MÉTODOS DE FILTRO DE REFACCIONES (Sin cambios) ---
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
            List<SparePart> filtrado;

            if (string.IsNullOrEmpty(filtroCategoria) || filtroCategoria == _mostrarTodas)
            {
                filtrado = allParts.Where(p => p.Stock > 0).ToList();
            }
            else
            {
                filtrado = allParts.Where(p => p.Categoria == filtroCategoria && p.Stock > 0).ToList();
            }

            PartsListView.ItemsSource = filtrado;
        }

        // --- MÉTODO NUEVO: Cargar Clientes ---
        private void PoblarClientesComboBox()
        {
            List<Cliente> clientes = MySqlHelper.GetAllClientes();

            ClienteComboBox.Items.Clear();
            ClienteComboBox.Items.Add(_clienteAnonimo);

            foreach (var cliente in clientes)
            {
                ClienteComboBox.Items.Add(cliente);
            }

            ClienteComboBox.SelectedItem = _clienteAnonimo;
        }

        // --- MÉTODO NUEVO: Botón Añadir Cliente ---
        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            var customerWindow = new CustomerManagementWindow();

            customerWindow.Closed += (s, args) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    PoblarClientesComboBox();
                });
            };

            customerWindow.Activate();
        }


        // --- MÉTODOS DEL CARRITO (Sin cambios) ---
        private async void AddToCart_Click(object sender, RoutedEventArgs e)
        {
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
                        XamlRoot = this.Content.XamlRoot // ⬅️ Este diálogo SÍ lo tenía
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
            if (CartListView.SelectedItem is CartItem selectedCartItem)
            {
                cart.Remove(selectedCartItem);
                ActualizarCartListView();
            }
        }

        // --- MÉTODO 'CONFIRMAR COMPRA' (MODIFICADO) ---
        private async void ConfirmPurchase_Click(object sender, RoutedEventArgs e)
        {
            if (cart.Count == 0)
            {
                var dialog = new ContentDialog { Title = "Carrito vacío", Content = "No hay nada que comprar.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await dialog.ShowAsync();
                return;
            }

            var freshPartsList = MySqlHelper.GetAllParts();
            foreach (var itemInCart in cart)
            {
                var freshPart = freshPartsList.FirstOrDefault(p => p.Id == itemInCart.Part.Id);
                if (freshPart == null || itemInCart.Quantity > freshPart.Stock)
                {
                    var errorDialog = new ContentDialog { Title = "¡Venta Fallida! Stock modificado", Content = $"Lo sentimos, mientras comprabas, el stock de '{itemInCart.Part.Nombre}' cambió. " + $"Solo quedan {freshPart?.Stock ?? 0}. \n\nSe vaciará tu carrito. Por favor, vuelve a intentarlo.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                    await errorDialog.ShowAsync();
                    cart.Clear();
                    ActualizarCartListView();
                    CargarRefacciones();
                    PoblarFiltroCategorias();
                    return;
                }
            }

            int clienteIdParaVenta = -1;
            string nombreCliente = _clienteAnonimo.Nombre;

            if (ClienteComboBox.SelectedItem is Cliente selectedCliente)
            {
                clienteIdParaVenta = selectedCliente.ClienteID;
                nombreCliente = selectedCliente.Nombre;
            }

            int ventaId = MySqlHelper.RegistrarVenta(cart, clienteIdParaVenta);

            if (ventaId > -1)
            {
                double total = cart.Sum(item => item.Subtotal);
                string ticketPath = GenerarTicket(cart, ventaId, total, nombreCliente);

                var successDialog = new ContentDialog
                {
                    Title = "Compra confirmada",
                    Content = $"La compra (Venta ID: {ventaId}) para {nombreCliente} se ha registrado con éxito. Total: ${total:F2}\n\nSe guardó un ticket en: {ticketPath}",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await successDialog.ShowAsync();

                cart.Clear();
                ActualizarCartListView();
                CargarRefacciones();
                PoblarFiltroCategorias();
                PoblarClientesComboBox();
            }
            else
            {
                var errorDialog = new ContentDialog { Title = "Error en la Venta", Content = "No se pudo registrar la venta. La base de datos revirtió los cambios. El stock no ha sido modificado. Por favor, intenta de nuevo.", CloseButtonText = "Aceptar", XamlRoot = this.Content.XamlRoot };
                await errorDialog.ShowAsync();
            }
        }

        private string GenerarTicket(List<CartItem> carrito, int ventaId, double total, string nombreCliente)
        {
            string fileName = $"Venta_{ventaId}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, fileName);

            var sb = new StringBuilder();
            sb.AppendLine("*************************************");
            sb.AppendLine("      REFACCIONARIA EL GALLITO     ");
            sb.AppendLine("*************************************");
            sb.AppendLine($"Venta ID: {ventaId}");
            sb.AppendLine($"Fecha: {DateTime.Now:g}");
            sb.AppendLine($"Cliente: {nombreCliente}");
            sb.AppendLine("-------------------------------------");
            sb.AppendLine("Cant.  Producto              Subtotal");
            sb.AppendLine("-------------------------------------");

            foreach (var item in carrito)
            {
                string nombre = item.Part.Nombre.Length > 20 ? item.Part.Nombre.Substring(0, 20) : item.Part.Nombre.PadRight(20);
                string cantidad = item.Quantity.ToString().PadLeft(3);
                string subtotal = item.Subtotal.ToString("C2").PadLeft(10);

                sb.AppendLine($"{cantidad}   {nombre}  {subtotal}");
            }

            sb.AppendLine("-------------------------------------");
            sb.AppendLine($"TOTAL: {total:C2}".PadLeft(36));
            sb.AppendLine("\nGracias por su compra!");
            sb.AppendLine("*************************************");

            try
            {
                File.WriteAllText(filePath, sb.ToString());
                return filePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al guardar ticket: {ex.Message}");
                return "Error (no se pudo guardar el ticket)";
            }
        }

        // ⬇⬇ --- MÉTODO 'LOGOUT' (CORREGIDO) --- ⬇⬇
        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Cerrar sesión",
                Content = "¿Seguro que quieres cerrar sesión?",
                PrimaryButtonText = "Sí",
                CloseButtonText = "Cancelar",
                // ¡ESTA LÍNEA ES LA CORRECCIÓN!
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
            CartListView.ItemsSource = new List<CartItem>(cart);
        }
    }

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