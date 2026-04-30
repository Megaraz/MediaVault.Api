using System.Security.Claims;
using media_vault_app.Application.DTOs;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class MediaEntriesController : ControllerBase
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
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateMovie(
            [FromBody] MovieEntryCreateDto createDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreatedResult(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

        [HttpPost("tv-series")]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateTvSeries(
            [FromBody] TvSeriesEntryCreateDto createDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreatedResult(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

        [HttpPost("games")]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateGame(
            [FromBody] GameEntryCreateDto createDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreatedResult(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

        [HttpPost("books")]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateBook(
            [FromBody] BookEntryCreateDto createDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreatedResult(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

        [HttpPost("manga")]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateManga(
            [FromBody] MangaEntryCreateDto createDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreatedResult(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

        #endregion

        #region Update Operations - Type-Specific Endpoints

        [HttpPut("movies/{id:Guid}")]
        public async Task<IActionResult> UpdateMovie(
            [FromRoute] Guid id,
            [FromBody] MovieEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContentResult(result);
        }

        [HttpPut("tv-series/{id:Guid}")]
        public async Task<IActionResult> UpdateTvSeries(
            [FromRoute] Guid id,
            [FromBody] TvSeriesEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContentResult(result);
        }

        [HttpPut("games/{id:Guid}")]
        public async Task<IActionResult> UpdateGame(
            [FromRoute] Guid id,
            [FromBody] GameEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContentResult(result);
        }

        [HttpPut("books/{id:Guid}")]
        public async Task<IActionResult> UpdateBook(
            [FromRoute] Guid id,
            [FromBody] BookEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContentResult(result);
        }

        [HttpPut("manga/{id:Guid}")]
        public async Task<IActionResult> UpdateManga(
            [FromRoute] Guid id,
            [FromBody] MangaEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContentResult(result);
        }

        #endregion

        #region Read Operations - Shared Endpoints

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<MediaEntryMinimalDto>>> SearchMediaEntries(
            [FromBody] SearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _readService.SearchMediaEntriesAsync(userId, request, page, pageSize, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<MediaEntryDetailedDto>> GetMediaEntryById(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _readService.GetByIdAsync(userId, id, ct);

            return this.ToActionResult(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MediaEntryDetailedDto>>> GetMediaEntries(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _readService.GetDetailedCollectionByOwnerIdAsync(userId, pageNumber, pageSize, ct);

            return this.ToActionResult(result);
        }

        #endregion

        #region Delete Operations - Shared Endpoint

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteMediaEntry(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.DeleteAsync(userId, id, ct);

            return this.ToNoContentResult(result);
        }

        #endregion

        private bool TryGetCurrentUserId(out Guid userId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out userId))
            {
                return false;
            }
            return true;
        }
    }
}
