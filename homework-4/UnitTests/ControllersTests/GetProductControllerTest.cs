using AutoFixture.AutoMoq;
using AutoFixture;
using Domain.Entities;
using Domain.Exeptions;
using Domain.Services.Interfaces;
using Homework2.Controllers.DTO.Requests;
using Homework2.Controllers.DTO.Responses;
using Homework2.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Domain.DTO.Requests;

namespace UnitTests.ControllersTests
{
    public class GetProductControllerTest
    {
        private readonly IFixture _fixture;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly GetProductController _controller;

        public GetProductControllerTest()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _productServiceMock = _fixture.Freeze<Mock<IProductService>>();

            _controller = new GetProductController(_productServiceMock.Object);
        }

        [Fact]
        public void GetProduct_ShouldReturnOkResult_WithProductResponse_WhenProductExists()
        {
            // Arrange
            var getProductRequest = _fixture.Create<GetProductRequest>();
            var productEntity = _fixture.Create<ProductEntity>();

            _productServiceMock
                .Setup(s => s.GetById(It.Is<GetProductModel>(m => m.Id == getProductRequest.Id)))
                .Returns(productEntity);

            // Act
            var result = _controller.GetProduct(getProductRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<GetProductResponse>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<GetProductResponse>(okResult.Value);

            Assert.Equal(productEntity.Name, response.Name);
            Assert.Equal(productEntity.Price, response.Price);
            Assert.Equal(productEntity.Weight, response.Weight);
            Assert.Equal(productEntity.ProductType, response.ProductType);
            Assert.Equal(productEntity.CreatedDate, response.CreatedDate);
            Assert.Equal(productEntity.WarehouseId, response.WarehouseId);

            _productServiceMock.Verify(s => s.GetById(It.Is<GetProductModel>(m => m.Id == getProductRequest.Id)), Times.Once);
        }

        [Fact]
        public void GetProduct_ShouldReturnBadRequest_WhenProductNotFound()
        {
            // Arrange
            var getProductRequest = _fixture.Create<GetProductRequest>();

            _productServiceMock
                .Setup(s => s.GetById(It.Is<GetProductModel>(m => m.Id == getProductRequest.Id)))
                .Throws(new NotFoundExeption("Product not found"));

            // Act
            var result = _controller.GetProduct(getProductRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<GetProductResponse>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);

            Assert.Contains("Error", errorMessage);
            Assert.Contains("Product not found", errorMessage);

            _productServiceMock.Verify(s => s.GetById(It.Is<GetProductModel>(m => m.Id == getProductRequest.Id)), Times.Once);
        }
    }
}
