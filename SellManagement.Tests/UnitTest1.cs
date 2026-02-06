using Xunit;
using SellManagement.Api.Models; // Importamos tus modelos

namespace SellManagement.Tests
{
    public class ProductTests
    {
        // [Fact] indica que este método es una prueba automática
        [Fact]
        public void Product_ShouldHaveCorrectProperties()
        {
            // 1. ARRANGE (Preparar)
            // Creamos un producto de prueba
            var product = new Product
            {
                Id = 1,
                Name = "Laptop Gamer",
                Price = 1500.00m,
                Stock = 10
            };

            // 2. ACT (Actuar)
            // En este caso simple, la acción es la misma creación,
            // pero normalmente aquí llamaríamos a un método.

            // 3. ASSERT (Verificar)
            // Comprobamos que los valores sean los que esperamos
            Assert.Equal(1, product.Id);
            Assert.Equal("Laptop Gamer", product.Name);
            Assert.Equal(1500.00m, product.Price);
            Assert.Equal(10, product.Stock);
        }
    }
}