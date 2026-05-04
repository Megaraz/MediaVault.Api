using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs;
using media_vault_app.Application.DTOs.External_API_Contracts.Rawg;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.DTOs.Rawg;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services.API
{
    public class RawgApiService : IRawgApiService
    {
        private readonly IRawgApiClient _client;

        public RawgApiService(IRawgApiClient client)
        {
            _client = client;
        }

        public async Task<Result<RawgGameDetailedDto>> GetGameByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var idValidationErrorContext = DefineErrorContext(nameof(GetGameByIdAsync), OperationType.Get);

            if (!id.IsValidId(idValidationErrorContext, out var idError))
            {
                return Result<RawgGameDetailedDto>.ValidationFailure([idError]);
            }

            var result = await _client.GetGameByIdAsync(id, cancellationToken);

            return result.Map(ToDetailedDto);

        }
        public async Task<Result<IReadOnlyList<MediaEntrySearchResultDto>>> SearchGamesAsync(
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

            if (search.IsNullOrWhiteSpace(errorContext with { FieldName = nameof(search) }, out var searchError))
            {
                errors.Add(searchError);
            }

            Validator.ValidateAndAdjustPaginationParameters(ref page, ref pageSize);

            if (errors.Any())
            {
                return Result<IReadOnlyList<MediaEntrySearchResultDto>>.ValidationFailure(errors, "RAWG game search validation failed.");
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

            var result = await _client.SearchGamesAsync(queryParameters, cancellationToken);

            return result.Map(searchResponse => MapToGameSearchResult(searchResponse.Results));

        }



        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                Layer: "Application",
                ServiceName: GetType().Name,
                MethodName: methodName,
                Operation: operation,
                EntityName: entityName ?? "Rawg Game",
                FieldName: fieldName);
        }

        private static IReadOnlyList<MediaEntrySearchResultDto> MapToGameSearchResult(IReadOnlyList<RawgGameSearchResult>? rawgGames)
        {
            return rawgGames?.Select(MapToGameSearchResult).ToArray() ?? Array.Empty<MediaEntrySearchResultDto>();
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

        private static MediaEntrySearchResultDto MapToGameSearchResult(RawgGameSearchResult rawgGame)
        {
            return new MediaEntrySearchResultDto(
                rawgGame.Id.ToString(),
                rawgGame.Name ?? string.Empty,
                rawgGame.BackgroundImage,
                MediaType.Game);
        }

    }
}
