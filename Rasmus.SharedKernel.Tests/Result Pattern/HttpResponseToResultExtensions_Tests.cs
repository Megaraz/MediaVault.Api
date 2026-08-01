using System.Net;
using System.Text;
using System.Text.Json;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Diagnostics;
using Rasmus.SharedKernel.ResultPatternCompatibility;
using LegacyErrorLogger = Rasmus.SharedKernel.ResultPattern.ErrorLogger;
using LegacyErrorLoggerConfiguration = Rasmus.SharedKernel.ResultPattern.ErrorLoggerConfiguration;
using LegacyErrorLogPolicy = Rasmus.SharedKernel.ResultPattern.ErrorLogPolicy;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    // -------------------------------------------------------------------------
    // MapToResultAsync<TValue> — generic overload
    // -------------------------------------------------------------------------
    public class HttpResponseToResultExtensions_Generic_Tests
    {
        private static readonly ErrorContext ErrorContext = PackageErrorContextFactory.Create();

        private record TestDto(string Name, int Value);

        // --- Null response ---

        [Fact]
        public async Task NullResponse_Returns_Failure_With_TransportFailure()
        {
            HttpResponseMessage? response = null;

            var result = await response.MapToResultAsync<TestDto>(ErrorContext);

            Assert.True(result.IsFailure);
            var httpError = Assert.IsType<HttpError>(result.PrimaryError);
            Assert.Equal(HttpErrorType.TransportFailure, httpError.HttpErrorType);
        }

        // --- Non-success status codes ---

        [Theory]
        [InlineData(HttpStatusCode.NotFound, HttpErrorType.NotFound)]
        [InlineData(HttpStatusCode.BadRequest, HttpErrorType.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized, HttpErrorType.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden, HttpErrorType.Forbidden)]
        [InlineData(HttpStatusCode.Conflict, HttpErrorType.Conflict)]
        [InlineData(HttpStatusCode.UnprocessableContent, HttpErrorType.UnprocessableContent)]
        [InlineData(HttpStatusCode.InternalServerError, HttpErrorType.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests, HttpErrorType.TooManyRequests)]
        public async Task NonSuccess_StatusCode_Returns_Failure_With_Expected_HttpErrorType(
            HttpStatusCode statusCode,
            HttpErrorType expectedErrorType)
        {
            using var response = new HttpResponseMessage(statusCode);

            var result = await response.MapToResultAsync<TestDto>(ErrorContext);

            Assert.True(result.IsFailure);
            var httpError = Assert.IsType<HttpError>(result.PrimaryError);
            Assert.Equal(expectedErrorType, httpError.HttpErrorType);
        }

        [Fact]
        public async Task Unhandled_StatusCode_Returns_Failure_With_UnexpectedStatusCode()
        {
            using var response = new HttpResponseMessage((HttpStatusCode)418); // I'm a teapot

            var result = await response.MapToResultAsync<TestDto>(ErrorContext);

            Assert.True(result.IsFailure);
            var httpError = Assert.IsType<HttpError>(result.PrimaryError);
            Assert.Equal(HttpErrorType.UnexpectedStatusCode, httpError.HttpErrorType);
        }

        // --- Success path: valid response ---

        [Fact]
        public async Task Success_With_Valid_Json_Returns_Deserialized_Value()
        {
            var dto = new TestDto("Alice", 42);
            var json = JsonSerializer.Serialize(dto);
            using var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var result = await response.MapToResultAsync<TestDto>(ErrorContext);

            Assert.True(result.IsSuccess);
            Assert.Equal("Alice", result.Value.Name);
            Assert.Equal(42, result.Value.Value);
        }

        // --- Success path: malformed / missing response body ---

        [Fact]
        public async Task Success_With_Empty_Body_Returns_Failure_With_MalformedResponse()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            var result = await response.MapToResultAsync<TestDto>(ErrorContext);

            Assert.True(result.IsFailure);
            var httpError = Assert.IsType<HttpError>(result.PrimaryError);
            Assert.Equal(HttpErrorType.MalformedResponse, httpError.HttpErrorType);
        }

        [Fact]
        public async Task Success_With_Json_Null_Returns_Failure_With_MalformedResponse()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };

            var result = await response.MapToResultAsync<TestDto>(ErrorContext);

            Assert.True(result.IsFailure);
            var httpError = Assert.IsType<HttpError>(result.PrimaryError);
            Assert.Equal(HttpErrorType.MalformedResponse, httpError.HttpErrorType);
        }

        [Fact]
        public async Task Success_With_Malformed_Json_Returns_Failure_With_MalformedResponse()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ not valid json %%", Encoding.UTF8, "application/json")
            };

            var result = await response.MapToResultAsync<TestDto>(ErrorContext);

            Assert.True(result.IsFailure);
            var httpError = Assert.IsType<HttpError>(result.PrimaryError);
            Assert.Equal(HttpErrorType.MalformedResponse, httpError.HttpErrorType);
        }

        [Fact]
        public async Task Success_With_Unsupported_ContentType_Returns_Failure_With_MalformedResponse()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<xml><Name>Alice</Name></xml>", Encoding.UTF8, "application/xml")
            };

            var result = await response.MapToResultAsync<TestDto>(ErrorContext);

            Assert.True(result.IsFailure);
            var httpError = Assert.IsType<HttpError>(result.PrimaryError);
            Assert.Equal(HttpErrorType.MalformedResponse, httpError.HttpErrorType);
        }
    }

    // -------------------------------------------------------------------------
    // MapToResultAsync — non-generic overload
    // -------------------------------------------------------------------------
    public class HttpResponseToResultExtensions_NonGeneric_Tests
    {
        private static readonly ErrorContext ErrorContext = PackageErrorContextFactory.Create();

        [Fact]
        public async Task NullResponse_Returns_Failure_With_TransportFailure()
        {
            HttpResponseMessage? response = null;

            var result = await response.MapToResultAsync(ErrorContext);

            Assert.True(result.IsFailure);
            var httpError = Assert.IsType<HttpError>(result.PrimaryError);
            Assert.Equal(HttpErrorType.TransportFailure, httpError.HttpErrorType);
        }

        [Fact]
        public async Task Success_Response_Returns_Success()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.OK);

            var result = await response.MapToResultAsync(ErrorContext);

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound, HttpErrorType.NotFound)]
        [InlineData(HttpStatusCode.BadRequest, HttpErrorType.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized, HttpErrorType.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden, HttpErrorType.Forbidden)]
        [InlineData(HttpStatusCode.Conflict, HttpErrorType.Conflict)]
        [InlineData(HttpStatusCode.UnprocessableContent, HttpErrorType.UnprocessableContent)]
        [InlineData(HttpStatusCode.InternalServerError, HttpErrorType.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests, HttpErrorType.TooManyRequests)]
        public async Task NonSuccess_StatusCode_Returns_Failure_With_Expected_HttpErrorType(
            HttpStatusCode statusCode,
            HttpErrorType expectedErrorType)
        {
            using var response = new HttpResponseMessage(statusCode);

            var result = await response.MapToResultAsync(ErrorContext);

            Assert.True(result.IsFailure);
            var httpError = Assert.IsType<HttpError>(result.PrimaryError);
            Assert.Equal(expectedErrorType, httpError.HttpErrorType);
        }

        [Fact]
        public async Task Unhandled_StatusCode_Returns_Failure_With_UnexpectedStatusCode()
        {
            using var response = new HttpResponseMessage((HttpStatusCode)418);

            var result = await response.MapToResultAsync(ErrorContext);

            Assert.True(result.IsFailure);
            var httpError = Assert.IsType<HttpError>(result.PrimaryError);
            Assert.Equal(HttpErrorType.UnexpectedStatusCode, httpError.HttpErrorType);
        }
    }
}
