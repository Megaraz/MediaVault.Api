using media_vault_app.Application.DTOs.GoogleBooks;
using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.Interfaces.Services;
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
    public class GoogleBooksApiController : MediaVaultControllerBase
    {
        private readonly IGoogleBooksApiService _googleBooksApiService;

        public GoogleBooksApiController(IGoogleBooksApiService googleBooksApiService)
        {
            _googleBooksApiService = googleBooksApiService;
        }

        [HttpPost("search")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.GoogleBooksMetadataByUser)]
        [RequestTimeout(MediaVaultRequestTimeoutPolicies.ExternalMetadata)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status504GatewayTimeout)]
        [ProducesResponseType(typeof(IReadOnlyList<GoogleBooksDetailedDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<GoogleBooksDetailedDto>>> SearchBooks(
            [FromBody] SearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 8)
        {
            var result = await _googleBooksApiService.SearchBooksAsync(request.Query, page, pageSize, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("{volumeId}")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.GoogleBooksMetadataByUser)]
        [RequestTimeout(MediaVaultRequestTimeoutPolicies.ExternalMetadata)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status504GatewayTimeout)]
        [ProducesResponseType(typeof(GoogleBooksDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GoogleBooksDetailedDto>> GetBookById(
            [FromRoute] string volumeId,
            CancellationToken ct)
        {
            var result = await _googleBooksApiService.GetBookByIdAsync(volumeId, ct);
            return this.ToActionResult(result);
        }
    }
}
