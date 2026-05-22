using System.Net;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class ErrorLogPolicy_Tests
    {
        private readonly ErrorLogPolicy _policy = new();

        // ── Never log ─────────────────────────────────────────────────────────

        [Fact]
        public void Should_Not_Log_ValidationError()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: "Email");
            var error = ValidationError.Required(ctx);

            Assert.False(_policy.ShouldLog(error));
        }

        [Fact]
        public void Should_Not_Log_Cancelled_Error()
        {
            var ctx = TestErrorContextFactory.Create();
            var error = Error.Cancelled(ctx);

            Assert.False(_policy.ShouldLog(error));
        }

        // ── HttpError — never log (4xx client errors) ─────────────────────────

        [Theory]
        [MemberData(nameof(HttpErrorsThatShouldNotBeLogged))]
        public void Should_Not_Log_Http_Client_Errors(HttpError error)
        {
            Assert.False(_policy.ShouldLog(error));
        }

        public static IEnumerable<object[]> HttpErrorsThatShouldNotBeLogged()
        {
            var ctx = TestErrorContextFactory.Create();

            yield return [HttpError.BadRequest(ctx)];
            yield return [HttpError.NotFound(ctx)];
            yield return [HttpError.Conflict(ctx)];
            yield return [HttpError.UnprocessableContent(ctx)];
        }

        // ── Always log ────────────────────────────────────────────────────────

        [Fact]
        public void Should_Log_DatabaseError()
        {
            var ctx = TestErrorContextFactory.Create();
            var error = DatabaseError.SaveChangesFailure(ctx, new Exception("db failure"));

            Assert.True(_policy.ShouldLog(error));
        }

        [Fact]
        public void Should_Log_General_Failure()
        {
            var ctx = TestErrorContextFactory.Create();
            var error = Error.Failure(ctx);

            Assert.True(_policy.ShouldLog(error));
        }

        [Fact]
        public void Should_Log_NotFound_Error()
        {
            var ctx = TestErrorContextFactory.Create();
            var error = Error.NotFound(ctx);

            Assert.True(_policy.ShouldLog(error));
        }

        // ── HttpError — always log (auth + server errors) ─────────────────────

        [Theory]
        [MemberData(nameof(HttpErrorsThatShouldBeLogged))]
        public void Should_Log_Http_Auth_And_Server_Errors(HttpError error)
        {
            Assert.True(_policy.ShouldLog(error));
        }

        public static IEnumerable<object[]> HttpErrorsThatShouldBeLogged()
        {
            var ctx = TestErrorContextFactory.Create();

            yield return [HttpError.UnauthorizedAccess(ctx)];
            yield return [HttpError.Forbidden(ctx)];
            yield return [HttpError.InternalServerError(ctx)];
            yield return [HttpError.TooManyRequests(ctx)];
            yield return [HttpError.TransportFailure(ctx)];
            yield return [HttpError.MalformedResponse(ctx)];
            yield return [HttpError.UnexpectedStatusCode(ctx, HttpStatusCode.ServiceUnavailable)];
            yield return [HttpError.Custom(ctx, "Custom.")];
        }
    }
}
