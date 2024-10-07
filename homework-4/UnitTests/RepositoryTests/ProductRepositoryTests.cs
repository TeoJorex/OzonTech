using Bogus;
using Domain.Entities;
using Domain.Exeptions;
using Domain.Repository;
using Xunit;

namespace UnitTests.RepositoryTests
{
    public class ProductRepositoryTests
    {
        private readonly ProductRepository _repository;
        private readonly Faker<ProductEntity> _productFaker;

        public ProductRepositoryTests()
        {
            _repository = new ProductRepository();
            _productFaker = new Faker<ProductEntity>()
                .RuleFor(p => p.Name, f => f.Vehicle.Model())
                .RuleFor(p => p.CreatedDate, f => f.Date.Past())
                .RuleFor(p => p.ProductType, (f, p) => f.PickRandom<ProductType>())
                .RuleFor(p => p.WarehouseId, f => f.Random.Int(1, 5))
                .RuleFor(p => p.Price, f => f.Random.Double(1, 1000))
                .RuleFor(p => p.Weight, f => f.Random.Double(1, 1000));
        }

        [Fact]
        public void Add_ShouldReturnNewId()
        {
            // Arrange
            var product = _productFaker.Generate();

            // Act
            var id = _repository.Add(product);

            // Assert
            Assert.Equal(1, id);
        }

        [Fact]
        public void GetById_ShouldReturnCorrectProduct_WhenExists()
        {
            // Arrange
            var product = _productFaker.Generate();
            var compareProduct = new ProductEntity()
            {
                Name = product.Name,
                Price = product.Price,
                Weight = product.Weight,
                CreatedDate = product.CreatedDate,
                ProductType = product.ProductType,
                WarehouseId = product.WarehouseId
            };

            var id = _repository.Add(compareProduct);

            // Act
            var retrievedProduct = _repository.GetById(id);

            // Assert
            Assert.NotNull(retrievedProduct);
            Assert.Equal(product.Name, retrievedProduct.Name);
            Assert.Equal(product.Price, retrievedProduct.Price);
            Assert.Equal(product.Weight, retrievedProduct.Weight);
            Assert.Equal(product.ProductType, retrievedProduct.ProductType);
            Assert.Equal(product.CreatedDate, retrievedProduct.CreatedDate);
            Assert.Equal(product.WarehouseId, retrievedProduct.WarehouseId);
        }

        [Fact]
        public void GetById_ShouldThrowNotFoundExeption_WhenDoesNotExist()
        {
            // Arrange
            var nonExistentId = 0;

            // Act & Assert
            Assert.Throws<NotFoundExeption>(() => _repository.GetById(nonExistentId));
        }

        [Fact]
        public void GetProductsByFilter_ShouldReturnAllProducts()
        {
            // Arrange
            var productCount = 20;
            var products = _productFaker.Generate(productCount);
            foreach (var product in products)
            {
                _repository.Add(product);
            }

            var filter = new FilterEntity
            {
                DateTime = null,
                WarehouseId = null,
                ProductType = null,
                PageNumber = 1,
                PageSize = productCount
            };

            // Act
            var filteredProducts = _repository.GetProductsByFilter(filter);

            // Assert
            Assert.NotEmpty(filteredProducts);
            Assert.Equal(productCount, filteredProducts.Count);
        }

        [Fact]
        public void GetProductsByFilter_ShouldReturnPagedResults()
        {
            // Arrange
            var productsCount = 5;
            var products = _productFaker.Generate(15);
            foreach (var product in products)
            {
                _repository.Add(product);
            }

            var filter = new FilterEntity
            {
                PageNumber = 2,
                PageSize = productsCount
            };

            // Act
            var pagedProducts = _repository.GetProductsByFilter(filter);

            // Assert
            Assert.Equal(productsCount, pagedProducts.Count);
        }

        [Theory]
        [InlineData(ProductType.Grocerie)]
        [InlineData(ProductType.Electronic)]
        [InlineData(ProductType.General)]
        [InlineData(ProductType.HouseholdChemical)]
        public void GetProductsByFilter_ShouldReturnFilteredProductsByProductType(ProductType productType)
        {
            // Arrange
            var products = _productFaker.Generate(20);
            foreach (var product in products)
            {
                _repository.Add(product);
            }

            var filter = new FilterEntity
            {
                ProductType = productType,
                PageNumber = 1,
                PageSize = 5
            };

            // Act
            var filteredProducts = _repository.GetProductsByFilter(filter);

            // Assert
            Assert.All(filteredProducts, p => Assert.Equal(filter.ProductType, p.ProductType));
        }

        [Fact]
        public void GetProductsByFilter_ShouldReturnFilteredProductsByWarehouseId()
        {
            // Arrange
            var products = _productFaker.Generate(20);
            foreach (var product in products)
            {
                _repository.Add(product);
            }

            var filter = new FilterEntity
            {
                WarehouseId = products[0].WarehouseId,
                PageNumber = 1,
                PageSize = 5
            };

            // Act
            var filteredProducts = _repository.GetProductsByFilter(filter);

            // Assert
            Assert.All(filteredProducts, p => Assert.Equal(filter.WarehouseId, p.WarehouseId));
        }

        [Fact]
        public void GetProductsByFilter_ShouldReturnFilteredProductsByCreatedDate()
        {
            // Arrange
            var products = _productFaker.Generate(20);
            foreach (var product in products)
            {
                _repository.Add(product);
            }

            var filter = new FilterEntity
            {
                DateTime = products[0].CreatedDate,
                PageNumber = 1,
                PageSize = 5
            };

            // Act
            var filteredProducts = _repository.GetProductsByFilter(filter);

            // Assert
            Assert.All(filteredProducts, p => Assert.Equal(filter.DateTime, p.CreatedDate));
        }

        [Fact]
        public void UpdatePrice_ShouldChangePrice_WhenProductExists()
        {
            // Arrange
            var product = _productFaker.Generate();
            var id = _repository.Add(product);
            var newPrice = product.Price + 50;

            // Act
            var result = _repository.UpdatePrice(id, newPrice);
            var updatedProduct = _repository.GetById(id);

            // Assert
            Assert.Equal("Успешно", result);
            Assert.Equal(newPrice, updatedProduct.Price);
        }

        [Fact]
        public void UpdatePrice_ShouldThrowNotFoundExeption_WhenProductDoesNotExist()
        {
            // Arrange
            var nonExistentId = 0;
            var newPrice = 500.0;

            // Act & Assert
            Assert.Throws<NotFoundExeption>(() => _repository.UpdatePrice(nonExistentId, newPrice));
        }

        [Fact]
        public void Add_ShouldBeThreadSafe()
        {
            // Arrange
            var productCount = 1000;
            var products = _productFaker.Generate(productCount);
            var exceptions = new List<Exception>();

            // Act
            Parallel.ForEach(products, product =>
            {
                try
                {
                    _repository.Add(product);
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });

            // Assert
            Assert.Empty(exceptions);
            Assert.Equal(productCount, _repository.GetProductsByFilter(new FilterEntity { PageNumber = 1, PageSize = productCount }).Count);
        }
    }
}
