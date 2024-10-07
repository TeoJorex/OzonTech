using Domain.Entities;
using Domain.Repository;
using Domain.Repository.Interfaces;
using Domain.Services.Interfaces;
using FluentValidation;
using GrpcService;
using Homework2.Validators;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using AddProductRequest = Homework2.Controllers.DTO.Requests.AddProductRequest;
using AddProductResponse = Homework2.Controllers.DTO.Responses.AddProductResponse;


namespace Integration_Tests.ControllersTests
{
    public class AddProductControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public AddProductControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductService>();
                    services.RemoveAll<IValidator<AddProductRequest>>();
                    services.RemoveAll<IProductRepository>();

                    services.AddSingleton<ProductRepository>();
                    services.AddSingleton<IProductRepository>(sp => sp.GetRequiredService<ProductRepository>());
                    services.AddSingleton<IProductService, Domain.Services.ProductService>();

                    services.AddSingleton<IValidator<AddProductRequest>, AddProductRequestValidator>();
                });
            });

            _client = _factory.CreateClient();
        }


        [Fact]
        public async Task AddProduct_ValidRequest_ReturnsOkAndProductId()
        {
            // Arrange
            var request = new AddProductRequest
            {
                Name = "Test Product",
                Price = 150.0,
                Weight = 2.5,
                ProductType = (ProductType)PRODUCT_TYPE_GENERAL.General,
                CreatedDate = DateTime.UtcNow,
                WarehouseId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/AddNewProduct", request);

            // Assert
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.True(false, $"Expected OK but got {response.StatusCode}. Response content: {errorContent}");
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var addResponse = await response.Content.ReadFromJsonAsync<AddProductResponse>();
            Assert.NotNull(addResponse);
            Assert.True(addResponse.Id > 0, "Полученный Id должен быть больше 0");

            using (var scope = _factory.Services.CreateScope())
            {
                var productRepository = scope.ServiceProvider.GetRequiredService<ProductRepository>();
                var product = productRepository.GetById(addResponse.Id);
                Assert.NotNull(product);
                Assert.Equal(request.Name, product.Name);
                Assert.Equal(request.Price, product.Price);
                Assert.Equal(request.Weight, product.Weight);
                Assert.Equal((ProductType)request.ProductType, product.ProductType);
                Assert.Equal(request.WarehouseId, product.WarehouseId);
            }
        }

        [Fact]
        public async Task AddProduct_EmptyName_ReturnsBadRequest()
        {
            // Arrange
            var request = new AddProductRequest
            {
                Name = "", 
                Price = 150.0,
                Weight = 2.5,
                ProductType = (ProductType)PRODUCT_TYPE_GENERAL.General,
                CreatedDate = DateTime.UtcNow,
                WarehouseId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/AddNewProduct", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Имя не должно быть пустым", errorContent);
        }

        [Fact]
        public async Task AddProduct_NegativePrice_ReturnsBadRequest()
        {
            // Arrange
            var request = new AddProductRequest
            {
                Name = "Test Product",
                Price = -100.0, 
                Weight = 2.5,
                ProductType = (ProductType)PRODUCT_TYPE_GENERAL.General,
                CreatedDate = DateTime.UtcNow,
                WarehouseId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/AddNewProduct", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Цена должна быть больше 0", errorContent);
        }
       
        [Fact]
        public async Task AddProduct_InvalidWarehouseId_ReturnsBadRequest()
        {
            // Arrange
            var request = new AddProductRequest
            {
                Name = "Test Product",
                Price = 150.0,
                Weight = 2.5,
                ProductType = (ProductType)PRODUCT_TYPE_GENERAL.General,
                CreatedDate = DateTime.UtcNow,
                WarehouseId = 0
            };

            // Act
            var response = await _client.PostAsJsonAsync("/AddNewProduct", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Id склада должен быть больше 0", errorContent);
        }

    }
}
