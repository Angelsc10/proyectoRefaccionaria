// Busca este archivo (probablemente SparePart.cs)
namespace proyectoRefaccionaria
{
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
        public int Stock
        {
            get; set;
        } // Esta línea ya la tenías

        // ⬇⬇ AÑADE ESTA LÍNEA ⬇⬇
        public string Categoria
        {
            get; set;
        }
    }
}