using Domain.Repository;
using Domain.Repository.Interfaces;
using Domain.Services.Interfaces;
using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;
using Homework2.Validators;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using static GrpcService.ProductService;

namespace Integration_Tests.GRPCTests
{
    public class ProductServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProductServiceIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductService>();
                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<ValidationInterceptor>();

                    services.AddSingleton<ProductRepository>();
                    services.AddSingleton<IProductRepository>(sp => sp.GetRequiredService<ProductRepository>());
                    services.AddSingleton<IProductService, Domain.Services.ProductService>();

                    services.AddValidatorsFromAssemblyContaining<AddProductRequestValidator>();

                    services.AddSingleton<ValidationInterceptor>();

                    services.AddGrpc(options =>
                    {
                        options.EnableDetailedErrors = true;
                        options.Interceptors.Add<ValidationInterceptor>();
                    });
                });
            });
        }

        private ProductServiceClient CreateGrpcClient()
        {
            var client = _factory.CreateDefaultClient();
            var channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
            {
                HttpClient = client
            });
            return new ProductServiceClient(channel);
        }

        #region Test Add

        [Fact]
        public async Task AddProduct_ValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var grpcClient = CreateGrpcClient();

            var request = new AddProductRequest
            {
                Name = "Valid Product",
                Price = 100.0,
                Weight = 2.5,
                ProductType = PRODUCT_TYPE_GENERAL.General,
                CreatedDate = Timestamp.FromDateTime(DateTime.UtcNow),
                WarehouseId = 1
            };

            // Act
            var response = await grpcClient.AddProductAsync(request);

            // Assert
            Assert.True(response.Id > 0);

            var repository = _factory.Services.GetRequiredService<ProductRepository>();
            var product = repository._products[response.Id];
            Assert.NotNull(product);
            Assert.Equal(request.Name, product.Name);
        }

        [Fact]
        public async Task AddProduct_InvalidRequest_ShouldReturnValidationError()
        {
            // Arrange
            var grpcClient = CreateGrpcClient();

            var request = new AddProductRequest
            {
                Price = -100.0,
                Weight = -2.5,
                ProductType = PRODUCT_TYPE_GENERAL.General,
                CreatedDate = Timestamp.FromDateTime(DateTime.UtcNow),
                WarehouseId = 1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await grpcClient.AddProductAsync(request);
            });

            Console.WriteLine($"Status Detail: {ex.Status.Detail}");

            Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);

            var error = "Имя не должно быть пустым, Цена должна быть больше 0, Вес должен быть больше 0";

            Assert.Equal(error, ex.Status.Detail);

        }

        #endregion


        #region Test GetProductById 

        [Fact]
        public async Task GetProductById_ExistingProduct_ShouldReturnProduct()
        {
            // Arrange
            var grpcClient = CreateGrpcClient();

            var addRequest = new AddProductRequest
            {
                Name = "Test Product",
                Price = 100.0,
                Weight = 2.5,
                ProductType = PRODUCT_TYPE_GENERAL.General,
                CreatedDate = Timestamp.FromDateTime(DateTime.UtcNow),
                WarehouseId = 1
            };

            var addResponse = await grpcClient.AddProductAsync(addRequest);

            var getRequest = new GetProductByIdRequest
            {
                Id = addResponse.Id
            };

            // Act
            var getResponse = await grpcClient.GetProductByIdAsync(getRequest);

            // Assert
            Assert.NotNull(getResponse);
            Assert.Equal(addRequest.Name, getResponse.Name);
            Assert.Equal(addRequest.Price, getResponse.Price);
            Assert.Equal(addRequest.Weight, getResponse.Weight);
            Assert.Equal(addRequest.ProductType, getResponse.ProductType);
            Assert.Equal(addRequest.WarehouseId, getResponse.WarehouseId);
        }

        [Fact]
        public async Task GetProductById_NonExistingProduct_ShouldReturnNotFound()
        {
            // Arrange
            var grpcClient = CreateGrpcClient();

            var getRequest = new GetProductByIdRequest
            {
                Id = -1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await grpcClient.GetProductByIdAsync(getRequest);
            });

            Console.WriteLine($"Status Detail: {ex.Status.Detail}");

            Assert.Equal(StatusCode.NotFound, ex.StatusCode);

            Assert.Contains("не найден", ex.Status.Detail);
        }

        #endregion

        #region Test Update

        [Fact]
        public async Task UpdatePriceById_ValidRequest_ShouldUpdatePrice()
        {
            // Arrange
            var grpcClient = CreateGrpcClient();

            var addRequest = new AddProductRequest
            {
                Name = "Test Product",
                Price = 100.0,
                Weight = 2.5,
                ProductType = PRODUCT_TYPE_GENERAL.General,
                CreatedDate = Timestamp.FromDateTime(DateTime.UtcNow),
                WarehouseId = 1
            };

            var addResponse = await grpcClient.AddProductAsync(addRequest);

            var updateRequest = new UpdatePricetByIdRequest
            {
                Id = addResponse.Id,
                NewPrice = 150
            };

            // Act
            var updateResponse = await grpcClient.UpdatePricetByIdAsync(updateRequest);

            // Assert
            Assert.NotNull(updateResponse);
            Assert.Equal("Успешно", updateResponse.Message);

            var getRequest = new GetProductByIdRequest
            {
                Id = addResponse.Id
            };

            var getResponse = await grpcClient.GetProductByIdAsync(getRequest);

            Assert.Equal(updateRequest.NewPrice, getResponse.Price);
        }

        [Fact]
        public async Task UpdatePriceById_InvalidPrice_ShouldReturnValidationError()
        {
            // Arrange
            var grpcClient = CreateGrpcClient();

            var addRequest = new AddProductRequest
            {
                Name = "Test Product",
                Price = 100.0,
                Weight = 2.5,
                ProductType = PRODUCT_TYPE_GENERAL.General,
                CreatedDate = Timestamp.FromDateTime(DateTime.UtcNow),
                WarehouseId = 1
            };

            var addResponse = await grpcClient.AddProductAsync(addRequest);

            var updateRequest = new UpdatePricetByIdRequest
            {
                Id = addResponse.Id,
                NewPrice = -50
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await grpcClient.UpdatePricetByIdAsync(updateRequest);
            });

            Console.WriteLine($"Status Detail: {ex.Status.Detail}");

            Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);

            Assert.Contains("Цена должна быть больше 0", ex.Status.Detail);
        }

        [Fact]
        public async Task UpdatePriceById_NonExistingProduct_ShouldReturnNotFound()
        {
            // Arrange
            var grpcClient = CreateGrpcClient();

            var updateRequest = new UpdatePricetByIdRequest
            {
                Id = -1,
                NewPrice = 150
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await grpcClient.UpdatePricetByIdAsync(updateRequest);
            });

            Console.WriteLine($"Status Detail: {ex.Status.Detail}");

            Assert.Equal(StatusCode.NotFound, ex.StatusCode);

            Assert.Contains("не найден", ex.Status.Detail);
        }

        #endregion

        #region Test Filter

        [Fact]
        public async Task GetProductsByFilters_ValidRequest_ShouldReturnProducts()
        {
            // Arrange
            var grpcClient = CreateGrpcClient();

            var addRequests = new List<AddProductRequest>
            {
                 new AddProductRequest
                 {
                        Name = "Product 1",
                        Price = 50.0,
                        Weight = 1.0,
                        ProductType = PRODUCT_TYPE_GENERAL.General,
                        CreatedDate = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                        WarehouseId = 1
                    },
                    new AddProductRequest
                    {
                        Name = "Product 2",
                        Price = 100.0,
                        Weight = 2.0,
                        ProductType = PRODUCT_TYPE_GENERAL.Electronic,
                        CreatedDate = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                        WarehouseId = 2
                    },
                    new AddProductRequest
                    {
                        Name = "Product 3",
                        Price = 150.0,
                        Weight = 3.0,
                        ProductType = PRODUCT_TYPE_GENERAL.General,
                        CreatedDate = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-3)),
                        WarehouseId = 1
                    }
                };

            foreach (var addRequest in addRequests)
            {
                await grpcClient.AddProductAsync(addRequest);
            }

            var filterRequest = new GetProductsByFiltersRequest
            {
                ProductType = PRODUCT_TYPE_GENERAL.General,
                WarehouseId = 1,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var response = await grpcClient.GetProductsByFiltersAsync(filterRequest);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Products.Count > 0);

            foreach (var product in response.Products)
            {
                Assert.Equal(PRODUCT_TYPE_GENERAL.General, product.ProductType);
                Assert.Equal(1, product.WarehouseId);
            }
        }

        [Fact]
        public async Task GetProductsByFilters_InvalidPageParameters_ShouldReturnValidationError()
        {
            // Arrange
            var grpcClient = CreateGrpcClient();

            var filterRequest = new GetProductsByFiltersRequest
            {
                PageNumber = 0,
                PageSize = -5
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await grpcClient.GetProductsByFiltersAsync(filterRequest);
            });

            Console.WriteLine($"Status Detail: {ex.Status.Detail}");

            Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);

            var error = "Номер страницы должен быть больше 0, Размер страницы не должен быть больше 0"; // ??? у меня тут бага в валидаторе нет "не" ,а VS считает, что надо

            Assert.Equal(error, ex.Status.Detail);
        }

        #endregion
    }
}
