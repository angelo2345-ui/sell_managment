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

        // CREAR UNA VENTA COMPLETA (MAESTRO-DETALLE)
        [HttpPost]
        public IActionResult CreateSale([FromBody] SaleRequest request)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                // Iniciamos una transacción para asegurar que todo se guarde o nada
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
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

                    // 2. Insertar los Detalles (Productos de la venta)
                    foreach (var detail in request.Details)
                    {
                        string sqlDetail = "INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice) VALUES (@SaleId, @ProductId, @Quantity, @UnitPrice)";
                        using (var command = new SqlCommand(sqlDetail, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@SaleId", newSaleId);
                            command.Parameters.AddWithValue("@ProductId", detail.ProductId);
                            command.Parameters.AddWithValue("@Quantity", detail.Quantity);
                            command.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);
                            command.ExecuteNonQuery();
                        }

                        // Opcional: Aquí podrías restar el Stock del producto en la tabla Products
                    }

                    // Si todo salió bien, confirmamos los cambios
                    transaction.Commit();
                    return Ok(new { Message = "¡Venta registrada con éxito!", SaleId = newSaleId });
                }
                catch (Exception ex)
                {
                    // Si algo falló, deshacemos todo
                    transaction.Rollback();
                    return BadRequest("Error al registrar la venta: " + ex.Message);
                }
            }
        }
        
        // OBTENER VENTAS (Solo cabeceras para listar)
        [HttpGet]
        public IActionResult GetSales()
        {
             var sales = new List<Sale>();
             using (var connection = _databaseService.GetConnection())
             {
                 connection.Open();
                 string sql = "SELECT * FROM Sales";
                 using (var command = new SqlCommand(sql, connection))
                 using (var reader = command.ExecuteReader())
                 {
                     while (reader.Read())
                     {
                         sales.Add(new Sale
                         {
                             Id = (int)reader["Id"],
                             Date = (DateTime)reader["Date"],
                             ClientId = (int)reader["ClientId"],
                             Total = (decimal)reader["Total"]
                         });
                     }
                 }
             }
             return Ok(sales);
        }
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