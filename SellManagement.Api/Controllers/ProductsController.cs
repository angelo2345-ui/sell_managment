using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SellManagement.Api.Models;
using SellManagement.Api.Services;

namespace SellManagement.Api.Controllers
{
    // Esta etiqueta le dice al sistema: "¡Oye, soy un controlador web!"
    [ApiController]
    // Esto define la dirección web: será "tu-sitio/api/products"
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        // Aquí pedimos el "llavero" (DatabaseService) para poder entrar a la base de datos
        public ProductsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // --- ORDEN 1: OBTENER TODOS LOS PRODUCTOS (GET) ---
        // Se activa cuando alguien visita: GET /api/products
        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = new List<Product>();

            // Usamos el llavero para abrir la conexión
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                
                // Preparamos la orden SQL: "Selecciona todo de la tabla Products"
                string sql = "SELECT * FROM Products";
                
                using (var command = new SqlCommand(sql, connection))
                {
                    // Ejecutamos la orden y leemos los resultados
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Por cada fila que encontramos, creamos una ficha de Producto
                            products.Add(new Product
                            {
                                Id = reader.GetInt32(0),                    // Columna 0: Id
                                Name = reader.GetString(1),                 // Columna 1: Nombre
                                Description = reader.IsDBNull(2) ? "" : reader.GetString(2), // Columna 2: Descripción (verificamos si está vacía)
                                Price = reader.GetDecimal(3),               // Columna 3: Precio
                                Stock = reader.GetInt32(4)                  // Columna 4: Stock
                            });
                        }
                    }
                }
            }

            // Devolvemos la lista de productos al usuario
            return Ok(products);
        }

        // --- ORDEN 2: CREAR UN PRODUCTO NUEVO (POST) ---
        // Se activa cuando alguien envía datos a: POST /api/products
        [HttpPost]
        public IActionResult CreateProduct([FromBody] Product product)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();

                // La orden SQL para insertar (guardar) un nuevo producto
                string sql = "INSERT INTO Products (Name, Description, Price, Stock) VALUES (@Name, @Description, @Price, @Stock)";
                
                using (var command = new SqlCommand(sql, connection))
                {
                    // Rellenamos los huecos (@Name, etc.) con los datos que nos enviaron para evitar hackeos
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", product.Description);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@Stock", product.Stock);

                    // Ejecutamos la orden de guardado
                    command.ExecuteNonQuery();
                }
            }

            return Ok("¡Producto creado con éxito!");
        }

        // --- ORDEN 3: ACTUALIZAR UN PRODUCTO (PUT) ---
        // Se activa con: PUT /api/products/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product product)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = "UPDATE Products SET Name = @Name, Description = @Description, Price = @Price, Stock = @Stock WHERE Id = @Id";
                
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", product.Description);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@Stock", product.Stock);

                    int rowsAffected = command.ExecuteNonQuery();
                    
                    if (rowsAffected == 0)
                        return NotFound("Producto no encontrado.");
                }
            }
            return Ok("¡Producto actualizado!");
        }

        // --- ORDEN 4: ELIMINAR UN PRODUCTO (DELETE) ---
        // Se activa con: DELETE /api/products/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = "DELETE FROM Products WHERE Id = @Id";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        return NotFound("Producto no encontrado.");
                }
            }
            return Ok("¡Producto eliminado!");
        }
    }
}