using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MediaEntriesController : ControllerBase
    {
        private readonly IMediaEntryReadService _readService;
        private readonly IMediaEntryWriteService _writeService;

        // Hardcoded user ID for testing (no auth yet)
        //private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public MediaEntriesController(IMediaEntryReadService readService, IMediaEntryWriteService writeService)
        {
            _readService = readService;
            _writeService = writeService;
        }

        [HttpPost("{userId:Guid}")]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateMediaEntry(
            [FromRoute] Guid userId,
            [FromBody] MediaEntryCreateDto createDto,
            CancellationToken ct)
        {
            var result = await _writeService.CreateAsync(userId, createDto, ct);

            return this.ToCreated(result, nameof(GetMediaEntryById), value => new { userId = value.UserId, id = value.Id });
        }

        [HttpGet("{userId:Guid}/{id:Guid}")]
        public async Task<ActionResult<MediaEntryDetailedDto>> GetMediaEntryById(
            [FromRoute] Guid userId,
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            var result = await _readService.GetByIdAsync(userId, id, ct);

            return this.ToOk(result);
        }

        [HttpGet("{userId:Guid}")]
        public async Task<ActionResult<IEnumerable<MediaEntryDetailedDto>>> GetMediaEntries(
            [FromRoute] Guid userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _readService.GetDetailedCollectionAsync(userId, pageNumber, pageSize, ct);

            return this.ToOk(result);
        }

        [HttpPut("{userId:Guid}/{id:Guid}")]
        public async Task<IActionResult> UpdateMediaEntry(
            [FromRoute] Guid userId,
            [FromRoute] Guid id,
            [FromBody] MediaEntryUpdateDto updateDto,
            CancellationToken ct)
        {
            var result = await _writeService.UpdateAsync(userId, id, updateDto, ct);

            return this.ToNoContent(result);
        }

        [HttpDelete("{userId:Guid}/{id:Guid}")]
        public async Task<IActionResult> DeleteMediaEntry(
            [FromRoute] Guid userId,
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            var result = await _writeService.DeleteAsync(userId, id, ct);

            return this.ToNoContent(result);
        }
    }
}
