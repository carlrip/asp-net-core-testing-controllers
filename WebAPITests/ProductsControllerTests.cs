using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Transactions;
using WebAPI;
using WebAPI.Controllers;
using Xunit;

namespace WebAPITests
{
    public class ProductsControllerTests
    {
        [Fact]
        public async void GetAll_ReturnsTwoProducts()
        {
            var configuration = GetConfig();

            var sut = new ProductsController(configuration);

            var result = await sut.GetAll();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async void GetById_WhenKnownId_ReturnsProduct()
        {
            var configuration = GetConfig();

            var sut = new ProductsController(configuration);

            var result = await sut.GetById(Guid.Parse("E897FF55-8F3D-4154-B582-8D37D116347F"));

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var product = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(Guid.Parse("E897FF55-8F3D-4154-B582-8D37D116347F"), product.ProductId);
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async void GetById_WhenUnknownId_Returns404()
        {
            var configuration = GetConfig();

            var sut = new ProductsController(configuration);

            var result = await sut.GetById(Guid.Parse("B051D7B5-E437-45BC-9CD1-4ED1971C2AE0"));

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void Post_ReturnsProduct()
        {
            var configuration = GetConfig();

            var sut = new ProductsController(configuration);

            ActionResult<Product> result;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                result = await sut.Post(new Product() { ProductName = "Test" });

                scope.Dispose();
            }

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var product = Assert.IsType<Product>(okResult.Value);
            Assert.False(product.ProductId == Guid.Empty);
            Assert.Equal("Test", product.ProductName);
        }

        private IConfiguration GetConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true) // use appsettings.json in current folder
                .AddEnvironmentVariables();

            return builder.Build();
        }
    }
}
