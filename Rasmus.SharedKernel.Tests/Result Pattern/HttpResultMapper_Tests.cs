using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class HttpResultMapper_Tests
    {

        [Fact]
        public void ToHttpResponse_WithValidationError_ReturnsBadRequest()
        {
            // Arrange

            var validationError = ValidationError.AlreadyExists(TestErrorContextFactory.Create());
            var result = Result.ValidationFailure([validationError]);
            // Act
            var httpResponse = HttpResultMapper.ToHttpResponse(result);
            // Assert
            Assert.Equal(400, httpResponse.StatusCode);
            Assert.NotNull(httpResponse.Body);
            Assert.IsType<ValidationError>(httpResponse.Body);
            var error = (ValidationError)httpResponse.Body;
            AssertCommon(error, ValidationErrorType.Required);
        }

        private static void AssertCommon(ValidationError error, ValidationErrorType expectedType)
        {
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.Equal(ErrorType.Validation, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(expectedType, error.ValidationErrorType);
        }

    }
}
