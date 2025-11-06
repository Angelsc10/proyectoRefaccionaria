using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace proyectoRefaccionaria
{
    public static class MySqlHelper
    {
        private static string connectionString = "server=localhost;database=refaccionaria;user=root;password=1234;";

        // ✅ ACTUALIZADO (Lee el Stock)
        public static List<SparePart> GetAllParts()
        {
            var parts = new List<SparePart>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // ⬇️ Consulta actualizada para incluir Stock
                    string query = "SELECT Id, Nombre, Precio, Stock FROM spareparts";

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
                                Stock = reader.GetInt32("Stock") // ⬅️ Línea añadida
                            });
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error de MySQL al obtener partes: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al obtener partes: {ex.Message}");
            }
            return parts;
        }

        // ✅ ACTUALIZADO (Escribe el Stock)
        public static void AddPart(SparePart part)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // ⬇️ Consulta actualizada para incluir Stock
                    string query = "INSERT INTO spareparts (Nombre, Precio, Stock) VALUES (@nombre, @precio, @stock)";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@nombre", part.Nombre);
                        cmd.Parameters.AddWithValue("@precio", part.Precio);
                        cmd.Parameters.AddWithValue("@stock", part.Stock); // ⬅️ Línea añadida
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error de MySQL al añadir parte: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al añadir parte: {ex.Message}");
            }
        }

        // ⬇️ CÓDIGO NUEVO (Actualiza todo: Nombre, Precio y Stock) ⬇️
        public static void UpdatePart(SparePart part)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE spareparts SET Nombre = @nombre, Precio = @precio, Stock = @stock WHERE Id = @id";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@nombre", part.Nombre);
                        cmd.Parameters.AddWithValue("@precio", part.Precio);
                        cmd.Parameters.AddWithValue("@stock", part.Stock);
                        cmd.Parameters.AddWithValue("@id", part.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error de MySQL al actualizar parte: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al actualizar parte: {ex.Message}");
            }
        }

        // ✅ SIN CAMBIOS (Sigue igual, ya era robusto)
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
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error de MySQL al eliminar parte: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general al eliminar parte: {ex.Message}");
            }
        }
    }
}