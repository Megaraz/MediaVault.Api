using media_vault_app.Application.DTOs.External_API_Contracts.Rawg;
using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.DTOs.Rawg;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Enums;
using Microsoft.Extensions.Logging;
using Rasmus.SharedKernel.Pagination;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Results;
using Rasmus.SharedKernel.Validation;

namespace media_vault_app.Application.Services.API
{
    public class RawgApiService : IRawgApiService
    {
        private readonly IRawgApiClient _client;
        private readonly ILogger<RawgApiService> _logger;

        public RawgApiService(IRawgApiClient client, ILogger<RawgApiService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Result<RawgGameDetailedDto>> GetGameByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var idValidationErrorContext = DefineErrorContext(nameof(GetGameByIdAsync), OperationType.Get);

            if (id.IsNotValidMediaVaultId(idValidationErrorContext, out var idError))
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, [idError], nameof(RawgApiService), nameof(GetGameByIdAsync), idValidationErrorContext);
                return Result<RawgGameDetailedDto>.ValidationFailure([idError], MediaVaultResultMessages.ValidationFailure);
            }

            var clientResult = await _client.GetGameByIdAsync(id, cancellationToken);

            return clientResult.Map(ToDetailedDto);

        }
        public async Task<Result<IReadOnlyList<MediaEntryExternalSearchResultDto>>> SearchGamesAsync(
            string search,
            int page = 1,
            int pageSize = 8,
            bool? searchPrecise = null,
            bool? searchExact = null,
            string? ordering = null,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchGamesAsync), OperationType.GetCollection);
            List<ValidationError> errors = new();

            if (search.IsMissingMediaVaultValue(errorContext with { FieldName = nameof(search) }, out var searchError))
            {
                errors.Add(searchError);
            }

            var pagination = PaginationParameters.Normalize(page, pageSize);
            page = pagination.PageNumber;
            pageSize = pagination.PageSize;

            if (errors.Any())
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, errors, nameof(RawgApiService), nameof(SearchGamesAsync), errorContext);
                return Result<IReadOnlyList<MediaEntryExternalSearchResultDto>>.ValidationFailure(errors, "RAWG game search validation failed.");
            }

            var queryParameters = new List<string>
            {
                $"search={Uri.EscapeDataString(search)}",
                $"page={page}",
                $"page_size={pageSize}"
            };

            if (searchPrecise.HasValue)
            {
                queryParameters.Add($"search_precise={searchPrecise.Value.ToString().ToLowerInvariant()}");
            }

            if (searchExact.HasValue)
            {
                queryParameters.Add($"search_exact={searchExact.Value.ToString().ToLowerInvariant()}");
            }

            if (!string.IsNullOrWhiteSpace(ordering))
            {
                queryParameters.Add($"ordering={Uri.EscapeDataString(ordering)}");
            }

            var clientResult = await _client.SearchGamesAsync(queryParameters, cancellationToken);

            return clientResult.Map(searchResponse => MapToGameSearchResult(searchResponse.Results));

        }



        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                operation: operation,
                entityName: entityName ?? "Rawg Game",
                fieldName: fieldName);
        }

        private static IReadOnlyList<MediaEntryExternalSearchResultDto> MapToGameSearchResult(IReadOnlyList<RawgGameSearchResult>? rawgGames)
        {
            return rawgGames?.Select(MapToGameSearchResult).ToArray() ?? Array.Empty<MediaEntryExternalSearchResultDto>();
        }

        private static RawgGameDetailedDto ToDetailedDto(RawgGameDetailedResponse rawgGame)
        {
            var rawgPlatforms = rawgGame.Platforms?
                .Select(p => p.Platform1?.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? Array.Empty<string>();

            var rawgRequirements = MapRequirements(rawgGame.Platforms);

            return new RawgGameDetailedDto(
                RawgId: rawgGame.Id,
                RawgSlug: rawgGame.Slug,
                RawgName: rawgGame.Name,
                RawgDescription: rawgGame.Description,
                RawgMetacritic: rawgGame.Metacritic,
                RawgReleased: rawgGame.Released,
                RawgBackgroundImage: rawgGame.BackgroundImage,
                RawgWebsite: rawgGame.Website,
                RawgPlatforms: rawgPlatforms,
                RawgRequirements: rawgRequirements
            );
        }

        private static GamePcRequirementsDto? MapRequirements(IEnumerable<Platform>? platforms)
        {
            var selectedPlatform = platforms?
                .FirstOrDefault(platform =>
                    string.Equals(platform.Platform1?.Name, "PC", StringComparison.OrdinalIgnoreCase)
                    && HasRequirements(platform.Requirements))
                ?? platforms?.FirstOrDefault(platform => HasRequirements(platform.Requirements));

            if (selectedPlatform?.Requirements is null)
            {
                return null;
            }

            return new GamePcRequirementsDto(
                Minimum: NullIfWhiteSpace(selectedPlatform.Requirements.Minimum),
                Recommended: NullIfWhiteSpace(selectedPlatform.Requirements.Recommended),
                High: null,
                VeryHigh: null,
                Ultra: null);
        }

        private static bool HasRequirements(Requirements? requirements)
        {
            return !string.IsNullOrWhiteSpace(requirements?.Minimum)
                || !string.IsNullOrWhiteSpace(requirements?.Recommended);
        }

        private static string? NullIfWhiteSpace(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static MediaEntryExternalSearchResultDto MapToGameSearchResult(RawgGameSearchResult rawgGame)
        {
            return new MediaEntryExternalSearchResultDto(
                rawgGame.Id.ToString(),
                rawgGame.Name ?? string.Empty,
                rawgGame.BackgroundImage,
                MediaType.Game);
        }

    }
}
