using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests
{
    public class Validator_PaginationParameters_Tests
    {
        // ── pageNumber clamping ───────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void PageNumber_Below_One_Should_Be_Set_To_One(int invalidPageNumber)
        {
            int pageNumber = invalidPageNumber;
            int pageSize = 10;

            Validator.ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            Assert.Equal(1, pageNumber);
        }

        [Fact]
        public void Valid_PageNumber_Should_Not_Be_Changed()
        {
            int pageNumber = 5;
            int pageSize = 10;

            Validator.ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            Assert.Equal(5, pageNumber);
        }

        // ── pageSize clamping ─────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void PageSize_Below_One_Should_Be_Set_To_One(int invalidPageSize)
        {
            int pageNumber = 1;
            int pageSize = invalidPageSize;

            Validator.ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            Assert.Equal(1, pageSize);
        }

        [Fact]
        public void Valid_PageSize_Should_Not_Be_Changed()
        {
            int pageNumber = 1;
            int pageSize = 25;

            Validator.ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            Assert.Equal(25, pageSize);
        }

        // ── Both parameters adjusted independently ────────────────────────────

        [Fact]
        public void Both_Invalid_PageNumber_And_PageSize_Should_Each_Be_Clamped_To_One()
        {
            int pageNumber = -5;
            int pageSize = -10;

            Validator.ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            Assert.Equal(1, pageNumber);
            Assert.Equal(1, pageSize);
        }
    }
}
