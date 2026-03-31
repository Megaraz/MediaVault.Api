using media_vault_app.Application.DTOs.Tmdb.Movie;
using media_vault_app.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers
{

    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class TmdbMovieApiController : ControllerBase
    {
        private readonly ITmdbMovieApiService _tmdbMovieApiService;

        public TmdbMovieApiController(ITmdbMovieApiService tmdbMovieApiService)
        {
            _tmdbMovieApiService = tmdbMovieApiService;
        }

        [HttpPost("search")]
        public async Task<ActionResult<IReadOnlyList<MovieSearchResultDto>>> SearchMovies(
            [FromBody] MovieSearchRequestDto request,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? ordering = null)
        {
            var result = await _tmdbMovieApiService.SearchMoviesAsync(request.Query, page, pageSize, ordering, ct);
            return this.ToActionResult(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MovieSearchResultDto>> GetMovieById(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _tmdbMovieApiService.GetMovieByIdAsync(id, ct);
            return this.ToActionResult(result);
        }

    }
}
