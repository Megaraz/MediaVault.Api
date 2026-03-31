using media_vault_app.Application.DTOs.Tmdb.Movie;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class TmdbApiController : ControllerBase
    {
        private readonly ITmdbApiService _tmdbApiService;

        public TmdbApiController(ITmdbApiService tmdbApiService)
        {
            _tmdbApiService = tmdbApiService;
        }

        [HttpPost("movie/search")]
        public async Task<ActionResult<IReadOnlyList<TmdbSearchResultDto>>> SearchMovies(
            [FromBody] SearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? ordering = null)
        {
            var result = await _tmdbApiService.SearchAsync(request.Query, MediaEntryType.MovieEntry, page, pageSize, ordering, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("movie/{id:int}")]
        public async Task<ActionResult<TmdbSearchResultDto>> GetMovieById(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _tmdbApiService.GetByIdAsync(id, MediaEntryType.MovieEntry, ct);
            return this.ToActionResult(result);
        }

        [HttpPost("tv/search")]
        public async Task<ActionResult<IReadOnlyList<TmdbSearchResultDto>>> SearchTvSeries(
            [FromBody] SearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? ordering = null)
        {
            var result = await _tmdbApiService.SearchAsync(request.Query, MediaEntryType.SeriesEntry, page, pageSize, ordering, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("tv/{id:int}")]
        public async Task<ActionResult<TmdbSearchResultDto>> GetTvSeriesById(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _tmdbApiService.GetByIdAsync(id, MediaEntryType.SeriesEntry, ct);
            return this.ToActionResult(result);
        }
    }
}
