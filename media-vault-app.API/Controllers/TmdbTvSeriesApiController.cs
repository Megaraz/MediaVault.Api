using media_vault_app.Application.DTOs.Tmdb.TVSeries;
using media_vault_app.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers
{

    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class TmdbTvSeriesApiController : ControllerBase
    {
        private readonly ITmdbTvSeriesApiService _tmdbTvSeriesApiService;

        public TmdbTvSeriesApiController(ITmdbTvSeriesApiService tmdbTvSeriesApiService)
        {
            _tmdbTvSeriesApiService = tmdbTvSeriesApiService;
        }

        [HttpPost("search")]
        public async Task<ActionResult<IReadOnlyList<TvSearchResultDto>>> SearchTvSeries(
            [FromBody] TvSearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? ordering = null)
        {
            var result = await _tmdbTvSeriesApiService.SearchTvSeriesAsync(request.Query, page, pageSize, ordering, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TvSearchResultDto>> GetTvSeriesById(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _tmdbTvSeriesApiService.GetTvSeriesByIdAsync(id, ct);
            return this.ToActionResult(result);
        }

    }
}
