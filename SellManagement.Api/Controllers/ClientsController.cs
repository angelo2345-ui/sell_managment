using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SellManagement.Api.Models;
using SellManagement.Api.Services;
using System.Collections.Generic;

namespace SellManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public ClientsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // 1. OBTENER TODOS LOS CLIENTES
        [HttpGet]
        public IActionResult GetClients()
        {
            var clients = new List<Client>();
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                // Solo traemos clientes activos
                string sql = "SELECT * FROM Clients WHERE IsActive = 1";
                using (var command = new SqlCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Phone = reader["Phone"].ToString()
                            // IsActive es true por defecto
                        });
                    }
                }
            }

            var response = new ApiResponse<List<Client>>
            {
                Success = true,
                Message = "Lista de clientes obtenida correctamente",
                Data = clients
            };
            return Ok(response);
        }

        // 2. CREAR UN CLIENTE
        [HttpPost]
        public IActionResult CreateClient([FromBody] Client client)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();

                    // Validación: Verificar si el email ya existe
                    string checkSql = "SELECT COUNT(*) FROM Clients WHERE Email = @Email";
                    using (var checkCmd = new SqlCommand(checkSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", client.Email);
                        int count = (int)checkCmd.ExecuteScalar();
                        
                        if (count > 0)
                        {
                            return BadRequest(new ApiResponse<string>
                            {
                                Success = false,
                                Message = "El correo electrónico ya está registrado.",
                                Data = null
                            });
                        }
                    }

                    string sql = "INSERT INTO Clients (Name, Email, Phone) VALUES (@Name, @Email, @Phone)";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", client.Name);
                        command.Parameters.AddWithValue("@Email", client.Email);
                        command.Parameters.AddWithValue("@Phone", client.Phone);
                        command.ExecuteNonQuery();
                    }
                }

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "¡Cliente creado exitosamente!",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Error al crear cliente: " + ex.Message,
                    Data = null
                });
            }
        }

        // 3. ACTUALIZAR CLIENTE
        [HttpPut("{id}")]
        public IActionResult UpdateClient(int id, [FromBody] Client client)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string sql = "UPDATE Clients SET Name = @Name, Email = @Email, Phone = @Phone WHERE Id = @Id";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@Name", client.Name);
                        command.Parameters.AddWithValue("@Email", client.Email);
                        command.Parameters.AddWithValue("@Phone", client.Phone);
                        
                        int rows = command.ExecuteNonQuery();
                        if (rows == 0) return NotFound(new ApiResponse<string> { Success = false, Message = "Cliente no encontrado", Data = null });
                    }
                }
                return Ok(new ApiResponse<string> { Success = true, Message = "Cliente actualizado correctamente", Data = null });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Error: " + ex.Message, Data = null });
            }
        }

        // 4. ELIMINAR CLIENTE
        [HttpDelete("{id}")]
        public IActionResult DeleteClient(int id)
        {
            Console.WriteLine($"[DELETE CLIENT REQUEST] Intentando eliminar cliente ID: {id} (Soft Delete)");
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    // Implementación Soft Delete: Marcar como inactivo
                    string sql = "UPDATE Clients SET IsActive = 0 WHERE Id = @Id";
                    
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        int rows = command.ExecuteNonQuery();
                        if (rows == 0) return NotFound(new ApiResponse<string> { Success = false, Message = "Cliente no encontrado", Data = null });
                    }
                }
                return Ok(new ApiResponse<string> { Success = true, Message = "Cliente eliminado correctamente", Data = null });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR DELETE CLIENT] {ex.Message}");
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

                return BadRequest(new ApiResponse<string> { Success = false, Message = "Error: " + ex.Message, Data = null });
            }
        }
    }
}