using System.Security.Claims;
using media_vault_app.Application.DTOs.ExternalAPIs;
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

        [HttpPost]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateMediaEntry(
            [FromBody] MediaEntryCreateDto createDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreatedResult(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

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

            var result = await _readService.GetDetailedCollectionAsync(userId, pageNumber, pageSize, ct);

            return this.ToActionResult(result);
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> UpdateMediaEntry(
            [FromRoute] Guid id,
            [FromBody] MediaEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContentResult(result);
        }

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
