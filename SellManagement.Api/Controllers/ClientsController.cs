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
                string sql = "SELECT * FROM Clients";
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
                        });
                    }
                }
            }
            return Ok(clients);
        }

        // 2. CREAR UN CLIENTE
        [HttpPost]
        public IActionResult CreateClient([FromBody] Client client)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = "INSERT INTO Clients (Name, Email, Phone) VALUES (@Name, @Email, @Phone)";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", client.Name);
                    command.Parameters.AddWithValue("@Email", client.Email);
                    command.Parameters.AddWithValue("@Phone", client.Phone);
                    command.ExecuteNonQuery();
                }
            }
            return Ok("¡Cliente creado exitosamente!");
        }

        // 3. ACTUALIZAR UN CLIENTE
        [HttpPut("{id}")]
        public IActionResult UpdateClient(int id, [FromBody] Client client)
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
                    if (rows == 0) return NotFound("Cliente no encontrado.");
                }
            }
            return Ok("¡Cliente actualizado!");
        }

        // 4. ELIMINAR UN CLIENTE
        [HttpDelete("{id}")]
        public IActionResult DeleteClient(int id)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = "DELETE FROM Clients WHERE Id = @Id";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rows = command.ExecuteNonQuery();
                    if (rows == 0) return NotFound("Cliente no encontrado.");
                }
            }
            return Ok("¡Cliente eliminado!");
        }
    }
}