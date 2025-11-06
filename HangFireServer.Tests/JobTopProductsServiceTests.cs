using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.DTOs;
using HangFireServer.Application.Services;
using HangFireServer.Core.Absttractions;
using HangFireServer.Domain.Models;
using Moq;
using Xunit;

namespace HangFireServer.Tests
{
    public class JobTopProductsServiceTests
    {
        private readonly Mock<IRequestValidator> _validatorMock;
        private readonly Mock<IRequestLogger> _requestLoggerMock;
        private readonly Mock<IErrorLogger> _errorLoggerMock;
        private readonly Mock<IEventLogger> _eventLoggerMock;
        private readonly Mock<IJobService> _jobServiceMock;
        private readonly Mock<IRequestEnricher> _requestEnricherMock;
        private readonly JobTopProductsService _service;

        public JobTopProductsServiceTests()
        {
            _validatorMock = new Mock<IRequestValidator>();
            _requestLoggerMock = new Mock<IRequestLogger>();
            _errorLoggerMock = new Mock<IErrorLogger>();
            _eventLoggerMock = new Mock<IEventLogger>();
            _jobServiceMock = new Mock<IJobService>();
            _requestEnricherMock = new Mock<IRequestEnricher>();

            _service = new JobTopProductsService(
                _validatorMock.Object,
                _requestLoggerMock.Object,
                _errorLoggerMock.Object,
                _eventLoggerMock.Object,
                _jobServiceMock.Object,
                _requestEnricherMock.Object
            );
        }

        [Fact]
        public async Task JobsProcessTopProducts_ShouldReturnQueued_WhenValidRequest()
        {
            // Arrange
            var request = CreateValidRequest();

            _validatorMock
                .Setup(v => v.Validate(request))
                .Returns((true, string.Empty, "flow-test"));

            // Act
            var result = await _service.JobsProcessTopProducts(request);

            // Assert
            Assert.NotNull(result);
            var resultString = result.ToString();
            Assert.Contains("queued", resultString);
            Assert.Contains(request.CorrelationId, resultString);

            // Verificar que se llamaron los métodos correctos
            _jobServiceMock.Verify(j => j.EnqueueJob(request), Times.Once);
            _requestLoggerMock.Verify(r => r.LogRequest(request, true, string.Empty, "flow-test"), Times.Once);

            // Verificar eventos de log
            _eventLoggerMock.Verify(e => e.LogEvent(
                request.CorrelationId,
                request.Service,
                request.Endpoint,
                "TopProductsProcessingStarted",
                It.IsAny<object>()), Times.Once);

            _eventLoggerMock.Verify(e => e.LogEvent(
                request.CorrelationId,
                request.Service,
                request.Endpoint,
                "TopProductsValidationJobsQueued",
                It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task JobsProcessTopProducts_ShouldReturnError_WhenInvalidRequest()
        {
            // Arrange
            var request = CreateValidRequest();
            var invalidReason = "Invalid data: Missing metadata";

            _validatorMock
                .Setup(v => v.Validate(request))
                .Returns((false, invalidReason, "flow-test"));

            // Act
            var result = await _service.JobsProcessTopProducts(request);

            // Assert
            Assert.NotNull(result);
            var resultString = result.ToString();
            Assert.Contains("error", resultString);
            Assert.Contains(invalidReason, resultString);
            Assert.Contains(request.CorrelationId, resultString);

            // Verificar que NO se encoló el job
            _jobServiceMock.Verify(j => j.EnqueueJob(It.IsAny<TopProductsRequest>()), Times.Never);

            // Verificar que se logueó la validación fallida
            _requestLoggerMock.Verify(r => r.LogRequest(request, false, invalidReason, "flow-test"), Times.Once);

            _eventLoggerMock.Verify(e => e.LogEvent(
                request.CorrelationId,
                request.Service,
                request.Endpoint,
                "TopProductsValidationFailed",
                It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task JobsProcessTopProducts_ShouldReturnError_WhenEmptyProductList()
        {
            // Arrange
            var request = CreateValidRequest();
            request.Payload.TopProducts = new List<ProductSale>();

            _validatorMock
                .Setup(v => v.Validate(request))
                .Returns((false, "No products to process", "flow-test"));

            // Act
            var result = await _service.JobsProcessTopProducts(request);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("error", result.ToString());
            _jobServiceMock.Verify(j => j.EnqueueJob(It.IsAny<TopProductsRequest>()), Times.Never);
        }

        [Fact]
        public async Task JobsProcessTopProducts_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var request = CreateValidRequest();
            var expectedException = new Exception("Database connection failed");

            _validatorMock
                .Setup(v => v.Validate(request))
                .Throws(expectedException);

            // Act
            var result = await _service.JobsProcessTopProducts(request);

            // Assert
            Assert.NotNull(result);
            var resultString = result.ToString();
            Assert.Contains("error", resultString);
            Assert.Contains("Error interno del servidor", resultString);

            // Verificar que se logueó el error
            _errorLoggerMock.Verify(e => e.LogError(
                request,
                It.Is<string>(s => s.Contains(expectedException.Message)),
                It.IsAny<string>()), Times.Once);

            // Verificar que se logueó el evento de error
            _eventLoggerMock.Verify(e => e.LogEvent(
                request.CorrelationId,
                request.Service,
                request.Endpoint,
                "TopProductsProcessingError",
                It.IsAny<object>()), Times.Once);

            // Verificar que NO se encoló el job
            _jobServiceMock.Verify(j => j.EnqueueJob(It.IsAny<TopProductsRequest>()), Times.Never);
        }

        [Fact]
        public async Task JobsProcessTopProducts_ShouldHandleNullStackTrace()
        {
            // Arrange
            var request = CreateValidRequest();
            var exceptionWithoutStackTrace = new Exception("Error without stack trace");

            _validatorMock
                .Setup(v => v.Validate(request))
                .Throws(exceptionWithoutStackTrace);

            // Act
            var result = await _service.JobsProcessTopProducts(request);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("error", result.ToString());

            // Verificar que se logueó el error (con StackTrace porque Throws() genera uno automáticamente)
            _errorLoggerMock.Verify(e => e.LogError(
                request,
                It.Is<string>(s => s.Contains("Error without stack trace")),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task JobsProcessTopProducts_ShouldLogCorrectProductCount()
        {
            // Arrange
            var request = CreateValidRequest();
            request.Payload.TopProducts = new List<ProductSale>
            {
                new ProductSale { ProductId = 1, Name = "Producto1", TotalSold = 10 },
                new ProductSale { ProductId = 2, Name = "Producto2", TotalSold = 20 },
                new ProductSale { ProductId = 3, Name = "Producto3", TotalSold = 15 }
            };

            _validatorMock
                .Setup(v => v.Validate(request))
                .Returns((true, string.Empty, "flow-test"));

            // Act
            await _service.JobsProcessTopProducts(request);

            // Assert
            _eventLoggerMock.Verify(e => e.LogEvent(
                request.CorrelationId,
                request.Service,
                request.Endpoint,
                "TopProductsProcessingStarted",
                It.Is<object>(o => o.ToString()!.Contains("3"))), Times.Once);
        }

        [Fact]
        public async Task JobsProcessTopProducts_ShouldWorkWithMultipleValidationFlows()
        {
            // Arrange
            var request = CreateValidRequest();
            var flows = new[] { "flow-A", "flow-B", "flow-standard" };

            foreach (var flow in flows)
            {
                _validatorMock
                    .Setup(v => v.Validate(request))
                    .Returns((true, string.Empty, flow));

                // Act
                var result = await _service.JobsProcessTopProducts(request);

                // Assert
                Assert.NotNull(result);
                Assert.Contains("queued", result.ToString());

                _requestLoggerMock.Verify(r => r.LogRequest(request, true, string.Empty, flow), Times.Once);
            }
        }

        // Helper method para crear requests válidos
        private TopProductsRequest CreateValidRequest()
        {
            return new TopProductsRequest
            {
                CorrelationId = Guid.NewGuid().ToString(),
                Service = "TestService",
                Endpoint = "api/top-products",
                Payload = new TopProductsPayload
                {
                    TopProducts = new List<ProductSale>
                    {
                        new ProductSale { ProductId = 1, Name = "Producto1", TotalSold = 100 },
                        new ProductSale { ProductId = 2, Name = "Producto2", TotalSold = 85 }
                    },
                    Metadata = new Metadata
                    {
                        PreferredLanguage = "es",
                        ClientType = "web",
                        RequestSource = "unit-test"
                    }
                }
            };
        }
    }
}