using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.API.RateLimiting;
using media_vault_app.API.RequestLimits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class MediaEntriesController : MediaVaultControllerBase
    {
        private readonly IMediaEntryReadService _readService;
        private readonly IMediaEntryWriteService _writeService;

        public MediaEntriesController(
            IMediaEntryReadService readService,
            IMediaEntryWriteService writeService)
        {
            _readService = readService;
            _writeService = writeService;
        }

        #region Create Operations - Type-Specific Endpoints

        [HttpPost("movies")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(MediaEntryDetailedDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateMovie(
            [FromBody] MovieEntryCreateDto createDto,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreatedResult(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

        [HttpPost("tv-series")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(MediaEntryDetailedDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateTvSeries(
            [FromBody] TvSeriesEntryCreateDto createDto,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreatedResult(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

        [HttpPost("games")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(MediaEntryDetailedDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateGame(
            [FromBody] GameEntryCreateDto createDto,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreatedResult(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

        [HttpPost("books")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(MediaEntryDetailedDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateBook(
            [FromBody] BookEntryCreateDto createDto,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreatedResult(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

        [HttpPost("manga")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(MediaEntryDetailedDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateManga(
            [FromBody] MangaEntryCreateDto createDto,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreatedResult(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

        #endregion

        #region Update Operations - Type-Specific Endpoints

        [HttpPut("movies/{id:Guid}")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateMovie(
            [FromRoute] Guid id,
            [FromBody] MovieEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContentResult(result);
        }

        [HttpPut("tv-series/{id:Guid}")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateTvSeries(
            [FromRoute] Guid id,
            [FromBody] TvSeriesEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContentResult(result);
        }

        [HttpPut("games/{id:Guid}")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateGame(
            [FromRoute] Guid id,
            [FromBody] GameEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContentResult(result);
        }

        [HttpPut("books/{id:Guid}")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateBook(
            [FromRoute] Guid id,
            [FromBody] BookEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContentResult(result);
        }

        [HttpPut("manga/{id:Guid}")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateManga(
            [FromRoute] Guid id,
            [FromBody] MangaEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContentResult(result);
        }

        #endregion


        #region Read Operations - Type-Specific Endpoints

        [HttpGet("movies/{id:Guid}")]
        [ProducesResponseType(typeof(MovieEntryDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<MovieEntryDetailedDto>> GetMovieById(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _readService.GetMovieByIdAsync(userId, id, ct);

            return this.ToActionResult(result);
        }

        [HttpGet("tv-series/{id:Guid}")]
        [ProducesResponseType(typeof(TvSeriesEntryDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TvSeriesEntryDetailedDto>> GetTvSeriesById(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _readService.GetTvSeriesByIdAsync(userId, id, ct);

            return this.ToActionResult(result);
        }

        [HttpGet("games/{id:Guid}")]
        [ProducesResponseType(typeof(GameEntryDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GameEntryDetailedDto>> GetGameById(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _readService.GetGameByIdAsync(userId, id, ct);

            return this.ToActionResult(result);
        }

        [HttpGet("books/{id:Guid}")]
        [ProducesResponseType(typeof(BookEntryDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<BookEntryDetailedDto>> GetBookById(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _readService.GetBookByIdAsync(userId, id, ct);

            return this.ToActionResult(result);
        }

        [HttpGet("manga/{id:Guid}")]
        [ProducesResponseType(typeof(MangaEntryDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<MangaEntryDetailedDto>> GetMangaById(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _readService.GetMangaByIdAsync(userId, id, ct);

            return this.ToActionResult(result);
        }
        #endregion

        #region Read Operations - Shared Endpoints

        [HttpPost("search")]
        [ProducesResponseType(typeof(IReadOnlyList<MediaEntryMinimalDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MediaEntryMinimalDto>>> SearchMediaEntries(
            [FromBody] SearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _readService.SearchMediaEntriesAsync(userId, request, page, pageSize, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("{id:Guid}")]
        [ProducesResponseType(typeof(MediaEntryDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<MediaEntryDetailedDto>> GetMediaEntryById(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _readService.GetDetailedByIdAsync(userId, id, ct);

            return this.ToActionResult(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<MediaEntryMinimalDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MediaEntryMinimalDto>>> GetMediaEntries(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _readService.GetMinimalCollectionByOwnerIdAsync(userId, pageNumber, pageSize, ct);

            return this.ToActionResult(result);
        }

        #endregion

        #region Delete Operations - Shared Endpoint

        [HttpDelete("{id:Guid}")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteMediaEntry(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.DeleteAsync(userId, id, ct);

            return this.ToNoContentResult(result);
        }

        #endregion

    }
}
