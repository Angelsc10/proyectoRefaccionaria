using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media; // ⬅️ Para el Mica
using System.Collections.Generic;
using WinUIEx;

namespace proyectoRefaccionaria
{
    // ⬇⬇ LA LÍNEA MÁS IMPORTANTE DEL CAMBIO ESTÁ AQUÍ ⬇⬇
    public sealed partial class SalesReportWindow : WindowEx // Debe decir 'WindowEx', no 'Window'
    {
        public SalesReportWindow()
        {
            this.InitializeComponent();
            this.SystemBackdrop = new MicaBackdrop(); // Activa Mica

            CargarReporteVentas();
        }

        /// <summary>
        /// Carga la lista principal de todas las ventas (tickets).
        /// </summary>
        private void CargarReporteVentas()
        {
            // Llama al método que creamos en MySqlHelper
            List<VentaReporte> ventas = MySqlHelper.GetVentasReporte();
            VentasDataGrid.ItemsSource = ventas;
        }

        /// <summary>
        /// Se activa cuando el admin hace clic en una venta de la lista izquierda.
        /// </summary>
        private void VentasDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 1. Asegúrate de que algo esté seleccionado
            if (VentasDataGrid.SelectedItem is VentaReporte selectedVenta)
            {
                // 2. Actualiza el cabezal del detalle
                DetalleVentaHeader.Text = $"Mostrando detalle de la Venta ID: {selectedVenta.VentaID}";

                // 3. Llama al helper para obtener los detalles de ESA venta
                List<DetalleVentaReporte> detalles = MySqlHelper.GetDetalleVentaReporte(selectedVenta.VentaID);

                // 4. Muestra los detalles en la cuadrícula derecha
                DetalleVentaDataGrid.ItemsSource = detalles;
            }
            else
            {
                // 5. Si no hay nada seleccionado, limpia la cuadrícula derecha
                DetalleVentaHeader.Text = "Selecciona una venta de la izquierda para ver su detalle.";
                DetalleVentaDataGrid.ItemsSource = null;
            }
        }
    }
}