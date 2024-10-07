using AutoFixture;
using AutoFixture.AutoMoq;
using Domain.DTO.Requests;
using Domain.Services.Interfaces;
using Homework2.Controllers;
using Homework2.Controllers.DTO.Requests;
using Homework2.Controllers.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace UnitTests.ControllersTests
{
    public class AddProductsControllerTest
    {
        private readonly IFixture _fixture;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly AddProductController _controller;

        public AddProductsControllerTest()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _productServiceMock = _fixture.Freeze<Mock<IProductService>>();

            // Валидатор не используется в контроллере, поэтому его можно не передавать
            _controller = new AddProductController(_productServiceMock.Object, null);
        }

        [Fact]
        public void AddProduct_ShouldReturnOkResult_WithProductId_WhenProductIsAddedSuccessfully()
        {
            // Arrange
            var addProductRequest = _fixture.Create<AddProductRequest>();
            var expectedProductId = _fixture.Create<long>();

            _productServiceMock
                .Setup(s => s.Add(It.IsAny<AddProductModel>()))
                .Returns(expectedProductId);

            // Act
            var result = _controller.AddProduct(addProductRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AddProductResponse>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<AddProductResponse>(okResult.Value);
            Assert.Equal(expectedProductId, response.Id);

            _productServiceMock.Verify(s => s.Add(It.Is<AddProductModel>(m =>
                m.Name == addProductRequest.Name &&
                m.Price == addProductRequest.Price &&
                m.Weight == addProductRequest.Weight &&
                m.ProductType == addProductRequest.ProductType &&
                m.CreatedDate == addProductRequest.CreatedDate &&
                m.WarehouseId == addProductRequest.WarehouseId
            )), Times.Once);
        }

        [Fact]
        public void AddProduct_ShouldReturnBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var addProductRequest = _fixture.Create<AddProductRequest>();
            var exceptionMessage = "Test exception";

            _productServiceMock
                .Setup(s => s.Add(It.IsAny<AddProductModel>()))
                .Throws(new Exception(exceptionMessage));

            // Act
            var result = _controller.AddProduct(addProductRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AddProductResponse>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var errorResponse = Assert.IsType<string>(badRequestResult.Value);
            Assert.Contains($"Error {exceptionMessage}", errorResponse);

            _productServiceMock.Verify(s => s.Add(It.IsAny<AddProductModel>()), Times.Once);
        }
    }
}
