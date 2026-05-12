using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class Result_Tests
    {
        [Fact]
        public void Success_Should_Create_Valid_Success_Result()
        {
            var result = Result.Success();

            AssertValidSuccessResult(result);
        }

        [Fact]
        public void GenericSucess_Should_Contain_Value()
        {
            var result = Result<int>.Success(42);

            Assert.Equal(42, result.Value);
            AssertValidSuccessResult(result);
        }

        [Fact]
        public void Generic_ValidationFailure_Should_Throw_When_Accessing_Value()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "Email");

            var validationError = ValidationError.InvalidFormat(errorContext, "mailname@adress.com");

            var result = Result<string>.ValidationFailure([validationError]);

            Assert.Throws<InvalidOperationException>(() => { var value = result.Value; });

            AssertValidFailureResult(result, validationError);
        }

        [Fact]
        public void ValidationFailure_Should_Create_Valid_Failure_Result()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "Email");

            var validationError = ValidationError.InvalidFormat(errorContext, "mailname@adress.com");

            var result = Result.ValidationFailure([validationError]);

            AssertValidFailureResult(result, validationError);
        }

        [Fact]
        public void Failure_Should_Create_Valid_Failure_Result()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "UserId");
            var error = Error.NotFound(errorContext);
            var result = Result.Failure(error);
            AssertValidFailureResult(result, error);
        }

        private static void AssertValidFailureResult(Result result, Error expectedError)
        {
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            if (expectedError.Type == ErrorType.Validation)
            {
                Assert.NotEmpty(result.ValidationErrors);
                Assert.Contains(expectedError, result.ValidationErrors);
            }
            else
            {
                Assert.Equal(expectedError, result.PrimaryError);
            }
        }

        private static void AssertValidSuccessResult(Result result)
        {
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(Error.None, result.PrimaryError);

        }


    }
}
