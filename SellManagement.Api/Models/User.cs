namespace SellManagement.Api.Models
{
    // Esta clase representa a un Usuario del sistema (para iniciar sesión).
    public class User
    {
        // Identificador único del usuario
        public int Id { get; set; }

        // Nombre de usuario para entrar (login)
        public string Username { get; set; }

        // Contraseña (guardada de forma segura/encriptada)
        public string PasswordHash { get; set; }

        // Rol del usuario (ej: "Admin", "Vendedor")
        public string Role { get; set; }
    }
}
