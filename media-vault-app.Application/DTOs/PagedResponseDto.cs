namespace media_vault_app.Application.DTOs;

public sealed record PagedResponseDto<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public required int PageNumber { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }

    public required int TotalPages { get; init; }

    public required bool HasNextPage { get; init; }

    public required bool HasPreviousPage { get; init; }

    public static PagedResponseDto<T> Create(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponseDto<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = pageNumber < totalPages,
            HasPreviousPage = pageNumber > 1 && totalPages > 0
        };
    }
}
