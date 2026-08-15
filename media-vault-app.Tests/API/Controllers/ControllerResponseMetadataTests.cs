using System.Reflection;
using media_vault_app.API.Controllers;
using media_vault_app.API.Diagnostics;
using media_vault_app.API.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
        typeof(TmdbApiController)
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
        Assert.Equal(typeof(MediaVaultAuthorizationProblemDetails), responses[401].Type);
        Assert.Equal(typeof(MediaVaultAuthorizationProblemDetails), responses[403].Type);
        Assert.Equal(typeof(ValidationErrorResponseBody), responses[422].Type);
        Assert.All(
            responses.Where(response => response.Key is not (401 or 403 or 422)),
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
            nameof(AuthController.RegisterUser),
            nameof(AuthController.UpdateUser),
            nameof(MediaEntriesController.UpdateMovie),
            nameof(MediaEntriesController.UpdateTvSeries),
            nameof(MediaEntriesController.UpdateGame),
            nameof(MediaEntriesController.UpdateBook),
            nameof(MediaEntriesController.UpdateManga),
            nameof(MediaEntriesController.DeleteMediaEntry)
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
    public void AuthController_DeclaresTheIntendedAnonymousAndAuthenticatedSplit()
    {
        var anonymousActions = new[]
        {
            typeof(AuthController).GetMethod(nameof(AuthController.RegisterUser))!,
            typeof(AuthController).GetMethod(nameof(AuthController.LoginUser))!
        };
        var authenticatedActions = new[]
        {
            typeof(AuthController).GetMethod(nameof(AuthController.UpdateUser))!,
            typeof(AuthController).GetMethod(nameof(AuthController.GetCurrentUser))!
        };

        Assert.All(
            anonymousActions,
            action => Assert.NotNull(action.GetCustomAttribute<AllowAnonymousAttribute>()));
        Assert.All(
            authenticatedActions,
            action => Assert.NotNull(action.GetCustomAttribute<AuthorizeAttribute>()));
        Assert.Null(typeof(AuthController).Assembly.GetType(
            "media_vault_app.API.Controllers.UsersController"));
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

    [Fact]
    public void TmdbSearchActions_ExposeOnlyTheSupportedPageQueryParameter()
    {
        var searchActions = new[]
        {
            typeof(TmdbApiController).GetMethod(nameof(TmdbApiController.SearchMovies))!,
            typeof(TmdbApiController).GetMethod(nameof(TmdbApiController.SearchTvSeries))!
        };

        Assert.All(searchActions, action =>
        {
            var queryParameters = action
                .GetParameters()
                .Where(parameter => parameter.GetCustomAttribute<FromQueryAttribute>() is not null)
                .Select(parameter => parameter.Name ?? string.Empty)
                .ToArray();

            Assert.Equal(["page"], queryParameters);
        });
    }

    [Fact]
    public void OnlyApprovedEndpointsHaveNamedRateLimitsAndDeclareTheSafeLocal429Contract()
    {
        var registration = new[]
        {
            typeof(AuthController).GetMethod(nameof(AuthController.RegisterUser))!
        };
        var login = new[]
        {
            typeof(AuthController).GetMethod(nameof(AuthController.LoginUser))!
        };
        var rawg = new[]
        {
            typeof(RawgApiController).GetMethod(nameof(RawgApiController.SearchGames))!,
            typeof(RawgApiController).GetMethod(nameof(RawgApiController.GetGameById))!
        };
        var tmdb = new[]
        {
            typeof(TmdbApiController).GetMethod(nameof(TmdbApiController.SearchMovies))!,
            typeof(TmdbApiController).GetMethod(nameof(TmdbApiController.GetMovieById))!,
            typeof(TmdbApiController).GetMethod(nameof(TmdbApiController.SearchTvSeries))!,
            typeof(TmdbApiController).GetMethod(nameof(TmdbApiController.GetTvSeriesById))!
        };
        var googleBooks = new[]
        {
            typeof(GoogleBooksApiController).GetMethod(nameof(GoogleBooksApiController.SearchBooks))!,
            typeof(GoogleBooksApiController).GetMethod(nameof(GoogleBooksApiController.GetBookById))!
        };
        var authenticatedWrites = new[]
        {
            typeof(AuthController).GetMethod(nameof(AuthController.UpdateUser))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.CreateMovie))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.CreateTvSeries))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.CreateGame))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.CreateBook))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.CreateManga))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.UpdateMovie))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.UpdateTvSeries))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.UpdateGame))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.UpdateBook))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.UpdateManga))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.DeleteMediaEntry))!
        };

        Assert.All(registration, action => Assert.Equal(
            MediaVaultRateLimitPolicies.RegistrationByIp,
            action.GetCustomAttribute<EnableRateLimitingAttribute>()?.PolicyName));
        Assert.All(login, action => Assert.Equal(
            MediaVaultRateLimitPolicies.LoginByIp,
            action.GetCustomAttribute<EnableRateLimitingAttribute>()?.PolicyName));
        Assert.All(rawg, action => Assert.Equal(
            MediaVaultRateLimitPolicies.RawgMetadataByUser,
            action.GetCustomAttribute<EnableRateLimitingAttribute>()?.PolicyName));
        Assert.All(tmdb, action => Assert.Equal(
            MediaVaultRateLimitPolicies.TmdbMetadataByUser,
            action.GetCustomAttribute<EnableRateLimitingAttribute>()?.PolicyName));
        Assert.All(googleBooks, action => Assert.Equal(
            MediaVaultRateLimitPolicies.GoogleBooksMetadataByUser,
            action.GetCustomAttribute<EnableRateLimitingAttribute>()?.PolicyName));
        Assert.All(authenticatedWrites, action => Assert.Equal(
            MediaVaultRateLimitPolicies.AuthenticatedWriteByUser,
            action.GetCustomAttribute<EnableRateLimitingAttribute>()?.PolicyName));

        var limitedActions = registration
            .Concat(login)
            .Concat(rawg)
            .Concat(tmdb)
            .Concat(googleBooks)
            .Concat(authenticatedWrites)
            .ToHashSet();
        var allActions = ControllerTypes.SelectMany(type => type.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.All(allActions.Where(action => !limitedActions.Contains(action)), action =>
            Assert.Null(action.GetCustomAttribute<EnableRateLimitingAttribute>()));
        Assert.All(limitedActions, action => Assert.Contains(
            action.GetCustomAttributes<ProducesResponseTypeAttribute>(),
            attribute => attribute.StatusCode == StatusCodes.Status429TooManyRequests &&
                         attribute.Type == typeof(ErrorResponseBody)));
    }

    [Fact]
    public void BodyBoundWriteActions_DeclareTheSharedRequestSizeAnd413Contract()
    {
        var bodyBoundWrites = new[]
        {
            typeof(AuthController).GetMethod(nameof(AuthController.RegisterUser))!,
            typeof(AuthController).GetMethod(nameof(AuthController.UpdateUser))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.CreateMovie))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.CreateTvSeries))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.CreateGame))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.CreateBook))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.CreateManga))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.UpdateMovie))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.UpdateTvSeries))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.UpdateGame))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.UpdateBook))!,
            typeof(MediaEntriesController).GetMethod(nameof(MediaEntriesController.UpdateManga))!
        };

        Assert.All(bodyBoundWrites, action =>
        {
            Assert.NotNull(action.GetCustomAttribute<RequestSizeLimitAttribute>());
            Assert.Contains(
                action.GetCustomAttributes<ProducesResponseTypeAttribute>(),
                attribute => attribute.StatusCode == StatusCodes.Status413PayloadTooLarge &&
                             attribute.Type == typeof(ErrorResponseBody));
        });

        var allActions = ControllerTypes.SelectMany(type => type.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.All(allActions.Where(action => !bodyBoundWrites.Contains(action)), action =>
            Assert.Null(action.GetCustomAttribute<RequestSizeLimitAttribute>()));
    }
}
