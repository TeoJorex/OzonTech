using AutoFixture.AutoMoq;
using AutoFixture;
using Domain.Entities;
using Domain.Services.Interfaces;
using Homework2.Controllers.DTO.Requests;
using Homework2.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using GetProductsWithFiltersModel = Homework2.Controllers.DTO.Responses.GetProductsWithFiltersModel;
using Domain.Exeptions;
using Homework2.Controllers.DTO.Responses;

namespace UnitTests.ControllersTests
{
    public class GetProductsWithFiltersControllerTest
    {
        private readonly IFixture _fixture;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly GetProductsWithFiltersController _controller;

        public GetProductsWithFiltersControllerTest()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _productServiceMock = _fixture.Freeze<Mock<IProductService>>();

            _controller = new GetProductsWithFiltersController(_productServiceMock.Object, null);
        }

        [Fact]
        public void GetProductsByFilters_WhenExceptionOccurs_ShouldReturnBadRequest()
        {
            // Arrange
            var getProductsRequest = _fixture.Create<GetProductsWithFiltersRequest>();
            var exceptionMessage = "Test exception";

            _productServiceMock
                .Setup(s => s.GetProductsByFilter(It.IsAny<Domain.DTO.Requests.GetProductsWithFiltersModel>()))
                .Throws(new Exception(exceptionMessage));

            // Act
            var result = _controller.GetProductsByFilters(getProductsRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<GetProductsWithFiltersResponse>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);

            Assert.Contains($"Error {exceptionMessage}", errorMessage);

            _productServiceMock.Verify(s => s.GetProductsByFilter(It.IsAny<Domain.DTO.Requests.GetProductsWithFiltersModel>()), Times.Once);
        }

        [Fact]
        public void GetProductsByFilters_WhenCalledWithValidParameters_ShouldCallService()
        {
            // Arrange
            var getProductsRequest = _fixture.Create<GetProductsWithFiltersRequest>();

            _productServiceMock
                .Setup(s => s.GetProductsByFilter(It.IsAny<Domain.DTO.Requests.GetProductsWithFiltersModel>()))
                .Returns(new List<ProductEntity>());

            // Act 
            var result = _controller.GetProductsByFilters(getProductsRequest);

            // Assert
            _productServiceMock.Verify(s => s.GetProductsByFilter(It.Is<Domain.DTO.Requests.GetProductsWithFiltersModel>(m =>
                m.ProductType == getProductsRequest.ProductType &&
                m.DateTime == getProductsRequest.DateTime &&
                m.WarehouseId == getProductsRequest.WarehouseId &&
                m.PageNumber == getProductsRequest.PageNumber &&
                m.PageSize == getProductsRequest.PageSize
            )), Times.Once);
        }

        [Fact]
        public void GetProductsByFilters_WhenProductsExist_ShouldReturnProductList()
        {
            // Arrange
            var getProductsRequest = _fixture.Create<GetProductsWithFiltersRequest>();
            var productEntities = _fixture.Create<List<ProductEntity>>();

            _productServiceMock
                .Setup(s => s.GetProductsByFilter(It.IsAny<Domain.DTO.Requests.GetProductsWithFiltersModel>()))
                .Returns(productEntities);

            // Act 
            var result = _controller.GetProductsByFilters(getProductsRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<GetProductsWithFiltersResponse>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var responseList = Assert.IsType<List<GetProductsWithFiltersModel>>(okResult.Value);

            Assert.Equal(productEntities.Count, responseList.Count);

            for (int i = 0; i < productEntities.Count; i++)
            {
                Assert.Equal(productEntities[i].Name, responseList[i].Name);
                Assert.Equal(productEntities[i].Price, responseList[i].Price);
                Assert.Equal(productEntities[i].Weight, responseList[i].Weight);
                Assert.Equal(productEntities[i].ProductType, responseList[i].ProductType);
                Assert.Equal(productEntities[i].CreatedDate, responseList[i].CreatedDate);
                Assert.Equal(productEntities[i].WarehouseId, responseList[i].WarehouseId);
            }

            _productServiceMock.Verify(s => s.GetProductsByFilter(It.IsAny<Domain.DTO.Requests.GetProductsWithFiltersModel>()), Times.Once);
        }

        [Fact]
        public void GetProductsByFilters_WhenNoProductsFound_ShouldReturnEmptyList()
        {
            // Arrange
            var getProductsRequest = _fixture.Create<GetProductsWithFiltersRequest>();
            var productEntities = new List<ProductEntity>(); 

            _productServiceMock
                .Setup(s => s.GetProductsByFilter(It.IsAny<Domain.DTO.Requests.GetProductsWithFiltersModel>()))
                .Returns(productEntities);

            // Act 
            var result = _controller.GetProductsByFilters(getProductsRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<GetProductsWithFiltersResponse>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var responseList = Assert.IsType<List<GetProductsWithFiltersModel>>(okResult.Value);

            Assert.Empty(responseList);

            _productServiceMock.Verify(s => s.GetProductsByFilter(It.IsAny<Domain.DTO.Requests.GetProductsWithFiltersModel>()), Times.Once);
        }

        [Fact]
        public void GetProductsByFilters_WhenServiceReturnsNull_ShouldReturnBadRequest()
        {
            // Arrange
            var getProductsRequest = _fixture.Create<GetProductsWithFiltersRequest>();

            _productServiceMock
                .Setup(s => s.GetProductsByFilter(It.IsAny<Domain.DTO.Requests.GetProductsWithFiltersModel>()))
                .Returns((List<ProductEntity>)null);

            // Act 
            var result = _controller.GetProductsByFilters(getProductsRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<GetProductsWithFiltersResponse>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);

            Assert.Contains("Error", errorMessage);

            _productServiceMock.Verify(s => s.GetProductsByFilter(It.IsAny<Domain.DTO.Requests.GetProductsWithFiltersModel>()), Times.Once);
        }       
    }
}
