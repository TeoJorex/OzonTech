using AutoFixture.AutoMoq;
using AutoFixture;
using Domain.Services.Interfaces;
using Moq;
using Homework2.Controllers;
using Domain.DTO.Requests;
using Homework2.Controllers.DTO.Requests;
using Homework2.Controllers.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Domain.Exeptions;

namespace UnitTests.ControllersTests
{
    public class UpdateProductPriceControllerTest
    {
        private readonly IFixture _fixture;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly UpdateProductPriceController _controller;

        public UpdateProductPriceControllerTest()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _productServiceMock = _fixture.Freeze<Mock<IProductService>>();

            _controller = new UpdateProductPriceController(_productServiceMock.Object,null);
        }

        [Fact]
        public void UpdateProductPrice_WhenProductExists_ShouldReturnOkWithMessage()
        {
            // Arrange
            var updateProductPriceRequest = _fixture.Create<UpdateProductPriceRequest>();
            var expectedMessage = "Price updated successfully";

            _productServiceMock
                .Setup(s => s.UpdatePrice(It.IsAny<UpdateProductPriceModel>()))
                .Returns(expectedMessage);

            // Act
            var result = _controller.UpdateProductPrice(updateProductPriceRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UpdateProductPriceResponse>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<UpdateProductPriceResponse>(okResult.Value);

            Assert.Equal(expectedMessage, response.Message);

            _productServiceMock.Verify(s => s.UpdatePrice(It.Is<UpdateProductPriceModel>(m =>
                m.Id == updateProductPriceRequest.Id &&
                m.Price == updateProductPriceRequest.Price
            )), Times.Once);
        }

        [Fact]
        public void UpdateProductPrice_WhenProductDoesNotExist_ShouldReturnBadRequest()
        {
            // Arrange
            var updateProductPriceRequest = _fixture.Create<UpdateProductPriceRequest>();
            var exceptionMessage = "Product not found";

            _productServiceMock
                .Setup(s => s.UpdatePrice(It.IsAny<UpdateProductPriceModel>()))
                .Throws(new NotFoundExeption(exceptionMessage));

            // Act
            var result = _controller.UpdateProductPrice(updateProductPriceRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UpdateProductPriceResponse>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);

            Assert.Equal(exceptionMessage, errorMessage);

            _productServiceMock.Verify(s => s.UpdatePrice(It.IsAny<UpdateProductPriceModel>()), Times.Once);
        }

        [Fact]
        public void UpdateProductPrice_WhenExceptionOccurs_ShouldReturnBadRequest()
        {
            // Arrange
            var updateProductPriceRequest = _fixture.Create<UpdateProductPriceRequest>();
            var exceptionMessage = "Unexpected error";

            _productServiceMock
                .Setup(s => s.UpdatePrice(It.IsAny<UpdateProductPriceModel>()))
                .Throws(new Exception(exceptionMessage));

            // Act
            var result = _controller.UpdateProductPrice(updateProductPriceRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UpdateProductPriceResponse>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);

            Assert.Equal($"Error {exceptionMessage}", errorMessage);

            _productServiceMock.Verify(s => s.UpdatePrice(It.IsAny<UpdateProductPriceModel>()), Times.Once);
        }
    }
}
