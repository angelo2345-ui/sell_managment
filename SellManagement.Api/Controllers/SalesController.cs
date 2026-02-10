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

        // ACTUALIZAR VENTA (Simulada por seguridad: Solo actualiza el cliente o fecha, no los productos para no romper stock complejo)
        // Nota: Para editar productos, se recomienda eliminar y volver a crear la venta para mantener la integridad del stock.
        [HttpPut("{id}")]
        public IActionResult UpdateSale(int id, [FromBody] Sale sale)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string sql = "UPDATE Sales SET ClientId = @ClientId, Date = @Date WHERE Id = @Id";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@ClientId", sale.ClientId);
                        command.Parameters.AddWithValue("@Date", sale.Date);
                        
                        int rows = command.ExecuteNonQuery();
                        if (rows == 0) return NotFound(new ApiResponse<string> { Success = false, Message = "Venta no encontrada", Data = null });
                    }
                }
                return Ok(new ApiResponse<string> { Success = true, Message = "Venta actualizada (Cliente/Fecha).", Data = null });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Error al actualizar venta: " + ex.Message, Data = null });
            }
        }

        // ELIMINAR VENTA (Con devolución de Stock)
        [HttpDelete("{id}")]
        public IActionResult DeleteSale(int id)
        {
            try 
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Obtener los detalles de la venta para devolver el stock
                            string getDetailsSql = "SELECT ProductId, Quantity FROM SaleDetails WHERE SaleId = @SaleId";
                            var detailsToRestore = new List<(int ProductId, int Quantity)>();
                            
                            using (var cmd = new SqlCommand(getDetailsSql, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@SaleId", id);
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        detailsToRestore.Add((reader.GetInt32(0), reader.GetInt32(1)));
                                    }
                                }
                            }

                            // 2. Devolver el stock
                            foreach (var item in detailsToRestore)
                            {
                                string updateStockSql = "UPDATE Products SET Stock = Stock + @Quantity WHERE Id = @ProductId";
                                using (var updateCmd = new SqlCommand(updateStockSql, connection, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    updateCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }

                            // 3. Eliminar Detalles
                            string deleteDetailsSql = "DELETE FROM SaleDetails WHERE SaleId = @SaleId";
                            using (var cmd = new SqlCommand(deleteDetailsSql, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@SaleId", id);
                                cmd.ExecuteNonQuery();
                            }

                            // 4. Eliminar Cabecera
                            string deleteSaleSql = "DELETE FROM Sales WHERE Id = @Id";
                            using (var cmd = new SqlCommand(deleteSaleSql, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", id);
                                int rows = cmd.ExecuteNonQuery();
                                if (rows == 0) throw new Exception("Venta no encontrada");
                            }

                            transaction.Commit();
                            return Ok(new ApiResponse<string> { Success = true, Message = "Venta eliminada y stock restaurado.", Data = null });
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Error al eliminar venta: " + ex.Message, Data = null });
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
