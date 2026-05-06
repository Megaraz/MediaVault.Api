using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.DTOs.Tmdb;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class TmdbApiController : ControllerBase
    {
        private readonly ITmdbApiService _tmdbApiService;

        public TmdbApiController(ITmdbApiService tmdbApiService)
        {
            _tmdbApiService = tmdbApiService;
        }

        [HttpPost("movie/search")]
        public async Task<ActionResult<IReadOnlyList<MediaEntrySearchResultDto>>> SearchMovies(
            [FromBody] SearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? ordering = null)
        {
            var result = await _tmdbApiService.SearchAsync(request.Query, MediaType.Movie, page, pageSize, ordering, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("movie/{id:int}")]
        public async Task<ActionResult<TmdbMovieDetailedDto>> GetMovieById(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _tmdbApiService.GetMovieByIdAsync(id, ct);
            return this.ToActionResult(result);
        }

        [HttpPost("tv/search")]
        public async Task<ActionResult<IReadOnlyList<MediaEntrySearchResultDto>>> SearchTvSeries(
            [FromBody] SearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? ordering = null)
        {
            var result = await _tmdbApiService.SearchAsync(request.Query, MediaType.TvSeries, page, pageSize, ordering, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("tv/{id:int}")]
        public async Task<ActionResult<TmdbTvSeriesDetailedDto>> GetTvSeriesById(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _tmdbApiService.GetTvSeriesByIdAsync(id, ct);
            return this.ToActionResult(result);
        }
    }
}
