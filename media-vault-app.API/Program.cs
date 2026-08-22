using System.Net.Http.Headers;
using System.Text;
using media_vault_app.API.Configuration;
using media_vault_app.API.Diagnostics;
using media_vault_app.API.Health;
using media_vault_app.API.Observability;
using media_vault_app.API.RateLimiting;
using media_vault_app.API.Security;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Interfaces.Validators;
using media_vault_app.Application.Services;
using media_vault_app.Application.Services.API;
using media_vault_app.Application.Services.Auth;
using media_vault_app.Application.Services.MediaEntry;
using media_vault_app.Application.Services.User;
using media_vault_app.Application.Validators.MediaEntry;
using media_vault_app.Application.Validators.User;
using media_vault_app.Domain.Entities;
using media_vault_app.Infrastructure;
using media_vault_app.Infrastructure.API.Clients;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Infrastructure.Repos;
using media_vault_app.Infrastructure.Timestamps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace media_vault_app.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Keep the supported application providers deterministic across hosts.
            // In particular, the Windows Event Log provider can throw when the
            // process cannot create or access its source, which must not turn a
            // diagnostic warning into a failed request.
            builder.Logging.ClearProviders();
            builder.Logging.AddConfiguration(
                builder.Configuration.GetSection("Logging"));
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            builder.AddMediaVaultOpenTelemetry();

            // Add services to the container.

            var requestBudgetOptions = builder.Configuration
                .GetSection(RequestBudgetOptions.SectionName)
                .Get<RequestBudgetOptions>() ?? new RequestBudgetOptions();
            builder.Services.AddProviderResilienceOptions(
                builder.Configuration,
                requestBudgetOptions.ExternalMetadataMilliseconds);

            var connectionString = builder.Configuration
                .GetConnectionString("Default") ??
                throw new InvalidOperationException("Connection string 'Default' not found.");

            if (builder.Environment.IsProduction())
                SqliteConnectionStringPolicy.ValidateForProduction(connectionString);

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services
                .AddHealthChecks()
                .AddCheck<DatabaseReadinessHealthCheck>(
                    "database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["ready"]);

            #region Rawg API

            builder.Services
                .AddOptions<RawgApiOptions>()
                .BindConfiguration(RawgApiOptions.SectionName)
                .ValidateDataAnnotations()
                .Validate(
                    options => IsAbsoluteHttpsUrl(options.BaseUrl),
                    "BaseUrl must be an absolute HTTPS URL.")
                .ValidateOnStart();

            builder.Services.AddHttpClient<IRawgApiClient, RawgApiClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<RawgApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = Timeout.InfiniteTimeSpan;
            }).AddMediaVaultProviderResilience(ProviderResilienceNames.Rawg);

            #endregion

            #region TMDB API

            builder.Services
                .AddOptions<TmdbApiOptions>()
                .BindConfiguration(TmdbApiOptions.SectionName)
                .ValidateDataAnnotations()
                .Validate(
                    options => IsAbsoluteHttpsUrl(options.BaseUrl),
                    "BaseUrl must be an absolute HTTPS URL.")
                .ValidateOnStart();

            builder.Services.AddHttpClient<ITmdbApiClient, TmdbApiClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<TmdbApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.ApiAccessToken);
                client.Timeout = Timeout.InfiniteTimeSpan;
            }).AddMediaVaultProviderResilience(ProviderResilienceNames.Tmdb);

            #endregion


            #region Google Books API

            builder.Services
                .AddOptions<GoogleBooksApiOptions>()
                .BindConfiguration(GoogleBooksApiOptions.SectionName)
                .ValidateDataAnnotations()
                .Validate(
                    options => IsAbsoluteHttpsUrl(options.BaseUrl),
                    "BaseUrl must be an absolute HTTPS URL.")
                .ValidateOnStart();

            builder.Services.AddHttpClient<IGoogleBooksApiClient, GoogleBooksApiClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<GoogleBooksApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = Timeout.InfiniteTimeSpan;
            }).AddMediaVaultProviderResilience(ProviderResilienceNames.GoogleBooks);

            #endregion



            #region Validators

            builder.Services.AddScoped<IMediaEntryDtoValidator, MediaEntryDtoValidator>();

            builder.Services.AddScoped<IUserDtoValidator, UserDtoValidator>();
            #endregion


            #region Repositories
            builder.Services.AddScoped<IUserRepo, UserRepo>();

            builder.Services.AddScoped<IMediaEntryRepo, MediaEntryRepo>();

            #endregion

            #region Services

            builder.Services.AddScoped<IMediaEntryReadService, MediaEntryReadService>();
            builder.Services.AddScoped<IMediaEntryWriteService, MediaEntryWriteService>();

            builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();

            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IUserReadService, UserReadService>();
            builder.Services.AddScoped<IUserWriteService, UserWriteService>();

            builder.Services.AddScoped<IRawgApiService, RawgApiService>();

            builder.Services.AddScoped<ITmdbApiService, TmdbApiService>();

            builder.Services.AddScoped<IGoogleBooksApiService, GoogleBooksApiService>();
            #endregion

            // Production failure events use standard logging.
            builder.Services.AddSingleton<ErrorEventPolicy>();
            builder.Services.AddSingleton<ServerTimestampPolicy>();
            builder.Services.AddSingleton(
                new ErrorDiagnosticsOptions(builder.Environment.IsDevelopment()));
            builder.Services.AddSingleton(typeof(ErrorEventLogger<>), typeof(ErrorEventLogger<>));

            builder.Services.AddControllers();

            builder.Services.AddSingleton<IProblemDetailsWriter, MediaVaultProblemDetailsWriter>();
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<MediaVaultExceptionHandler>();

            builder.Services
                .AddOptions<RequestBudgetOptions>()
                .BindConfiguration(RequestBudgetOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddRequestTimeouts(options =>
            {
                options.AddPolicy(
                    MediaVaultRequestTimeoutPolicies.Authentication,
                    new RequestTimeoutPolicy
                    {
                        Timeout = TimeSpan.FromMilliseconds(requestBudgetOptions.AuthenticationMilliseconds),
                        TimeoutStatusCode = StatusCodes.Status504GatewayTimeout,
                        WriteTimeoutResponse = MediaVaultRequestTimeoutResponse.WriteAsync
                    });
                options.AddPolicy(
                    MediaVaultRequestTimeoutPolicies.ExternalMetadata,
                    new RequestTimeoutPolicy
                    {
                        Timeout = TimeSpan.FromMilliseconds(requestBudgetOptions.ExternalMetadataMilliseconds),
                        TimeoutStatusCode = StatusCodes.Status504GatewayTimeout,
                        WriteTimeoutResponse = MediaVaultRequestTimeoutResponse.WriteAsync
                    });
            });

            builder.Services.AddOpenApi();

            builder.Services
                .AddOptions<CorsOptions>()
                .BindConfiguration(CorsOptions.SectionName)
                .ValidateDataAnnotations()
                .Validate(
                    options => !builder.Environment.IsProduction() ||
                        options.AllowedOrigins is { Length: > 0 },
                    "Cors:AllowedOrigins must contain at least one origin in Production.")
                .ValidateOnStart();

            var corsOptions = builder.Configuration
                .GetSection(CorsOptions.SectionName)
                .Get<CorsOptions>() ?? new CorsOptions();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsOptions.PolicyName, policy =>
                {
                    policy
                        .WithOrigins(corsOptions.AllowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            #region JWT Auth

            builder.Services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

            builder.Services
                .AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            builder.Services
                .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
                {
                    var jwt = jwtOptions.Value;

                    bearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwt.SecretKey))
                    };

                    bearerOptions.Events = new JwtBearerEvents
                    {
                        OnChallenge = MediaVaultAuthorizationResponse.WriteChallengeAsync,
                        OnForbidden = MediaVaultAuthorizationResponse.WriteForbiddenAsync
                    };
                });
            #endregion

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
            builder.Services.AddMediaVaultRateLimiting(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi().AllowAnonymous();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseExceptionHandler(new ExceptionHandlerOptions
            {
                // MediaVault's handler owns event 3000. Keep the .NET 10 framework
                // diagnostics suppressed for handled exceptions to avoid duplicates.
                SuppressDiagnosticsCallback = _ => true
            });

            app.UseCors(CorsOptions.PolicyName);

            app.UseRouting();
            app.UseAuthentication();
            app.UseRateLimiter();
            app.UseRequestTimeouts();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks(
                "/health/ready",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready"),
                    ResponseWriter = HealthCheckResponseWriter.WriteAsync
                })
                .AllowAnonymous();

            app.Run();
        }

        private static bool IsAbsoluteHttpsUrl(string? value) =>
            !string.IsNullOrWhiteSpace(value) &&
            Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
            uri is not null &&
            string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(uri.Host);
    }
}
