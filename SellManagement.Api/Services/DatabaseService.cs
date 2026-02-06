using Microsoft.Data.SqlClient;

namespace SellManagement.Api.Services
{
    // Esta clase sirve para conectarnos a la base de datos de forma fácil.
    // Imagina que es como un "llavero" que nos da la llave (conexión) para entrar a la base de datos.
    public class DatabaseService
    {
        private readonly IConfiguration _configuration;

        // El constructor recibe la configuración de la aplicación (donde está guardada la cadena de conexión)
        public DatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Este método crea y devuelve una nueva conexión a SQL Server
        public SqlConnection GetConnection()
        {
            // Leemos la cadena de conexión del archivo appsettings.json
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            // Creamos la conexión usando esa cadena
            return new SqlConnection(connectionString);
        }
    }
}
