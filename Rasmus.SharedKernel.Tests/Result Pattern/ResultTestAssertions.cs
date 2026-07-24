using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    internal static class ResultTestAssertions
    {
        public static void AssertValidSuccessResult(Result result)
        {
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(string.Empty, result.Message);
            Assert.Equal(Error.None, result.PrimaryError);
            Assert.Empty(result.ValidationErrors);
        }

        public static void AssertValidFailureResult(
            Result result,
            Error expectedError,
            string expectedMessage)
        {
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(expectedMessage, result.Message);
            Assert.Equal(expectedError, result.PrimaryError);

            if (expectedError.Type == ErrorType.Validation)
            {
                Assert.NotEmpty(result.ValidationErrors);
                Assert.Contains(expectedError, result.ValidationErrors);
            }
            else
            {
                Assert.Empty(result.ValidationErrors);
            }
        }
    }
}
