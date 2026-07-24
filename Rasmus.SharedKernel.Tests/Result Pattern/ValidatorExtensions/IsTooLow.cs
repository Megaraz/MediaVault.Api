using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class ValidatorExtensions_IsTooLow_Tests
    {
        [Theory]
        [InlineData(3, 5)]
        [InlineData(-1, 0)]
        [InlineData(4, 5)]
        public void Should_Return_True_And_Error_When_Value_Is_Below_MinValue(int value, int minValue)
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "Age");

            var result = ValidatorExtensions.IsTooLow(value, minValue, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsOutOfRange(error, fieldName: "Age", entityName: "User");
            Assert.Contains(minValue.ToString(), error.Description, StringComparison.Ordinal);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_When_Value_Equals_MinValue()
        {
            int value = 5;
            int minValue = 5;
            var errorContext = TestErrorContextFactory.Create(fieldName: "Age");

            var result = ValidatorExtensions.IsTooLow(value, minValue, errorContext, out var error);

            Assert.False(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_When_Value_Is_Above_MinValue()
        {
            int value = 10;
            int minValue = 5;
            var errorContext = TestErrorContextFactory.Create(fieldName: "Age");

            var result = ValidatorExtensions.IsTooLow(value, minValue, errorContext, out var error);

            Assert.False(result);
            Assert.Null(error);
        }
    }
}
