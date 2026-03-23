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
        private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public MediaEntriesController(IMediaEntryReadService readService, IMediaEntryWriteService writeService)
        {
            _readService = readService;
            _writeService = writeService;
        }

        [HttpPost]
        public async Task<ActionResult<MediaEntryDetailedDto>> CreateMediaEntry([FromBody] MediaEntryCreateDto createDto, CancellationToken ct)
        {
            var result = await _writeService.CreateAsync(TestUserId, createDto, ct);

            return this.ToCreated(result, nameof(GetMediaEntryById), value => new { id = value.Id });
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<MediaEntryDetailedDto>> GetMediaEntryById(Guid id, CancellationToken ct)
        {
            var result = await _readService.GetByIdAsync(TestUserId, id, ct);

            return this.ToOk(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MediaEntryDetailedDto>>> GetMediaEntries(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _readService.GetDetailedCollectionAsync(TestUserId, pageNumber, pageSize, ct);

            return this.ToOk(result);
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> UpdateMediaEntry(Guid id, [FromBody] MediaEntryUpdateDto updateDto, CancellationToken ct)
        {
            var result = await _writeService.UpdateAsync(TestUserId, id, updateDto, ct);

            return this.ToNoContent(result);
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteMediaEntry(Guid id, CancellationToken ct)
        {
            var result = await _writeService.DeleteAsync(TestUserId, id, ct);

            return this.ToNoContent(result);
        }
    }
}
