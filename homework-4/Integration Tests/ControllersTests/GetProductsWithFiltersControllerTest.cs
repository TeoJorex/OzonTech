using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Homework2.Controllers.DTO.Requests;
using Homework2.Controllers.DTO.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Domain.Services.Interfaces;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Homework2.Validators;
using Domain.Repository.Interfaces;
using Domain.Repository;
using Domain.Entities;

namespace Integration_Tests.ControllersTests
{
    public class GetProductsWithFiltersControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public GetProductsWithFiltersControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductService>();
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<IValidator<GetProductsWithFiltersRequest>>();

                    services.AddSingleton<ProductRepository>();
                    services.AddSingleton<IProductRepository>(sp => sp.GetRequiredService<ProductRepository>());
                    services.AddSingleton<IProductService, ProductService>();

                    services.AddSingleton<IValidator<GetProductsWithFiltersRequest>, GetProductsWithFiltersRequestValidator>();
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetProductsByFilters_ValidRequest_ReturnsProducts()
        {
            // Arrange
            var request = new GetProductsWithFiltersRequest
            {
                ProductType = null,
                DateTime = null,
                WarehouseId = null,
                PageNumber = 1,
                PageSize = 10
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var productRepository = scope.ServiceProvider.GetRequiredService<ProductRepository>();
                productRepository.Add(new ProductEntity
                {
                    Name = "Test Product 1",
                    Price = 100,
                    Weight = 1,
                    ProductType = ProductType.General,
                    CreatedDate = DateTime.UtcNow,
                    WarehouseId = 1
                });
                productRepository.Add(new ProductEntity
                {
                    Name = "Test Product 2",
                    Price = 200,
                    Weight = 2,
                    ProductType = ProductType.General,
                    CreatedDate = DateTime.UtcNow,
                    WarehouseId = 2
                });
            }

            // Act
            var response = await _client.PostAsJsonAsync("/GetProductsWithFilters", request);

            // Assert
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.True(false, $"Expected OK but got {response.StatusCode}. Response content: {errorContent}");
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var products = await response.Content.ReadFromJsonAsync<List<GetProductsWithFiltersModel>>();
            Assert.NotNull(products);
            Assert.NotEmpty(products);
            Assert.Equal(2,products.Count);
        }

        [Fact]
        public async Task GetProductsByFilters_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new GetProductsWithFiltersRequest
            {
                ProductType = null,
                DateTime = null,
                WarehouseId = null,
                PageNumber = 0,
                PageSize = -5
            };

            // Act
            var response = await _client.PostAsJsonAsync("/GetProductsWithFilters", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorMessage = await response.Content.ReadAsStringAsync();
            Assert.Contains("Номер страницы должен быть больше 0", errorMessage);
            Assert.Contains("Размер страницы должен быть больше 0", errorMessage);
        }

        [Fact]
        public async Task GetProductsByFilters_NoProductsFound_ReturnsEmptyList()
        {
            // Arrange
            var request = new GetProductsWithFiltersRequest
            {
                ProductType = ProductType.General,
                DateTime = DateTime.UtcNow.AddDays(-10),
                WarehouseId = 999, 
                PageNumber = 1,
                PageSize = 10
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var productRepository = scope.ServiceProvider.GetRequiredService<ProductRepository>();
                productRepository.Add(new ProductEntity
                {
                    Name = "Test Product 1",
                    Price = 100,
                    Weight = 1,
                    ProductType = ProductType.General,
                    CreatedDate = DateTime.UtcNow,
                    WarehouseId = 1
                });
                productRepository.Add(new ProductEntity
                {
                    Name = "Test Product 2",
                    Price = 200,
                    Weight = 2,
                    ProductType = ProductType.General,
                    CreatedDate = DateTime.UtcNow,
                    WarehouseId = 2
                });
            }

            // Act
            var response = await _client.PostAsJsonAsync("/GetProductsWithFilters", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var products = await response.Content.ReadFromJsonAsync<List<GetProductsWithFiltersModel>>();
            Assert.NotNull(products);
            Assert.Empty(products);
        }
    }
}

