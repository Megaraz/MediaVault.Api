using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class ResultExtensions_Tests
    {
        // ── Map — success path ────────────────────────────────────────────────

        [Fact]
        public void Map_On_Success_Should_Apply_The_Function_And_Return_Success()
        {
            var source = Result<int>.Success(5);

            var result = source.Map(x => x * 2);

            Assert.True(result.IsSuccess);
            Assert.Equal(10, result.Value);
        }

        [Fact]
        public void Map_On_Success_Should_Return_Correct_Output_Type()
        {
            var source = Result<int>.Success(42);

            var result = source.Map(x => x.ToString());

            Assert.IsType<Result<string>>(result);
            Assert.Equal("42", result.Value);
        }

        // ── Map — failure path ────────────────────────────────────────────────

        [Fact]
        public void Map_On_Failure_Should_Not_Invoke_The_Function()
        {
            var ctx = TestErrorContextFactory.Create();
            var source = Result<int>.Failure(Error.NotFound(ctx));
            var wasCalled = false;

            source.Map(x => { wasCalled = true; return x * 2; });

            Assert.False(wasCalled);
        }

        [Fact]
        public void Map_On_Failure_Should_Preserve_The_Original_Error()
        {
            var ctx = TestErrorContextFactory.Create();
            var originalError = Error.NotFound(ctx);
            var source = Result<int>.Failure(originalError, "Not found.");

            var result = source.Map(x => x.ToString());

            Assert.True(result.IsFailure);
            Assert.Equal(originalError, result.PrimaryError);
        }

        [Fact]
        public void Map_On_Failure_Should_Preserve_The_Original_Message()
        {
            var ctx = TestErrorContextFactory.Create();
            var source = Result<int>.Failure(Error.NotFound(ctx), "Not found.");

            var result = source.Map(x => x.ToString());

            Assert.Equal("Not found.", result.Message);
        }

        // ── From — copies failure state to a new value type ───────────────────

        [Fact]
        public void From_On_Failure_Should_Preserve_The_Error()
        {
            var ctx = TestErrorContextFactory.Create();
            var originalError = Error.Failure(ctx);
            var source = Result<int>.Failure(originalError, "Something failed.");

            var result = source.From<int, string>();

            Assert.True(result.IsFailure);
            Assert.Equal(originalError, result.PrimaryError);
        }

        [Fact]
        public void From_On_Failure_Should_Preserve_The_Message()
        {
            var ctx = TestErrorContextFactory.Create();
            var source = Result<int>.Failure(Error.Failure(ctx), "Something failed.");

            var result = source.From<int, string>();

            Assert.Equal("Something failed.", result.Message);
        }

        [Fact]
        public void From_On_ValidationFailure_Should_Preserve_ValidationErrors()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: "Email");
            var validationError = ValidationError.Required(ctx);
            var source = Result<int>.ValidationFailure([validationError]);

            var result = source.From<int, string>();

            Assert.True(result.IsFailure);
            Assert.Contains(validationError, result.ValidationErrors);
        }
    }
}
