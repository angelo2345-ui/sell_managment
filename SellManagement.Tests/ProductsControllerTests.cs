// using Xunit;
// using Microsoft.AspNetCore.Mvc;
// using SellManagement.Api.Controllers;
// using SellManagement.Api.Services;
// using SellManagement.Api.Models;
// using Microsoft.Extensions.Configuration;
// using System.Collections.Generic;

// namespace SellManagement.Tests
// {
//     public class ProductsControllerTests
//     {
//         // Esta prueba verifica que podemos obtener una lista vacía (o llena) de productos
//  
        
//         [Fact]
//         public void CreateProduct_ShouldReturnOk()
//         {
//             // 1. ARRANGE (Preparar el escenario)
//             // Usamos la misma configuración de base de datos
//             var myConfiguration = new ConfigurationBuilder()
//                 .AddInMemoryCollection(new Dictionary<string, string>
//                 {
//                     {"ConnectionStrings:DefaultConnection", "Server=.\\SQLEXPRESS;Database=SellManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"} 
//                     
//                 })
//                 .Build();

//             var dbService = new DatabaseService(myConfiguration);
//             var controller = new ProductsController(dbService);

//             // Creamos un producto de prueba (ficticio)
//             var newProduct = new Product
//             {
//                 Name = "Producto Test Automático",
//                 Description = "Creado por el robot de pruebas",
//                 Price = 99.99m,
//                 Stock = 50
//             };

//             // 2. ACT (Ejecutar el proceso)
//             var result = controller.CreateProduct(newProduct);

//             // 3. ASSERT (Verificar el resultado)
//             // El robot comprueba si el sistema respondió "Todo bien" (OkResult)
//             Assert.IsType<OkObjectResult>(result);
//         }
//     }
// }