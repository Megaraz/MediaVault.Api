using media_vault_app.Application.DTOs.GoogleBooks;
using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class GoogleBooksApiController : ControllerBase
    {
        private readonly IGoogleBooksApiService _googleBooksApiService;

        public GoogleBooksApiController(IGoogleBooksApiService googleBooksApiService)
        {
            _googleBooksApiService = googleBooksApiService;
        }

        [HttpPost("search")]
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
        public async Task<ActionResult<GoogleBooksDetailedDto>> GetBookById(
            [FromRoute] string volumeId,
            CancellationToken ct)
        {
            var result = await _googleBooksApiService.GetBookByIdAsync(volumeId, ct);
            return this.ToActionResult(result);
        }
    }
}
