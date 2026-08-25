using media_vault_app.Application.DTOs;

namespace media_vault_app.Tests.Pagination;

public sealed class PagedResponseDtoTests
{
    [Theory]
    [InlineData(0, 1, 10, 0, false, false)]
    [InlineData(20, 1, 10, 2, true, false)]
    [InlineData(20, 2, 10, 2, false, true)]
    [InlineData(21, 2, 10, 3, true, true)]
    public void Create_DerivesBoundedMetadata(
        int totalCount,
        int pageNumber,
        int pageSize,
        int totalPages,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        var response = PagedResponseDto<string>.Create(
            Array.Empty<string>(),
            pageNumber,
            pageSize,
            totalCount);

        Assert.Equal(pageNumber, response.PageNumber);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Equal(totalCount, response.TotalCount);
        Assert.Equal(totalPages, response.TotalPages);
        Assert.Equal(hasNextPage, response.HasNextPage);
        Assert.Equal(hasPreviousPage, response.HasPreviousPage);
    }
}
