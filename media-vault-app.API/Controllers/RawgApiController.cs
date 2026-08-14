using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.DTOs.Rawg;
using media_vault_app.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers
{

    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class RawgApiController : MediaVaultControllerBase
    {
        private readonly IRawgApiService _rawgApiService;

        public RawgApiController(IRawgApiService rawgApiService)
        {
            _rawgApiService = rawgApiService;
        }

        [HttpPost("search")]
        [RequestTimeout(MediaVaultRequestTimeoutPolicies.ExternalMetadata)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status504GatewayTimeout)]
        [ProducesResponseType(typeof(IReadOnlyList<MediaEntryExternalSearchResultDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MediaEntryExternalSearchResultDto>>> SearchGames(
            [FromBody] SearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 8,
            [FromQuery] bool? searchPrecise = null,
            [FromQuery] bool? searchExact = null,
            [FromQuery] string? ordering = null)
        {
            var result = await _rawgApiService.SearchGamesAsync(request.Query, page, pageSize, searchPrecise, searchExact, ordering, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("{id:int}")]
        [RequestTimeout(MediaVaultRequestTimeoutPolicies.ExternalMetadata)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status504GatewayTimeout)]
        [ProducesResponseType(typeof(RawgGameDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<RawgGameDetailedDto>> GetGameById(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _rawgApiService.GetGameByIdAsync(id, ct);
            return this.ToActionResult(result);

        }

    }
}
