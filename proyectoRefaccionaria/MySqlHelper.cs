using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace proyectoRefaccionaria
{
    // --- CLASES DE MODELO ---

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

    public class Cliente
    {
        public int ClienteID
        {
            get; set;
        }
        public string Nombre
        {
            get; set;
        }
        public string Telefono
        {
            get; set;
        }
        public string Email
        {
            get; set;
        }
        public string RFC
        {
            get; set;
        }

        // Esto es para que el ComboBox muestre el nombre
        public override string ToString()
        {
            return Nombre;
        }
    }


    // --- CLASE PRINCIPAL DEL HELPER ---
    public static class MySqlHelper
    {
        // ⚠️ IMPORTANTE: Asegúrate de que esta contraseña sea la correcta en tu laptop.
        // Si usas XAMPP sin contraseña, cámbiala a: password=;
        private static string connectionString = "server=localhost;database=refaccionaria;user=root;password=1234;";

        // --- MÉTODOS DE REFACCIONES ---
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

        // --- MÉTODO DE LOGIN ---
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

        // --- MÉTODO DE VENTA ---
        public static int RegistrarVenta(List<CartItem> cart, int clienteId)
        {
            MySqlTransaction transaction = null;
            long ventaId = -1;

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    double totalVenta = cart.Sum(item => item.Subtotal);

                    // 1. Insertar Venta
                    string ventaQuery = "INSERT INTO Ventas (TotalVenta, ClienteID) VALUES (@total, @clienteId)";
                    using (var cmd = new MySqlCommand(ventaQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@total", totalVenta);
                        if (clienteId > 0)
                            cmd.Parameters.AddWithValue("@clienteId", clienteId);
                        else
                            cmd.Parameters.AddWithValue("@clienteId", DBNull.Value);

                        cmd.ExecuteNonQuery();
                        ventaId = cmd.LastInsertedId;
                    }

                    // 2. Insertar Detalles y Actualizar Stock
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
                    return (int)ventaId;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al registrar la venta: {ex.Message}");
                transaction?.Rollback();
                return -1;
            }
        }

        // --- MÉTODOS DE REPORTE ---
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
                Debug.WriteLine($"Error al obtener reporte: {ex.Message}");
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
                Debug.WriteLine($"Error al obtener detalle: {ex.Message}");
            }
            return detalles;
        }

        // --- MÉTODOS DE USUARIOS ---
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
                Debug.WriteLine($"Error MySQL al añadir usuario: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al añadir usuario: {ex.Message}");
                return false;
            }
        }

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

        // --- MÉTODOS DE CLIENTES ---
        public static List<Cliente> GetAllClientes()
        {
            var clientes = new List<Cliente>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT ClienteID, Nombre, Telefono, Email, RFC FROM Clientes ORDER BY Nombre";
                    using (var cmd = new MySqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clientes.Add(new Cliente
                            {
                                ClienteID = reader.GetInt32("ClienteID"),
                                Nombre = reader.GetString("Nombre"),
                                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? string.Empty : reader.GetString("Telefono"),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString("Email"),
                                RFC = reader.IsDBNull(reader.GetOrdinal("RFC")) ? string.Empty : reader.GetString("RFC")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener clientes: {ex.Message}");
            }
            return clientes;
        }

        public static bool AddCliente(string nombre, string telefono, string email, string rfc)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Clientes (Nombre, Telefono, Email, RFC) VALUES (@nombre, @telefono, @email, @rfc)";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@telefono", (object)telefono ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@rfc", (object)rfc ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al añadir cliente: {ex.Message}");
                return false;
            }
        }

        // ⬇⬇ MÉTODO NUEVO: UpdateCliente ⬇⬇
        public static void UpdateCliente(Cliente cliente)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Clientes SET Nombre = @nombre, Telefono = @telefono, Email = @email, RFC = @rfc WHERE ClienteID = @id";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@nombre", cliente.Nombre);
                        cmd.Parameters.AddWithValue("@telefono", (object)cliente.Telefono ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@email", (object)cliente.Email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@rfc", (object)cliente.RFC ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@id", cliente.ClienteID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al actualizar cliente: {ex.Message}");
            }
        }

        public static bool DeleteCliente(int clienteId)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Clientes WHERE ClienteID = @id";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", clienteId);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    Debug.WriteLine("No se puede eliminar cliente, tiene ventas asociadas.");
                    return false;
                }
                Debug.WriteLine($"Error MySQL al eliminar cliente: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al eliminar cliente: {ex.Message}");
                return false;
            }
        }
    }
}