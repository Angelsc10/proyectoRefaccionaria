using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace proyectoRefaccionaria
{
    // --- CLASES PARA REPORTES Y GESTIÓN (Añadida 'Usuario') ---
    public class VentaReporte
    {
        public int VentaID
        {
            get; set;
        }
        public DateTime Fecha
        {
            get; set;
        }
        public double TotalVenta
        {
            get; set;
        }
    }

    public class DetalleVentaReporte
    {
        public string NombreRefaccion
        {
            get; set;
        }
        public int CantidadVendida
        {
            get; set;
        }
        public double PrecioEnLaVenta
        {
            get; set;
        }
        public double Subtotal => CantidadVendida * PrecioEnLaVenta;
    }

    // ⬇⬇ CLASE NUEVA ⬇⬇
    // Clase para el DataGrid de gestión de usuarios
    // (Omitimos el Password por seguridad, no queremos mostrarlo en la UI)
    public class Usuario
    {
        public int UsuarioID
        {
            get; set;
        }
        public string Username
        {
            get; set;
        }
        public string Rol
        {
            get; set;
        }
    }


    // --- CLASE PRINCIPAL DEL HELPER ---
    public static class MySqlHelper
    {
        private static string connectionString = "server=localhost;database=refaccionaria;user=root;password=1234;";

        // --- MÉTODOS DE REFACCIONES (Sin cambios) ---
        public static List<SparePart> GetAllParts()
        {
            var parts = new List<SparePart>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Id, Nombre, Precio, Categoria, Stock FROM spareparts";
                    using (var cmd = new MySqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            parts.Add(new SparePart
                            {
                                Id = reader.GetInt32("Id"),
                                Nombre = reader.GetString("Nombre"),
                                Precio = reader.GetDouble("Precio"),
                                Categoria = reader.IsDBNull(reader.GetOrdinal("Categoria")) ? string.Empty : reader.GetString("Categoria"),
                                Stock = reader.GetInt32("Stock")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al obtener partes: {ex.Message}");
            }
            return parts;
        }

        public static void AddPart(SparePart part)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO spareparts (Nombre, Precio, Categoria, Stock) VALUES (@nombre, @precio, @categoria, @stock)";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@nombre", part.Nombre);
                        cmd.Parameters.AddWithValue("@precio", part.Precio);
                        cmd.Parameters.AddWithValue("@categoria", (object)part.Categoria ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@stock", part.Stock);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al añadir parte: {ex.Message}");
            }
        }

        public static void UpdatePart(SparePart part)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE spareparts SET Nombre = @nombre, Precio = @precio, Categoria = @categoria, Stock = @stock WHERE Id = @id";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@nombre", part.Nombre);
                        cmd.Parameters.AddWithValue("@precio", part.Precio);
                        cmd.Parameters.AddWithValue("@categoria", (object)part.Categoria ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@stock", part.Stock);
                        cmd.Parameters.AddWithValue("@id", part.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al actualizar parte: {ex.Message}");
            }
        }

        public static void DeletePart(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM spareparts WHERE Id = @id";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al eliminar parte: {ex.Message}");
            }
        }

        // --- MÉTODO DE LOGIN (Sin cambios) ---
        public static string ValidarUsuario(string username, string password)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Rol FROM Usuarios WHERE Username = @username AND Password = @password";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            return result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al validar usuario: {ex.Message}");
            }
            return null;
        }

        // --- MÉTODOS DE VENTA Y REPORTE (Sin cambios) ---
        public static bool RegistrarVenta(List<CartItem> cart)
        {
            MySqlTransaction transaction = null;
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    double totalVenta = cart.Sum(item => item.Subtotal);
                    string ventaQuery = "INSERT INTO Ventas (TotalVenta) VALUES (@total)";
                    long ventaId;
                    using (var cmd = new MySqlCommand(ventaQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@total", totalVenta);
                        cmd.ExecuteNonQuery();
                        ventaId = cmd.LastInsertedId;
                    }
                    string detalleQuery = "INSERT INTO DetalleVenta (VentaID, RefaccionID, CantidadVendida, PrecioEnLaVenta) VALUES (@ventaId, @refaccionId, @cantidad, @precio)";
                    string stockQuery = "UPDATE spareparts SET Stock = Stock - @cantidad WHERE Id = @refaccionId";
                    foreach (var item in cart)
                    {
                        using (var cmd = new MySqlCommand(detalleQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ventaId", ventaId);
                            cmd.Parameters.AddWithValue("@refaccionId", item.Part.Id);
                            cmd.Parameters.AddWithValue("@cantidad", item.Quantity);
                            cmd.Parameters.AddWithValue("@precio", item.Part.Precio);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new MySqlCommand(stockQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@cantidad", item.Quantity);
                            cmd.Parameters.AddWithValue("@refaccionId", item.Part.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al registrar la venta (revirtiendo transacción): {ex.Message}");
                transaction?.Rollback();
                return false;
            }
        }

        public static List<VentaReporte> GetVentasReporte()
        {
            var ventas = new List<VentaReporte>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT VentaID, Fecha, TotalVenta FROM Ventas ORDER BY Fecha DESC";
                    using (var cmd = new MySqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ventas.Add(new VentaReporte
                            {
                                VentaID = reader.GetInt32("VentaID"),
                                Fecha = reader.GetDateTime("Fecha"),
                                TotalVenta = reader.GetDouble("TotalVenta")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al obtener reporte de ventas: {ex.Message}");
            }
            return ventas;
        }

        public static List<DetalleVentaReporte> GetDetalleVentaReporte(int ventaId)
        {
            var detalles = new List<DetalleVentaReporte>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT s.Nombre, dv.CantidadVendida, dv.PrecioEnLaVenta 
                        FROM DetalleVenta dv
                        JOIN spareparts s ON dv.RefaccionID = s.Id
                        WHERE dv.VentaID = @ventaId";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ventaId", ventaId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                detalles.Add(new DetalleVentaReporte
                                {
                                    NombreRefaccion = reader.GetString("Nombre"),
                                    CantidadVendida = reader.GetInt32("CantidadVendida"),
                                    PrecioEnLaVenta = reader.GetDouble("PrecioEnLaVenta")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al obtener detalle de venta: {ex.Message}");
            }
            return detalles;
        }

        // ⬇⬇ --- MÉTODOS NUEVOS PARA GESTIÓN DE USUARIOS --- ⬇⬇

        /// <summary>
        /// Obtiene todos los usuarios (sin contraseña) para el DataGrid.
        /// </summary>
        public static List<Usuario> GetAllUsers()
        {
            var usuarios = new List<Usuario>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT UsuarioID, Username, Rol FROM Usuarios";
                    using (var cmd = new MySqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usuarios.Add(new Usuario
                            {
                                UsuarioID = reader.GetInt32("UsuarioID"),
                                Username = reader.GetString("Username"),
                                Rol = reader.GetString("Rol")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener usuarios: {ex.Message}");
            }
            return usuarios;
        }

        /// <summary>
        /// Añade un nuevo usuario a la base de datos.
        /// </summary>
        /// <returns>Devuelve true si fue exitoso, false si falló (ej: usuario ya existe).</returns>
        public static bool AddUser(string username, string password, string rol)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Usuarios (Username, Password, Rol) VALUES (@username, @password, @rol)";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@rol", rol);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Código 1062 es para 'Entrada duplicada' (usuario ya existe)
                Debug.WriteLine($"Error MySQL al añadir usuario (puede ser duplicado): {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al añadir usuario: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Elimina un usuario de la base de datos por su ID.
        /// </summary>
        public static void DeleteUser(int usuarioId)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Usuarios WHERE UsuarioID = @id";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", usuarioId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al eliminar usuario: {ex.Message}");
            }
        }
    }
}