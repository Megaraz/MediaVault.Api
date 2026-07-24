namespace media_vault_app.Application.DTOs.Tmdb
{
    public sealed record TmdbMovieDetailedDto
    (
        string? TmdbBackdropPath,
        string? TmdbReleaseDate,
        IReadOnlyList<TmdbGenreDto> TmdbGenres,
        int TmdbMovieId,
        string? TmdbOverview,
        string? TmdbPosterPath,
        string? TmdbTitle,
        int TmdbRunTimeMinutes
    );
}
