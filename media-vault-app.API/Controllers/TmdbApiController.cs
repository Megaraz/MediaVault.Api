using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.DTOs.Tmdb;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using media_vault_app.API.RateLimiting;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class TmdbApiController : MediaVaultControllerBase
    {
        private readonly ITmdbApiService _tmdbApiService;

        public TmdbApiController(ITmdbApiService tmdbApiService)
        {
            _tmdbApiService = tmdbApiService;
        }

        [HttpPost("movie/search")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.TmdbMetadataByUser)]
        [RequestTimeout(MediaVaultRequestTimeoutPolicies.ExternalMetadata)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status504GatewayTimeout)]
        [ProducesResponseType(typeof(IReadOnlyList<MediaEntryExternalSearchResultDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MediaEntryExternalSearchResultDto>>> SearchMovies(
            [FromBody] SearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1)
        {
            var result = await _tmdbApiService.SearchAsync(request.Query, MediaType.Movie, page, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("movie/{id:int}")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.TmdbMetadataByUser)]
        [RequestTimeout(MediaVaultRequestTimeoutPolicies.ExternalMetadata)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status504GatewayTimeout)]
        [ProducesResponseType(typeof(TmdbMovieDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TmdbMovieDetailedDto>> GetMovieById(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _tmdbApiService.GetMovieByIdAsync(id, ct);
            return this.ToActionResult(result);
        }

        [HttpPost("tv/search")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.TmdbMetadataByUser)]
        [RequestTimeout(MediaVaultRequestTimeoutPolicies.ExternalMetadata)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status504GatewayTimeout)]
        [ProducesResponseType(typeof(IReadOnlyList<MediaEntryExternalSearchResultDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MediaEntryExternalSearchResultDto>>> SearchTvSeries(
            [FromBody] SearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1)
        {
            var result = await _tmdbApiService.SearchAsync(request.Query, MediaType.TvSeries, page, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("tv/{id:int}")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.TmdbMetadataByUser)]
        [RequestTimeout(MediaVaultRequestTimeoutPolicies.ExternalMetadata)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status504GatewayTimeout)]
        [ProducesResponseType(typeof(TmdbTvSeriesDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TmdbTvSeriesDetailedDto>> GetTvSeriesById(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _tmdbApiService.GetTvSeriesByIdAsync(id, ct);
            return this.ToActionResult(result);
        }
    }
}
