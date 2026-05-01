using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.DTOs.Tmdb
{
    public sealed record TmdbGenreDto
    {
        public int TmdbGenreId { get; init; }
        public string? TmdbGenreName { get; init; }
    }
}
