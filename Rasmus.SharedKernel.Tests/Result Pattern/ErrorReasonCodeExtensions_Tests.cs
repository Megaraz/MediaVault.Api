using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class ErrorReasonCodeExtensions_Tests
    {
        // ── ToCodePart — each defined case ───────────────────────────────────

        [Theory]
        [InlineData(ErrorReasonCode.Custom, "Custom")]
        // Validation
        [InlineData(ErrorReasonCode.ValidationRequired, "Required")]
        [InlineData(ErrorReasonCode.ValidationInvalidFormat, "InvalidFormat")]
        [InlineData(ErrorReasonCode.ValidationOutOfRange, "OutOfRange")]
        [InlineData(ErrorReasonCode.ValidationNonMatchingValues, "NonMatchingValues")]
        [InlineData(ErrorReasonCode.ValidationTooShort, "TooShort")]
        [InlineData(ErrorReasonCode.ValidationTooLong, "TooLong")]
        [InlineData(ErrorReasonCode.ValidationAlreadyExists, "AlreadyExists")]
        // Database
        [InlineData(ErrorReasonCode.DatabaseSaveChangesFailure, "DbSaveChangesFailure")]
        [InlineData(ErrorReasonCode.DatabaseConcurrencyFailure, "DbConcurrencyFailure")]
        [InlineData(ErrorReasonCode.DatabaseQueryFailure, "DbQueryFailure")]
        [InlineData(ErrorReasonCode.DatabaseUnexpectedFailure, "DbUnexpectedFailure")]
        // Operation
        [InlineData(ErrorReasonCode.OperationCancelled, "Cancelled")]
        // General
        [InlineData(ErrorReasonCode.GeneralFailure, "Failure")]
        [InlineData(ErrorReasonCode.GeneralNotFound, "NotFound")]
        [InlineData(ErrorReasonCode.GeneralConflict, "Conflict")]
        [InlineData(ErrorReasonCode.GeneralUnauthorized, "Unauthorized")]
        [InlineData(ErrorReasonCode.GeneralForbidden, "Forbidden")]
        [InlineData(ErrorReasonCode.UserLoginFailure, "LoginFailure")]
        // HTTP 4xx
        [InlineData(ErrorReasonCode.HttpBadRequest, "BadRequest")]
        [InlineData(ErrorReasonCode.HttpUnauthorized, "Unauthorized")]
        [InlineData(ErrorReasonCode.HttpForbidden, "Forbidden")]
        [InlineData(ErrorReasonCode.HttpNotFound, "NotFound")]
        [InlineData(ErrorReasonCode.HttpMethodNotAllowed, "MethodNotAllowed")]
        [InlineData(ErrorReasonCode.HttpRequestTimeout, "RequestTimeout")]
        [InlineData(ErrorReasonCode.HttpConflict, "Conflict")]
        [InlineData(ErrorReasonCode.HttpUnprocessableContent, "UnprocessableContent")]
        [InlineData(ErrorReasonCode.HttpTooManyRequests, "TooManyRequests")]
        // HTTP 5xx
        [InlineData(ErrorReasonCode.HttpInternalServerError, "InternalServerError")]
        [InlineData(ErrorReasonCode.HttpBadGateway, "BadGateway")]
        [InlineData(ErrorReasonCode.HttpServiceUnavailable, "ServiceUnavailable")]
        [InlineData(ErrorReasonCode.HttpGatewayTimeout, "GatewayTimeout")]
        [InlineData(ErrorReasonCode.HttpTransportFailure, "TransportFailure")]
        [InlineData(ErrorReasonCode.HttpMalformedResponse, "MalformedResponse")]
        [InlineData(ErrorReasonCode.HttpUnexpectedStatusCode, "UnexpectedStatusCode")]
        public void ToCodePart_Should_Return_Correct_String_For_Each_Defined_Value(
            ErrorReasonCode reason, string expected)
        {
            Assert.Equal(expected, reason.ToCodePart());
        }

        // ── ToCodePart — fallback ────────────────────────────────────────────

        [Fact]
        public void ToCodePart_Should_Return_Unknown_For_Undefined_Enum_Value()
        {
            var undefined = (ErrorReasonCode)9999;

            Assert.Equal("Unknown", undefined.ToCodePart());
        }
    }
}
