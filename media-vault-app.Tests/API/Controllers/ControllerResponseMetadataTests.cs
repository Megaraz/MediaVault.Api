using System.Reflection;
using media_vault_app.API.Controllers;
using media_vault_app.API.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Timeouts;

namespace media_vault_app.Tests.API.Controllers;

public sealed class ControllerResponseMetadataTests
{
    private static readonly Type[] ControllerTypes =
    [
        typeof(AuthController),
        typeof(GoogleBooksApiController),
        typeof(MediaEntriesController),
        typeof(RawgApiController),
        typeof(TmdbApiController),
        typeof(UsersController)
    ];

    [Fact]
    public void SharedResultMetadata_DescribesApprovedErrorBodiesAndStatuses()
    {
        var responses = typeof(MediaVaultControllerBase)
            .GetCustomAttributes<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        Assert.Equal(
            [400, 401, 403, 404, 409, 422, 429, 500, 502, 503],
            responses.Keys.Order());
        Assert.Equal(typeof(ValidationErrorResponseBody), responses[422].Type);
        Assert.All(
            responses.Where(response => response.Key != 422),
            response => Assert.Equal(typeof(ErrorResponseBody), response.Value.Type));
    }

    [Fact]
    public void EveryControllerAction_DeclaresItsRuntimeSuccessStatus()
    {
        var createdActions = new HashSet<string>
        {
            nameof(MediaEntriesController.CreateMovie),
            nameof(MediaEntriesController.CreateTvSeries),
            nameof(MediaEntriesController.CreateGame),
            nameof(MediaEntriesController.CreateBook),
            nameof(MediaEntriesController.CreateManga)
        };
        var noContentActions = new HashSet<string>
        {
            nameof(AuthController.UpdateUser),
            nameof(MediaEntriesController.UpdateMovie),
            nameof(MediaEntriesController.UpdateTvSeries),
            nameof(MediaEntriesController.UpdateGame),
            nameof(MediaEntriesController.UpdateBook),
            nameof(MediaEntriesController.UpdateManga),
            nameof(MediaEntriesController.DeleteMediaEntry),
            nameof(UsersController.DeleteUser)
        };

        foreach (var controllerType in ControllerTypes)
        {
            Assert.True(controllerType.IsSubclassOf(typeof(MediaVaultControllerBase)));

            var actions = controllerType.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var action in actions)
            {
                var expectedStatus = createdActions.Contains(action.Name)
                    ? StatusCodes.Status201Created
                    : noContentActions.Contains(action.Name)
                        ? StatusCodes.Status204NoContent
                        : StatusCodes.Status200OK;
                var successResponses = action
                    .GetCustomAttributes<ProducesResponseTypeAttribute>()
                    .Where(attribute => attribute.StatusCode is >= 200 and < 300)
                    .ToArray();

                var response = Assert.Single(successResponses);
                Assert.Equal(expectedStatus, response.StatusCode);
            }
        }
    }

    [Fact]
    public void OnlyApprovedEndpointsHaveNamedRequestBudgetsAndDeclareTheSafeTimeoutContract()
    {
        var authentication = new[]
        {
            typeof(AuthController).GetMethod(nameof(AuthController.RegisterUser))!,
            typeof(AuthController).GetMethod(nameof(AuthController.LoginUser))!
        };
        var externalMetadata = new[]
        {
            typeof(RawgApiController).GetMethod(nameof(RawgApiController.SearchGames))!,
            typeof(RawgApiController).GetMethod(nameof(RawgApiController.GetGameById))!,
            typeof(TmdbApiController).GetMethod(nameof(TmdbApiController.SearchMovies))!,
            typeof(TmdbApiController).GetMethod(nameof(TmdbApiController.GetMovieById))!,
            typeof(TmdbApiController).GetMethod(nameof(TmdbApiController.SearchTvSeries))!,
            typeof(TmdbApiController).GetMethod(nameof(TmdbApiController.GetTvSeriesById))!,
            typeof(GoogleBooksApiController).GetMethod(nameof(GoogleBooksApiController.SearchBooks))!,
            typeof(GoogleBooksApiController).GetMethod(nameof(GoogleBooksApiController.GetBookById))!
        };

        Assert.All(authentication, action => Assert.Equal(
            MediaVaultRequestTimeoutPolicies.Authentication,
            action.GetCustomAttribute<RequestTimeoutAttribute>()?.PolicyName));
        Assert.All(externalMetadata, action => Assert.Equal(
            MediaVaultRequestTimeoutPolicies.ExternalMetadata,
            action.GetCustomAttribute<RequestTimeoutAttribute>()?.PolicyName));

        var budgetedActions = authentication.Concat(externalMetadata).ToHashSet();
        var allActions = ControllerTypes.SelectMany(type => type.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.All(allActions.Where(action => !budgetedActions.Contains(action)), action =>
            Assert.Null(action.GetCustomAttribute<RequestTimeoutAttribute>()));
        Assert.All(budgetedActions, action => Assert.Contains(
            action.GetCustomAttributes<ProducesResponseTypeAttribute>(),
            attribute => attribute.StatusCode == StatusCodes.Status504GatewayTimeout &&
                         attribute.Type == typeof(ErrorResponseBody)));
    }
}
