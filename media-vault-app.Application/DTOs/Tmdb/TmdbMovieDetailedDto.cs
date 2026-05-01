using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.DTOs.Tmdb
{
    public record TmdbMovieDetailedDto
    {
        public int TmdbMovieId { get; init; }
        public IReadOnlyList<TmdbGenreDto> TmdbGenres { get; init; } = new List<TmdbGenreDto>();
        public string? TmdbPosterPath { get; init; }
        public string? TmdbTitle { get; init; }
        public string? TmdbOverview { get; init; }
        public string? TmdbReleaseDate { get; init; }
    }
}
