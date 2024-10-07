using Bogus;
using Domain.DTO.Requests;
using Domain.Entities;
using Domain.Exeptions;
using Domain.Repository.Interfaces;
using Domain.Services;
using Moq;
using Xunit;

namespace UnitTests.ServicesTests
{
    public class ProductServiceTest
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly ProductService _productService;
        private readonly Faker<AddProductModel> _addProductFaker;
        private readonly Faker<GetProductModel> _getProductFaker;
        private readonly Faker<GetProductsWithFiltersModel> _getProductsWithFiltersFaker;
        private readonly Faker<UpdateProductPriceModel> _updateProductPriceFaker;
        private readonly Faker<ProductEntity> _productEntityFaker;

        public ProductServiceTest()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _productService = new ProductService(_productRepositoryMock.Object);

            _addProductFaker = new Faker<AddProductModel>()
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Price, f => f.Random.Double(10, 1000))
                .RuleFor(p => p.Weight, f => f.Random.Double(0.1, 100))
                .RuleFor(p => p.ProductType, f => f.PickRandom<ProductType>())
                .RuleFor(p => p.CreatedDate, f => f.Date.Past())
                .RuleFor(p => p.WarehouseId, f => f.Random.Int(1, 10));

            _getProductFaker = new Faker<GetProductModel>()
                .RuleFor(p => p.Id, f => f.Random.Long(1, 1000));

            _getProductsWithFiltersFaker = new Faker<GetProductsWithFiltersModel>()
                .RuleFor(p => p.ProductType, f => f.PickRandom<ProductType>())
                .RuleFor(p => p.DateTime, f => f.Date.Past())
                .RuleFor(p => p.WarehouseId, f => f.Random.Int(1, 10))
                .RuleFor(p => p.PageNumber, f => f.Random.Int(1, 10))
                .RuleFor(p => p.PageSize, f => f.Random.Int(1, 100));

            _updateProductPriceFaker = new Faker<UpdateProductPriceModel>()
                .RuleFor(p => p.Id, f => f.Random.Long(1, 1000))
                .RuleFor(p => p.Price, f => f.Random.Double(10, 1000));

            _productEntityFaker = new Faker<ProductEntity>()
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Price, f => f.Random.Double(10, 1000))
                .RuleFor(p => p.Weight, f => f.Random.Double(0.1, 100))
                .RuleFor(p => p.ProductType, f => f.PickRandom<ProductType>())
                .RuleFor(p => p.CreatedDate, f => f.Date.Past())
                .RuleFor(p => p.WarehouseId, f => f.Random.Int(1, 10));
        }

        [Fact]
        public void Add_ShouldCallRepositoryAdd_AndReturnId()
        {
            // Arrange
            var addProductModel = _addProductFaker.Generate();
            var expectedId = 1;

            _productRepositoryMock
                .Setup(repo => repo.Add(It.IsAny<ProductEntity>()))
                .Returns(expectedId)
                .Verifiable();

            // Act
            var resultId = _productService.Add(addProductModel);

            // Assert
            _productRepositoryMock.Verify(repo => repo.Add(It.Is<ProductEntity>(p =>
                p.Name == addProductModel.Name &&
                p.Price == addProductModel.Price &&
                p.Weight == addProductModel.Weight &&
                p.ProductType == addProductModel.ProductType &&
                p.CreatedDate == addProductModel.CreatedDate &&
                p.WarehouseId == addProductModel.WarehouseId
            )), Times.Once);

            Assert.Equal(expectedId, resultId);
        }

        [Fact]
        public void GetById_ShouldReturnProduct_WhenExists()
        {
            // Arrange
            var getProductModel = _getProductFaker.Generate();
            var expectedProduct = _productEntityFaker.Generate();

            _productRepositoryMock
                .Setup(repo => repo.GetById(getProductModel.Id))
                .Returns(expectedProduct)
                .Verifiable();

            // Act
            var result = _productService.GetById(getProductModel);

            // Assert
            _productRepositoryMock.Verify(repo => repo.GetById(getProductModel.Id), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(expectedProduct.Name, result.Name);
            Assert.Equal(expectedProduct.Price, result.Price);
            Assert.Equal(expectedProduct.Weight, result.Weight);
            Assert.Equal(expectedProduct.ProductType, result.ProductType);
            Assert.Equal(expectedProduct.CreatedDate, result.CreatedDate);
            Assert.Equal(expectedProduct.WarehouseId, result.WarehouseId);
        }

        [Fact]
        public void GetById_ShouldThrowNotFoundExeption_WhenProductDoesNotExist()
        {
            // Arrange
            var getProductModel = _getProductFaker.Generate();
            _productRepositoryMock
                .Setup(repo => repo.GetById(getProductModel.Id))
                .Throws(new NotFoundExeption())
                .Verifiable();

            // Act & Assert
            var exception = Assert.Throws<NotFoundExeption>(() => _productService.GetById(getProductModel));
            Assert.Equal("Товар с данным id не найден", exception.Message);
            _productRepositoryMock.Verify(repo => repo.GetById(getProductModel.Id), Times.Once);
        }

        [Fact]
        public void GetProductsByFilter_ShouldReturnFilteredProducts()
        {
            // Arrange
            var filterModel = _getProductsWithFiltersFaker.Generate();
            var expectedProducts = _productEntityFaker.Generate(5);

            _productRepositoryMock
                .Setup(repo => repo.GetProductsByFilter(It.Is<FilterEntity>(f =>
                    f.ProductType == filterModel.ProductType &&
                    f.DateTime == filterModel.DateTime &&
                    f.WarehouseId == filterModel.WarehouseId &&
                    f.PageNumber == filterModel.PageNumber &&
                    f.PageSize == filterModel.PageSize
                )))
                .Returns(expectedProducts)
                .Verifiable();

            // Act
            var result = _productService.GetProductsByFilter(filterModel);

            _productRepositoryMock.Verify(repo => repo.GetProductsByFilter(It.Is<FilterEntity>(f =>
                f.ProductType == filterModel.ProductType &&
                f.DateTime == filterModel.DateTime &&
                f.WarehouseId == filterModel.WarehouseId &&
                f.PageNumber == filterModel.PageNumber &&
                f.PageSize == filterModel.PageSize
            )), Times.Once);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProducts.Count, result.Count);
            for (int i = 0; i < expectedProducts.Count; i++)
            {
                Assert.Equal(expectedProducts[i].Name, result[i].Name);
                Assert.Equal(expectedProducts[i].Price, result[i].Price);
                Assert.Equal(expectedProducts[i].Weight, result[i].Weight);
                Assert.Equal(expectedProducts[i].ProductType, result[i].ProductType);
                Assert.Equal(expectedProducts[i].CreatedDate, result[i].CreatedDate);
                Assert.Equal(expectedProducts[i].WarehouseId, result[i].WarehouseId);
            }
        }

        [Fact]
        public void UpdatePrice_ShouldCallRepositoryUpdatePrice_AndReturnSuccessMessage()
        {
            // Arrange
            var updatePriceModel = _updateProductPriceFaker.Generate();
            var expectedMessage = "Успешно";

            _productRepositoryMock
                .Setup(repo => repo.UpdatePrice(updatePriceModel.Id, updatePriceModel.Price))
                .Returns(expectedMessage)
                .Verifiable();

            // Act
            var result = _productService.UpdatePrice(updatePriceModel);

            // Assert
            _productRepositoryMock.Verify(repo => repo.UpdatePrice(updatePriceModel.Id, updatePriceModel.Price), Times.Once);
            Assert.Equal(expectedMessage, result);
        }

        [Fact]
        public void UpdatePrice_ShouldThrowNotFoundExeption_WhenProductDoesNotExist()
        {
            // Arrange
            var updatePriceModel = _updateProductPriceFaker.Generate();

            _productRepositoryMock
                .Setup(repo => repo.UpdatePrice(updatePriceModel.Id, updatePriceModel.Price))
                .Throws(new NotFoundExeption())
                .Verifiable();

            // Act & Assert
            var exception = Assert.Throws<NotFoundExeption>(() => _productService.UpdatePrice(updatePriceModel));
            Assert.Equal("Товар с данным id не найден", exception.Message);
            _productRepositoryMock.Verify(repo => repo.UpdatePrice(updatePriceModel.Id, updatePriceModel.Price), Times.Once);
        }

        [Fact]
        public void Add_ShouldHandleInvalidInput()
        {
            // Arrange
            var addProductModel = _addProductFaker.Generate();
            // Пусть репа выбрасывает ex при некорректных данных - по факту такое не может быть, ибо валидация решает, но пусть будет
            _productRepositoryMock
                .Setup(repo => repo.Add(It.IsAny<ProductEntity>()))
                .Throws(new ArgumentException("Invalid product data"))
                .Verifiable();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _productService.Add(addProductModel));
            Assert.Equal("Invalid product data", exception.Message);
            _productRepositoryMock.Verify(repo => repo.Add(It.IsAny<ProductEntity>()), Times.Once);
        }

        [Fact]
        public void GetProductsByFilter_ShouldReturnEmptyList_WhenNoProductsMatch()
        {
            // Arrange
            var filterModel = _getProductsWithFiltersFaker.Generate();
            var expectedProducts = new List<ProductEntity>();

            _productRepositoryMock
                .Setup(repo => repo.GetProductsByFilter(It.Is<FilterEntity>(f =>
                    f.ProductType == filterModel.ProductType &&
                    f.DateTime == filterModel.DateTime &&
                    f.WarehouseId == filterModel.WarehouseId &&
                    f.PageNumber == filterModel.PageNumber &&
                    f.PageSize == filterModel.PageSize
                )))
                .Returns(expectedProducts)
                .Verifiable();

            // Act
            var result = _productService.GetProductsByFilter(filterModel);

            // Assert
            _productRepositoryMock.Verify(repo => repo.GetProductsByFilter(It.Is<FilterEntity>(f =>
                f.ProductType == filterModel.ProductType &&
                f.DateTime == filterModel.DateTime &&
                f.WarehouseId == filterModel.WarehouseId &&
                f.PageNumber == filterModel.PageNumber &&
                f.PageSize == filterModel.PageSize
            )), Times.Once);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}


