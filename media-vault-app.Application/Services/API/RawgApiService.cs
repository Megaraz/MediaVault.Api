using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.Rawg;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Interfaces.Services;
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

        public async Task<Result<GameSearchResultDto>> GetGameByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var idValidationErrorContext = DefineErrorContext(nameof(GetGameByIdAsync), OperationType.Get);

            if (!id.IsValidId(idValidationErrorContext, out var idError))
            {
                idValidationErrorContext.DescriptionSuffix = $"Invalid game ID: {id}.";
                return Result<GameSearchResultDto>.ValidationFailure([idError], idValidationErrorContext.DescriptionSuffix);
            }

            var result = await _client.GetGameAsync(id, cancellationToken);

            return result.Map(MapToGameSearchResult);

        }
        public async Task<Result<IReadOnlyList<GameSearchResultDto>>> SearchGamesAsync(
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

            errorContext.FieldName = nameof(search);
            if (search.IsNullOrWhiteSpace(errorContext, out var searchError))
            {
                errors.Add(searchError);
            }

            if (page < 1)
            {
                errorContext.DescriptionSuffix = "Page number must be greater than 0.";
                errorContext.FieldName = nameof(page);

                var pageError = ValidationError.OutOfRange(errorContext, "Greater than 0");
                errors.Add(pageError);
            }

            if (pageSize < 1)
            {
                errorContext.DescriptionSuffix = "Page size must be greater than 0.";
                errorContext.FieldName = nameof(pageSize);

                var pageSizeError = ValidationError.OutOfRange(errorContext, "Greater than 0");
                errors.Add(pageSizeError);
            }

            if (errors.Any())
            {
                return Result<IReadOnlyList<GameSearchResultDto>>.ValidationFailure(errors, "RAWG game search validation failed.");
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
                layer: "Application",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: entityName ?? "Rawg Game",
                fieldName: fieldName);
        }

        private static IReadOnlyList<GameSearchResultDto> MapToGameSearchResult(IReadOnlyList<RawgGameResponse>? rawgGames)
        {
            return rawgGames?.Select(MapToGameSearchResult).ToArray() ?? Array.Empty<GameSearchResultDto>();
        }

        private static GameSearchResultDto MapToGameSearchResult(RawgGameResponse rawgGame)
        {
            return new GameSearchResultDto(
                rawgGame.Id,
                rawgGame.Name ?? string.Empty,
                rawgGame.BackgroundImage,
                rawgGame.Slug ?? string.Empty);
        }

    }
}
