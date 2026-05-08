using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests
{
    public class Validator_IsValidId_Tests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Should_Return_False_For_Invalid_Integer(int id)
        {
            Assert.False(Validator.IsValidId(id));
        }

        [Fact]
        public void Should_Return_False_For_Null_Integer()
        {
            int? id = null;

            Assert.False(Validator.IsValidId(id));
        }

        [Fact]
        public void Should_Return_True_For_Valid_Integer()
        {
            Assert.True(Validator.IsValidId(123));
        }

        [Fact]
        public void Should_Return_False_For_Null_String()
        {
            string? id = null;

            Assert.False(Validator.IsValidId(id));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Return_False_For_Invalid_String(string id)
        {
            Assert.False(Validator.IsValidId(id));
        }

        [Fact]
        public void Should_Return_True_For_Valid_String()
        {
            Assert.True(Validator.IsValidId("valid-id"));
        }

        [Fact]
        public void Should_Return_False_For_Empty_Guid()
        {
            Assert.False(Validator.IsValidId(Guid.Empty));
        }

        [Fact]
        public void Should_Return_False_For_Null_Guid()
        {
            Guid? id = null;

            Assert.False(Validator.IsValidId(id));
        }

        [Fact]
        public void Should_Return_True_For_Valid_Guid()
        {
            Assert.True(Validator.IsValidId(Guid.NewGuid()));
        }

        [Fact]
        public void Should_Return_False_For_Default_Long()
        {
            long id = default;

            Assert.False(Validator.IsValidId(id));
        }

        [Fact]
        public void Should_Return_True_For_Valid_Long()
        {
            Assert.True(Validator.IsValidId(1L));
        }
    }
}
