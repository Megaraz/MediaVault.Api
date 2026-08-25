namespace media_vault_app.Application.Pagination;

public sealed record PageSlice<T>(
    IReadOnlyList<T> Items,
    int TotalCount);
