using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs;
using media_vault_app.Application.DTOs.External_API_Contracts.Rawg;
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
            return new RawgGameDetailedDto(
                RawgId: rawgGame.Id,
                RawgSlug: rawgGame.Slug,
                RawgName: rawgGame.Name,
                RawgDescription: rawgGame.Description,
                RawgMetacritic: rawgGame.Metacritic,
                RawgReleased: rawgGame.Released,
                RawgBackgroundImage: rawgGame.BackgroundImage,
                RawgWebsite: rawgGame.Website,
                RawgPlatforms: rawgGame.Platforms?.Select(p => new RawgPlatformDto(
                    Platform1: new RawgPlatform1Dto(
                        RawgPlatform1Id: p?.Platform1?.Id ?? 0,
                        RawgPlatform1Name: p?.Platform1?.Name,
                        RawgPlatform1Slug: p?.Platform1?.Slug
                    ),
                    RawgReleasedAt: p?.ReleasedAt,
                    RawgRequirements: p?.Requirements != null ? new RawgRequirementsDto(
                        RawgRequirementsMinimum: p.Requirements.Minimum,
                        RawgRequirementsRecommended: p.Requirements.Recommended
                    ) : null
                )).ToArray()
                );
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
