using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.ExternalAPIs;
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

        public async Task<Result<SearchResultDto>> GetGameByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var idValidationErrorContext = DefineErrorContext(nameof(GetGameByIdAsync), OperationType.Get);

            if (!id.IsValidId(idValidationErrorContext, out var idError))
            {
                return Result<SearchResultDto>.ValidationFailure([idError]);
            }

            var result = await _client.GetGameAsync(id, cancellationToken);

            return result.Map(MapToGameSearchResult);

        }
        public async Task<Result<IReadOnlyList<SearchResultDto>>> SearchGamesAsync(
            string search,
            int page = 1,
            int pageSize = 10,
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
                return Result<IReadOnlyList<SearchResultDto>>.ValidationFailure(errors, "RAWG game search validation failed.");
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

        private static IReadOnlyList<SearchResultDto> MapToGameSearchResult(IReadOnlyList<RawgGameResponse>? rawgGames)
        {
            return rawgGames?.Select(MapToGameSearchResult).ToArray() ?? Array.Empty<SearchResultDto>();
        }

        private static SearchResultDto MapToGameSearchResult(RawgGameResponse rawgGame)
        {
            return new SearchResultDto(
                rawgGame.Id.ToString(),
                rawgGame.Name ?? string.Empty,
                rawgGame.BackgroundImage,
                MediaEntryType.GameEntry);
        }

    }
}
