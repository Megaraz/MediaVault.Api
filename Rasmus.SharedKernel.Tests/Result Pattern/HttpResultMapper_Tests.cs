using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class HttpResultMapper_Tests
    {
        // ── Success responses ───────────────────────────────────────────────────

        [Fact]
        public void ToHttpResponse_GenericSuccess_Should_Return_200_With_Value_Body()
        {
            var result = Result<int>.Success(42);

            var response = HttpResultMapper.ToHttpResponse(result);

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(42, response.Body);
            Assert.Null(response.Location);
        }

        [Fact]
        public void ToHttpResponse_Success_Should_Return_200_With_Null_Body()
        {
            var result = Result.Success();

            var response = HttpResultMapper.ToHttpResponse(result);

            Assert.Equal(200, response.StatusCode);
            Assert.Null(response.Body);
            Assert.Null(response.Location);
        }

        [Fact]
        public void ToNoContentResponse_Success_Should_Return_204_With_Null_Body()
        {
            var result = Result.Success();

            var response = HttpResultMapper.ToNoContentResponse(result);

            Assert.Equal(204, response.StatusCode);
            Assert.Null(response.Body);
            Assert.Null(response.Location);
        }

        [Fact]
        public void ToCreatedResponse_GenericSuccess_Should_Return_201_With_Value_Body_And_Location()
        {
            var result = Result<int>.Success(42);

            var response = HttpResultMapper.ToCreatedResponse(result, "/api/items/42");

            Assert.Equal(201, response.StatusCode);
            Assert.Equal(42, response.Body);
            Assert.Equal("/api/items/42", response.Location);
        }

        [Fact]
        public void ToCreatedResponse_GenericSuccess_Should_Return_201_With_Null_Location_When_No_Location_Is_Provided()
        {
            var result = Result<int>.Success(42);

            var response = HttpResultMapper.ToCreatedResponse(result);

            Assert.Equal(201, response.StatusCode);
            Assert.Equal(42, response.Body);
            Assert.Null(response.Location);
        }

        // ── Domain failure status mappings ───────────────────────────────────────

        [Theory]
        [MemberData(nameof(DomainErrorStatusCodeCases))]
        public void Failure_Should_Map_Domain_ErrorType_To_Expected_StatusCode(
            Error error,
            int expectedStatusCode)
        {
            var result = Result.Failure(error, "Test failure message.");

            var response = HttpResultMapper.ToHttpResponse(result);

            AssertErrorResponse(
                response,
                expectedStatusCode,
                "Test failure message.",
                error.Code);
        }

        public static IEnumerable<object[]> DomainErrorStatusCodeCases()
        {
            var ctx = CreateErrorContext();

            yield return [Error.NotFound(ctx), 404];
            yield return [Error.Conflict(ctx), 409];
            yield return [Error.Unauthorized(ctx), 401];

            yield return [new Error("Test.Forbidden.Code", "Forbidden description.", ErrorType.Forbidden, "Forbidden."), 403];
            yield return [Error.Failure(ctx), 500];
            yield return [Error.Cancelled(ctx), 500];

            yield return [new Error("Test.Database.Code", "Database description.", ErrorType.Database, "Database failure."), 500];

            // Fallback/default branch.
            yield return [new Error("Test.Unknown.Code", "Unknown description.", (ErrorType)999, "Unknown failure."), 400];
        }

        // ── HttpError status mappings ────────────────────────────────────────────

        [Theory]
        [MemberData(nameof(HttpErrorStatusCodeCases))]
        public void Failure_Should_Map_HttpErrorType_To_Expected_StatusCode(
            HttpError error,
            int expectedStatusCode)
        {
            var result = Result.Failure(error, "External service failure.");

            var response = HttpResultMapper.ToHttpResponse(result);

            AssertErrorResponse(
                response,
                expectedStatusCode,
                "External service failure.",
                error.Code);
        }

        public static IEnumerable<object[]> HttpErrorStatusCodeCases()
        {
            var ctx = CreateErrorContext();

            yield return [HttpError.BadRequest(ctx), 400];
            yield return [HttpError.UnauthorizedAccess(ctx), 401];
            yield return [HttpError.Forbidden(ctx), 403];
            yield return [HttpError.NotFound(ctx), 404];
            yield return [HttpError.Conflict(ctx), 409];
            yield return [HttpError.UnprocessableContent(ctx), 422];
            yield return [HttpError.TooManyRequests(ctx), 429];

            yield return [HttpError.InternalServerError(ctx), 502];
            yield return [HttpError.TransportFailure(ctx), 503];
            yield return [HttpError.MalformedResponse(ctx), 502];
            yield return [HttpError.UnexpectedStatusCode(ctx, HttpStatusCode.ServiceUnavailable), 502];

            yield return [HttpError.Custom(ctx, "Custom HTTP error."), 502];
        }

        // ── Failure response body mapping ────────────────────────────────────────

        [Fact]
        public void Failure_Should_Return_ErrorResponseBody_With_Message_And_Code()
        {
            var ctx = CreateErrorContext();
            var error = Error.NotFound(ctx);

            var result = Result.Failure(error, "User was not found.");

            var response = HttpResultMapper.ToHttpResponse(result);

            AssertErrorResponse(
                response,
                404,
                "User was not found.",
                error.Code);
        }

        [Fact]
        public void ValidationFailure_Should_Return_422_With_ValidationErrorResponseBody()
        {
            var ctx = CreateErrorContext(fieldName: "Email");

            var validationError = ValidationError.InvalidFormat(
                ctx,
                "example@email.com");

            var result = Result.ValidationFailure(
                [validationError],
                "Validation failed.");

            var response = HttpResultMapper.ToHttpResponse(result);

            AssertValidationErrorResponse(
                response,
                "Validation failed.",
                validationError.Code);
        }

        [Fact]
        public void ValidationFailure_Should_Include_All_ValidationError_Codes()
        {
            var emailContext = CreateErrorContext(fieldName: "Email");
            var passwordContext = CreateErrorContext(fieldName: "Password");

            var emailError = ValidationError.InvalidFormat(
                emailContext,
                "example@email.com");

            var passwordError = ValidationError.TooShort(
                passwordContext,
                "8-64 characters");

            var result = Result.ValidationFailure(
                [emailError, passwordError],
                "Validation failed.");

            var response = HttpResultMapper.ToHttpResponse(result);

            AssertValidationErrorResponse(
                response,
                "Validation failed.",
                emailError.Code,
                passwordError.Code);
        }

        // ── Failure should override success-style response methods ───────────────

        [Fact]
        public void ToHttpResponse_GenericFailure_Should_Map_Failure_Without_Throwing()
        {
            var ctx = CreateErrorContext();
            var error = Error.NotFound(ctx);

            var result = Result<int>.Failure(error, "Not found.");

            var response = HttpResultMapper.ToHttpResponse(result);

            AssertErrorResponse(
                response,
                404,
                "Not found.",
                error.Code);
        }

        [Fact]
        public void ToHttpResponse_GenericValidationFailure_Should_Return_422_With_ValidationErrorResponseBody()
        {
            var ctx = CreateErrorContext(fieldName: "Email");

            var validationError = ValidationError.InvalidFormat(
                ctx,
                "example@email.com");

            var result = Result<string>.ValidationFailure(
                [validationError],
                "Validation failed.");

            var response = HttpResultMapper.ToHttpResponse(result);

            AssertValidationErrorResponse(
                response,
                "Validation failed.",
                validationError.Code);
        }

        [Fact]
        public void ToNoContentResponse_Failure_Should_Map_Failure_Instead_Of_NoContent()
        {
            var ctx = CreateErrorContext();
            var error = Error.Conflict(ctx);

            var result = Result.Failure(error, "Conflict.");

            var response = HttpResultMapper.ToNoContentResponse(result);

            AssertErrorResponse(
                response,
                409,
                "Conflict.",
                error.Code);
        }

        [Fact]
        public void ToCreatedResponse_GenericFailure_Should_Map_Failure_Instead_Of_Created()
        {
            var ctx = CreateErrorContext();
            var error = Error.Conflict(ctx);

            var result = Result<int>.Failure(error, "Conflict.");

            var response = HttpResultMapper.ToCreatedResponse(result, "/api/items/42");

            AssertErrorResponse(
                response,
                409,
                "Conflict.",
                error.Code);

            Assert.Null(response.Location);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static void AssertErrorResponse(
            MappedHttpResponse response,
            int expectedStatusCode,
            string expectedMessage,
            string expectedCode)
        {
            Assert.Equal(expectedStatusCode, response.StatusCode);

            var body = Assert.IsType<ErrorResponseBody>(response.Body);

            Assert.Equal(expectedMessage, body.Message);
            Assert.Equal(expectedCode, body.Code);
            Assert.Null(response.Location);
        }

        private static void AssertValidationErrorResponse(
            MappedHttpResponse response,
            string expectedMessage,
            params string[] expectedValidationErrorCodes)
        {
            Assert.Equal(422, response.StatusCode);

            var body = Assert.IsType<ValidationErrorResponseBody>(response.Body);

            Assert.Equal(expectedMessage, body.Message);
            Assert.NotNull(body.ValidationErrors);

            var actualValidationErrorCodes = body.ValidationErrors!.ToList();

            Assert.Equal(expectedValidationErrorCodes.Length, actualValidationErrorCodes.Count);

            foreach (var expectedCode in expectedValidationErrorCodes)
            {
                Assert.Contains(expectedCode, actualValidationErrorCodes);
            }

            Assert.Null(response.Location);
        }

        private static ErrorContext CreateErrorContext(string fieldName = "UserId")
        {
            return TestErrorContextFactory.Create(fieldName: fieldName);
        }
    }
}
