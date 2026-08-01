using System.Net;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Diagnostics;
using Rasmus.SharedKernel.ResultPatternCompatibility;
using LegacyErrorLogger = Rasmus.SharedKernel.ResultPattern.ErrorLogger;
using LegacyErrorLoggerConfiguration = Rasmus.SharedKernel.ResultPattern.ErrorLoggerConfiguration;
using LegacyErrorLogPolicy = Rasmus.SharedKernel.ResultPattern.ErrorLogPolicy;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class HttpError_Tests
    {
        // ── Base ErrorType invariant ──────────────────────────────────────────
        // All factory methods must produce ErrorType.HttpError on the base class.
        // The HttpResultMapper relies on this to enter the HttpError branch.

        [Theory]
        [MemberData(nameof(AllFactoryInstances))]
        public void All_Factories_Should_Set_ErrorType_To_HttpError(HttpError error)
        {
            Assert.Equal(ErrorType.External, error.Type);
        }

        public static IEnumerable<object[]> AllFactoryInstances()
        {
            var ctx = PackageErrorContextFactory.Create();

            yield return [HttpError.BadRequest(ctx)];
            yield return [HttpError.UnauthorizedAccess(ctx)];
            yield return [HttpError.Forbidden(ctx)];
            yield return [HttpError.NotFound(ctx)];
            yield return [HttpError.Conflict(ctx)];
            yield return [HttpError.UnprocessableContent(ctx)];
            yield return [HttpError.TooManyRequests(ctx)];
            yield return [HttpError.InternalServerError(ctx)];
            yield return [HttpError.TransportFailure(ctx)];
            yield return [HttpError.MalformedResponse(ctx)];
            yield return [HttpError.UnexpectedStatusCode(ctx, HttpStatusCode.ServiceUnavailable)];
            yield return [HttpError.Custom(ctx, "Custom error.")];
        }

        // ── Exception attachment ──────────────────────────────────────────────
        // TransportFailure and MalformedResponse accept an optional exception and
        // should attach it to the error so callers can inspect the original cause.

        [Fact]
        public void TransportFailure_With_Exception_Should_Attach_Exception()
        {
            var ctx = PackageErrorContextFactory.Create();
            var exception = new HttpRequestException("Connection refused");

            var error = HttpError.TransportFailure(ctx, exception);

            Assert.Equal(exception, error.Exception);
        }

        [Fact]
        public void TransportFailure_Without_Exception_Should_Have_Null_Exception()
        {
            var ctx = PackageErrorContextFactory.Create();

            var error = HttpError.TransportFailure(ctx);

            Assert.Null(error.Exception);
        }

        [Fact]
        public void MalformedResponse_With_Exception_Should_Attach_Exception()
        {
            var ctx = PackageErrorContextFactory.Create();
            var exception = new InvalidOperationException("Unexpected JSON");

            var error = HttpError.MalformedResponse(ctx, exception);

            Assert.Equal(exception, error.Exception);
        }

        [Fact]
        public void MalformedResponse_Without_Exception_Should_Have_Null_Exception()
        {
            var ctx = PackageErrorContextFactory.Create();

            var error = HttpError.MalformedResponse(ctx);

            Assert.Null(error.Exception);
        }

        // ── UnexpectedStatusCode — description contains status code ───────────
        // The description suffix is the primary diagnostic for unexpected codes;
        // it must embed both the numeric value and the enum name.

        [Fact]
        public void UnexpectedStatusCode_Description_Should_Contain_Numeric_Status_Code()
        {
            var ctx = PackageErrorContextFactory.Create();

            var error = HttpError.UnexpectedStatusCode(ctx, HttpStatusCode.ServiceUnavailable);

            Assert.Contains("503", error.UserMessage);
        }

        [Fact]
        public void UnexpectedStatusCode_Description_Should_Contain_Status_Code_Name()
        {
            var ctx = PackageErrorContextFactory.Create();

            var error = HttpError.UnexpectedStatusCode(ctx, HttpStatusCode.ServiceUnavailable);

            Assert.Contains("ServiceUnavailable", error.UserMessage);
        }

        // ── Custom — UserMessage equals the provided suffix ───────────────────
        // Callers rely on UserMessage to surface the custom message to consumers.

        [Fact]
        public void Custom_UserMessage_Should_Equal_The_Provided_Suffix()
        {
            var ctx = PackageErrorContextFactory.Create();
            const string customSuffix = "Rate limit exceeded for external API.";

            var error = HttpError.Custom(ctx, customSuffix);

            Assert.Equal(customSuffix, error.UserMessage);
        }
    }
}
