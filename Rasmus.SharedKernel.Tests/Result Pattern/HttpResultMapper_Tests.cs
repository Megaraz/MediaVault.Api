using System.Net;
using System.Text.Json;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Diagnostics;
using Rasmus.SharedKernel.ResultPatternCompatibility;
using PackageDatabaseError = Megaraz.ResultPattern.Infrastructure.DatabaseError;

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

            yield return [Error.Custom("Test.Forbidden.Code", "Forbidden description.", ErrorType.Forbidden, "Forbidden."), 403];
            yield return [Error.Failure(ctx), 500];
            yield return [Error.Cancelled(ctx), 503];

            yield return [PackageDatabaseError.QueryFailure(ctx, new Exception("database")), 500];

            // Non-HTTP external errors follow MediaVault's approved 500 policy.
            yield return [Error.Custom("Test.External.Code", "External description.", ErrorType.External, "External failure."), 500];
        }

        // ── ErrorType.None is blocked before reaching the mapper ─────────────────

        [Fact]
        public void ToHttpResponse_WithErrorNone_IsBlockedByResultGuard_BeforeReachingMapper()
        {
            // Result.Failure rejects ErrorType.None — it can never reach MapFailure.
            // This test documents the architectural boundary: the mapper's _ fallback
            // is unreachable from Error.None; the guard fires first.
            Assert.Throws<ArgumentException>(() =>
            {
                var result = Result.Failure(Error.None, "Should not reach mapper.");
                HttpResultMapper.ToHttpResponse(result);
            });
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
        public void Failure_Should_Serialize_OrdinaryBody_With_Only_Message_And_Code()
        {
            var error = Error.NotFound(CreateErrorContext());
            var response = HttpResultMapper.ToHttpResponse(Result.Failure(error, "User was not found."));

            var root = SerializeBody(response);

            Assert.Equal(["message", "code"], root.EnumerateObject().Select(x => x.Name));
            Assert.Equal("User was not found.", root.GetProperty("message").GetString());
            Assert.Equal(error.Code, root.GetProperty("code").GetString());
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
                new ValidationErrorItem(validationError.FieldName, validationError.UserMessage));
        }

        [Fact]
        public void ValidationFailure_Should_Serialize_Without_Any_Error_Code()
        {
            var validationError = ValidationError.Required(CreateErrorContext(fieldName: "Email"));
            var response = HttpResultMapper.ToHttpResponse(
                Result.ValidationFailure([validationError], "Validation failed."));

            var root = SerializeBody(response);

            Assert.Equal(["message", "validationErrors"], root.EnumerateObject().Select(x => x.Name));
            Assert.False(root.TryGetProperty("code", out _));
            var item = Assert.Single(root.GetProperty("validationErrors").EnumerateArray());
            Assert.Equal(["field", "message"], item.EnumerateObject().Select(x => x.Name));
            Assert.False(item.TryGetProperty("code", out _));
            Assert.Equal("Email", item.GetProperty("field").GetString());
            Assert.Equal(validationError.UserMessage, item.GetProperty("message").GetString());
        }

        [Fact]
        public void ValidationFailure_Should_Include_All_ValidationErrorItems()
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
                new ValidationErrorItem(emailError.FieldName, emailError.UserMessage),
                new ValidationErrorItem(passwordError.FieldName, passwordError.UserMessage));
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
                new ValidationErrorItem(validationError.FieldName, validationError.UserMessage));
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
            params ValidationErrorItem[] expectedItems)
        {
            Assert.Equal(422, response.StatusCode);

            var body = Assert.IsType<ValidationErrorResponseBody>(response.Body);

            Assert.Equal(expectedMessage, body.Message);
            Assert.NotNull(body.ValidationErrors);

            var actualItems = body.ValidationErrors!.ToList();

            Assert.Equal(expectedItems.Length, actualItems.Count);

            foreach (var expected in expectedItems)
                Assert.Contains(actualItems, actual =>
                    actual.Field == expected.Field && actual.Message == expected.Message);

            Assert.Null(response.Location);
        }

        private static ErrorContext CreateErrorContext(string fieldName = "UserId")
        {
            return PackageErrorContextFactory.Create(fieldName: fieldName);
        }

        private static JsonElement SerializeBody(MappedHttpResponse response)
        {
            Assert.NotNull(response.Body);
            var json = JsonSerializer.Serialize(
                response.Body,
                response.Body.GetType(),
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return JsonSerializer.Deserialize<JsonElement>(json);
        }
    }
}
