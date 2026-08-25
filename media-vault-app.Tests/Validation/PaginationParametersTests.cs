using media_vault_app.Application.Pagination;

namespace media_vault_app.Tests.Validation
{
    public class Validator_PaginationParameters_Tests
    {
        // ── pageNumber low-end clamping ──────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void PageNumber_Below_One_Is_Clamped_To_One(int invalidPageNumber)
        {
            var result = PaginationParameters.Normalize(invalidPageNumber, 10);

            Assert.Equal(1, result.PageNumber);
        }

        [Fact]
        public void Valid_PageNumber_Is_Unchanged()
        {
            var result = PaginationParameters.Normalize(5, 10);

            Assert.Equal(5, result.PageNumber);
        }

        // ── pageSize low-end clamping ────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void PageSize_Below_One_Is_Clamped_To_One(int invalidPageSize)
        {
            var result = PaginationParameters.Normalize(1, invalidPageSize);

            Assert.Equal(1, result.PageSize);
        }

        [Fact]
        public void Valid_PageSize_Within_Default_Cap_Is_Unchanged()
        {
            var result = PaginationParameters.Normalize(1, 25);

            Assert.Equal(25, result.PageSize);
        }

        // ── pageSize high-end clamping ───────────────────────────────────────

        [Fact]
        public void PageSize_Above_Default_MaxPageSize_Is_Clamped_To_100()
        {
            var result = PaginationParameters.Normalize(1, 10_000);

            Assert.Equal(100, result.PageSize);
        }

        [Fact]
        public void PageSize_Exactly_At_Default_MaxPageSize_Is_Not_Clamped()
        {
            var result = PaginationParameters.Normalize(1, 100);

            Assert.Equal(100, result.PageSize);
        }

        [Fact]
        public void Custom_MaxPageSize_Is_Respected()
        {
            var result = PaginationParameters.Normalize(1, 50, maxPageSize: 25);

            Assert.Equal(25, result.PageSize);
        }

        // ── both parameters clamped ──────────────────────────────────────────

        [Fact]
        public void Both_Invalid_PageNumber_And_PageSize_Are_Each_Clamped()
        {
            var result = PaginationParameters.Normalize(-5, -10);

            Assert.Equal(1, result.PageNumber);
            Assert.Equal(1, result.PageSize);
        }
    }
}
