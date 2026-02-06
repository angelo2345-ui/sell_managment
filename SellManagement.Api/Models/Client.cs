namespace SellManagement.Api.Models
{
    // Esta clase representa a un Cliente que nos compra.
    public class Client
    {
        // Identificador único del cliente
        public int Id { get; set; }

        // Nombre completo del cliente
        public string Name { get; set; }

        // Correo electrónico para contacto
        public string Email { get; set; }

        // Número de teléfono
        public string Phone { get; set; }
    }
}
