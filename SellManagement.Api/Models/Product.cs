namespace SellManagement.Api.Models
{
    // Esta clase representa un Producto que vendemos.
    // Es como una ficha con los datos de cada artículo.
    public class Product
    {
        // El Id es el número único que identifica al producto en la base de datos
        public int Id { get; set; }

        // El nombre del producto (ej: "Coca Cola")
        public string Name { get; set; }

        // Una descripción breve (ej: "Bebida gaseosa 500ml")
        public string Description { get; set; }

        // El precio al que vendemos el producto
        public decimal Price { get; set; }

        // La cantidad que tenemos disponible en el inventario
        public int Stock { get; set; }

        // Indica si el producto está activo (True) o eliminado (False) - Soft Delete
        public bool IsActive { get; set; } = true;
    }
}
