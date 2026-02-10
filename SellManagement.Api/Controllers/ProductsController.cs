using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SellManagement.Api.Models;
using SellManagement.Api.Services;
using System;
using System.Collections.Generic;

namespace SellManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public ProductsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // --- ORDEN 1: OBTENER TODOS LOS PRODUCTOS (GET) ---
        [HttpGet]
        public IActionResult GetProducts()
        {
            try 
            {
                var products = new List<Product>();

                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    // Solo traemos los productos activos (IsActive = 1)
                    string sql = "SELECT * FROM Products WHERE IsActive = 1";
                    
                    using (var command = new SqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                Stock = reader.GetInt32(4)
                                // IsActive es true por defecto y por el filtro WHERE
                            });
                        }
                    }
                }

                // AHORA SÍ: Devolvemos la "Cajita Feliz" (ApiResponse)
                return Ok(new ApiResponse<List<Product>>
                {
                    Success = true,
                    Message = "Lista de productos obtenida correctamente",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Error al obtener productos: " + ex.Message,
                    Data = null
                });
            }
        }

        // --- ORDEN 2: CREAR UN PRODUCTO NUEVO (POST) ---
        [HttpPost]
        public IActionResult CreateProduct([FromBody] Product product)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string sql = "INSERT INTO Products (Name, Description, Price, Stock) VALUES (@Name, @Description, @Price, @Stock)";
                    
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Description", product.Description);
                        command.Parameters.AddWithValue("@Price", product.Price);
                        command.Parameters.AddWithValue("@Stock", product.Stock);
                        command.ExecuteNonQuery();
                    }
                }

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "¡Producto creado con éxito!",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Error al crear producto: " + ex.Message,
                    Data = null
                });
            }
        }

        // --- ORDEN 3: ACTUALIZAR UN PRODUCTO (PUT) ---
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product product)
        {
            try
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
                        {
                            return NotFound(new ApiResponse<string>
                            {
                                Success = false,
                                Message = "Producto no encontrado.",
                                Data = null
                            });
                        }
                    }
                }
                
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "¡Producto actualizado correctamente!",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Error al actualizar: " + ex.Message,
                    Data = null
                });
            }
        }

        // --- ORDEN 4: ELIMINAR UN PRODUCTO (DELETE) ---
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            Console.WriteLine($"[DELETE REQUEST] Intentando eliminar producto ID: {id} (Soft Delete)");
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    // Implementación de Soft Delete: En lugar de borrar, marcamos como inactivo
                    // Esto preserva el historial de ventas pero oculta el producto de la lista
                    string sql = "UPDATE Products SET IsActive = 0 WHERE Id = @Id";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new ApiResponse<string>
                            {
                                Success = false,
                                Message = "Producto no encontrado.",
                                Data = null
                            });
                        }
                    }
                }
                
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "¡Producto eliminado correctamente!",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR DELETE] {ex.Message}");
                // Si el error es por columna inválida, es porque la migración falló
                if (ex.Message.Contains("Invalid column name"))
                {
                     return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Error interno: La base de datos no está actualizada (Falta IsActive). Reinicie el backend.",
                        Data = null
                    });
                }

                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Error al eliminar: " + ex.Message,
                    Data = null
                });
            }
        }
    }
}