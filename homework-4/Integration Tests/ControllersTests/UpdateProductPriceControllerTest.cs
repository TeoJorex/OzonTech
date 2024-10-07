using Domain.Entities;
using Domain.Repository;
using Domain.Repository.Interfaces;
using Domain.Services.Interfaces;
using FluentValidation;
using GrpcService;
using Homework2.Controllers.DTO.Requests;
using Homework2.Controllers.DTO.Responses;
using Homework2.Validators;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Integration_Tests.ControllersTests
{
    public class UpdateProductPriceControllerTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public UpdateProductPriceControllerTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductService>();
                    services.RemoveAll<IValidator<UpdateProductPriceRequest>>();
                    services.RemoveAll<IProductRepository>();

                    services.AddSingleton<ProductRepository>();
                    services.AddSingleton<IProductRepository>(sp => sp.GetRequiredService<ProductRepository>());
                    services.AddSingleton<IProductService, Domain.Services.ProductService>();

                    services.AddSingleton<IValidator<UpdateProductPriceRequest>, UpdateProductPriceRequestValidator>();
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task UpdateProductPrice_ExistingProduct_ReturnsOk()
        {
            // Arrange
            var product = new ProductEntity
            {
                Name = "Test Product",
                Price = 100.0,
                Weight = 2.5,
                ProductType = ProductType.General,
                CreatedDate = DateTime.UtcNow,
                WarehouseId = 1
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                repository.Add(product);
            }

            var request = new UpdateProductPriceRequest
            {
                Id = 1, 
                Price = 150.0
            };

            // Act
            var response = await _client.PatchAsJsonAsync("/UpdatePriceById", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseData = await response.Content.ReadFromJsonAsync<UpdateProductPriceResponse>();
            Assert.NotNull(responseData);
            Assert.Equal("Успешно", responseData.Message);

            using (var scope = _factory.Services.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                var updatedProduct = repository.GetById(1);
                Assert.NotNull(updatedProduct);
                Assert.Equal(150.0, updatedProduct.Price);
            }
        }

        [Fact]
        public async Task UpdateProductPrice_NonExistingProduct_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateProductPriceRequest
            {
                Id = 999,
                Price = 150.0
            };

            // Act
            var response = await _client.PatchAsJsonAsync("/UpdatePriceById", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Товар с данным id не найден", errorContent);
        }

        [Fact]
        public async Task UpdateProductPrice_InvalidPrice_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateProductPriceRequest
            {
                Id = 1, 
                Price = -50.0 
            };

            // Act
            var response = await _client.PatchAsJsonAsync("/UpdatePriceById", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Цена должна быть больше 0", errorContent);
        }             
    }
}
