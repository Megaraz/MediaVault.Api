using media_vault_app.Application.DTOs.ExternalAPIs;
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
        public async Task<ActionResult<IReadOnlyList<SearchResultDto>>> SearchMovies(
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
        public async Task<ActionResult<SearchResultDto>> GetMovieById(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _tmdbApiService.GetByIdAsync(id, MediaType.Movie, ct);
            return this.ToActionResult(result);
        }

        [HttpPost("tv/search")]
        public async Task<ActionResult<IReadOnlyList<SearchResultDto>>> SearchTvSeries(
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
        public async Task<ActionResult<SearchResultDto>> GetTvSeriesById(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _tmdbApiService.GetByIdAsync(id, MediaType.TvSeries, ct);
            return this.ToActionResult(result);
        }
    }
}
