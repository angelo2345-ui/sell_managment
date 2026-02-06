namespace SellManagement.Api.Models
{
    // Esta clase representa una Venta realizada.
    // Guarda la información general de la compra.
    public class Sale
    {
        // Identificador único de la venta
        public int Id { get; set; }

        // La fecha y hora en que se hizo la venta
        public DateTime Date { get; set; }

        // El Id del cliente que hizo la compra (relación con la tabla Clients)
        public int ClientId { get; set; }

        // El monto total de dinero de la venta
        public decimal Total { get; set; }
    }
}
