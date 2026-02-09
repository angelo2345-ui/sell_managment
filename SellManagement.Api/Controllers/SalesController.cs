using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SellManagement.Api.Models;
using SellManagement.Api.Services;
using System;
using System.Collections.Generic;

namespace SellManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public SalesController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // CREAR UNA VENTA COMPLETA
        [HttpPost]
        public IActionResult CreateSale([FromBody] SaleRequest request)
        {
            try 
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    // Iniciamos una transacción para asegurar que todo se guarde o nada
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 0. Validar Stock Suficiente para TODOS los productos
                            foreach (var detail in request.Details)
                            {
                                string stockCheckSql = "SELECT Stock, Name FROM Products WHERE Id = @Id";
                                using (var checkCmd = new SqlCommand(stockCheckSql, connection, transaction))
                                {
                                    checkCmd.Parameters.AddWithValue("@Id", detail.ProductId);
                                    using (var reader = checkCmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            int currentStock = (int)reader["Stock"];
                                            string productName = reader["Name"].ToString();
                                            
                                            if (currentStock < detail.Quantity)
                                            {
                                                throw new Exception($"Stock insuficiente para el producto '{productName}'. Disponible: {currentStock}, Solicitado: {detail.Quantity}");
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception($"El producto con ID {detail.ProductId} no existe.");
                                        }
                                    }
                                }
                            }

                            // 1. Insertar la Venta (Cabecera) y obtener el ID generado
                            string sqlSale = "INSERT INTO Sales (Date, ClientId, Total) OUTPUT INSERTED.Id VALUES (@Date, @ClientId, @Total)";
                            int newSaleId;

                            using (var command = new SqlCommand(sqlSale, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Date", DateTime.Now);
                                command.Parameters.AddWithValue("@ClientId", request.ClientId);
                                command.Parameters.AddWithValue("@Total", request.Total);
                                
                                // Ejecutamos y obtenemos el ID de la venta recién creada
                                newSaleId = (int)command.ExecuteScalar();
                            }

                            // 2. Insertar los Detalles y Actualizar Stock
                            foreach (var detail in request.Details)
                            {
                                // A) Insertar Detalle
                                string sqlDetail = "INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice) VALUES (@SaleId, @ProductId, @Quantity, @UnitPrice)";
                                using (var command = new SqlCommand(sqlDetail, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@SaleId", newSaleId);
                                    command.Parameters.AddWithValue("@ProductId", detail.ProductId);
                                    command.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                    command.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);
                                    command.ExecuteNonQuery();
                                }

                                // B) Descontar del Stock
                                string sqlUpdateStock = "UPDATE Products SET Stock = Stock - @Quantity WHERE Id = @ProductId";
                                using (var updateCmd = new SqlCommand(sqlUpdateStock, connection, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                    updateCmd.Parameters.AddWithValue("@ProductId", detail.ProductId);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }

                            // Si todo salió bien, confirmamos los cambios en la BD
                            transaction.Commit();

                            return Ok(new ApiResponse<object> { Data = new { SaleId = newSaleId }, Message = "Venta registrada con éxito." });
                        }
                        catch (Exception ex)
                        {
                            // Si algo falló, deshacemos todo
                            transaction.Rollback();
                            throw ex; // Re-lanzamos para que lo capture el catch externo
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Error al registrar la venta: " + ex.Message });
            }
        }

        // OBTENER HISTORIAL DE VENTAS
        [HttpGet]
        public IActionResult GetSales()
        {
            try
            {
                // Diccionario para agrupar ventas y sus detalles
                var salesDict = new Dictionary<int, SaleResponseDto>();

                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT s.Id, s.Date, s.Total, c.Name as ClientName,
                               sd.Quantity, sd.UnitPrice, p.Name as ProductName
                        FROM Sales s
                        INNER JOIN Clients c ON s.ClientId = c.Id
                        INNER JOIN SaleDetails sd ON s.Id = sd.SaleId
                        INNER JOIN Products p ON sd.ProductId = p.Id
                        ORDER BY s.Date DESC, s.Id";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int saleId = (int)reader["Id"];

                            if (!salesDict.ContainsKey(saleId))
                            {
                                salesDict[saleId] = new SaleResponseDto
                                {
                                    Id = saleId,
                                    Date = (DateTime)reader["Date"],
                                    Total = (decimal)reader["Total"],
                                    ClientName = reader["ClientName"].ToString(),
                                    Items = new List<string>()
                                };
                            }

                            string productInfo = $"{reader["ProductName"]} (x{reader["Quantity"]})";
                            salesDict[saleId].Items.Add(productInfo);
                        }
                    }
                }

                return Ok(new ApiResponse<List<SaleResponseDto>> { Data = new List<SaleResponseDto>(salesDict.Values) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Error al obtener las ventas: " + ex.Message });
            }
        }
    }

    // DTO para enviar la respuesta al frontend
    public class SaleResponseDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public string ClientName { get; set; }
        public List<string> Items { get; set; }
    }

    // Clase auxiliar para recibir los datos desde el Frontend (JSON complejo)
    public class SaleRequest
    {
        public int ClientId { get; set; }
        public decimal Total { get; set; }
        public List<SaleDetailDto> Details { get; set; }
    }

    public class SaleDetailDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
