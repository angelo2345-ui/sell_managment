namespace SellManagement.Api.Models
{
    // Esta clase representa el detalle de una venta.
    // Es decir, qué producto se vendió y cuántos, dentro de una venta específica.
    public class SaleDetail
    {
        // Identificador único del detalle
        public int Id { get; set; }

        // A qué venta pertenece este detalle
        public int SaleId { get; set; }

        // Qué producto se vendió
        public int ProductId { get; set; }

        // Cuántas unidades de ese producto se vendieron
        public int Quantity { get; set; }

        // A qué precio se vendió cada unidad (por si el precio cambia en el futuro)
        public decimal UnitPrice { get; set; }
    }
}
