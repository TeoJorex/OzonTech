﻿using Domain.Entities;
using Domain.Repository;
using Domain.Repository.Interfaces;
using Domain.Services.Interfaces;
using Homework2.Controllers.DTO.Requests;
using Homework2.Controllers.DTO.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Integration_Tests.ControllersTests
{
    public class GetProductControllerTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public GetProductControllerTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductService>();
                    services.RemoveAll<IProductRepository>();

                    services.AddSingleton<ProductRepository>();
                    services.AddSingleton<IProductRepository>(sp => sp.GetRequiredService<ProductRepository>());
                    services.AddSingleton<IProductService, Domain.Services.ProductService>();
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetProduct_ExistingProduct_ReturnsProduct()
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

            var request = new GetProductRequest
            {
                Id = 1 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/GetProductById", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseData = await response.Content.ReadFromJsonAsync<GetProductResponse>();
            Assert.NotNull(responseData);
            Assert.Equal("Test Product", responseData.Name);
            Assert.Equal(100.0, responseData.Price);
            Assert.Equal(2.5, responseData.Weight);
            Assert.Equal(ProductType.General, responseData.ProductType);
            Assert.Equal(1, responseData.WarehouseId);
        }

        [Fact]
        public async Task GetProduct_NonExistingProduct_ReturnsBadRequest()
        {
            // Arrange
            var request = new GetProductRequest
            {
                Id = 999 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/GetProductById", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Товар с данным id не найден", errorContent);
        }
    }
}
