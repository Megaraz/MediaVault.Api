using System.Net.Http.Headers;
using System.Text;
using media_vault_app.API.Security;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Interfaces.Mappers;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Interfaces.Validators;
using media_vault_app.Application.Mappers.MediaEntry;
using media_vault_app.Application.Mappers.User;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;

namespace media_vault_app.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            var connectionString = builder.Configuration
                .GetConnectionString("Default") ??
                throw new InvalidOperationException("Connection string 'Default' not found.");
            //var connectionString = "Data Source=mediavault.db";

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));
            //options.UseSqlServer(connectionString));


            #region Rawg API

            builder.Services
                .AddOptions<RawgApiOptions>()
                .BindConfiguration(RawgApiOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddHttpClient<IRawgApiClient, RawgApiClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<RawgApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            #endregion

            #region TMDB API

            builder.Services
                .AddOptions<TmdbApiOptions>()
                .BindConfiguration(TmdbApiOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddHttpClient<ITmdbApiClient, TmdbApiClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<TmdbApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.ApiAccessToken);
            });

            #endregion


            #region Google Books API

            builder.Services
                .AddOptions<GoogleBooksApiOptions>()
                .BindConfiguration(GoogleBooksApiOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddHttpClient<IGoogleBooksApiClient, GoogleBooksApiClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<GoogleBooksApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            #endregion



            #region Mappers

            builder.Services.AddScoped<IMediaEntryEntityMapper, MediaEntryEntityMapper>();
            builder.Services.AddScoped<IMediaEntryDtoMapper, MediaEntryDtoMapper>();

            builder.Services.AddScoped<IUserEntityMapper, UserEntityMapper>();
            builder.Services.AddScoped<IUserDtoMapper, UserDtoMapper>();

            #endregion

            #region Validators

            builder.Services.AddScoped<IMediaEntryDtoValidator, MediaEntryDtoValidator>();

            builder.Services.AddScoped<IUserDtoValidator, UserDtoValidator>();
            #endregion


            #region Repositories
            builder.Services.AddScoped<IUserRepo, UserRepo>();

            builder.Services.AddScoped<IMediaEntryRepo, MediaEntryRepo>();

            builder.Services.AddScoped<IRepo<TvSeriesEntry, Guid>, RepoBase<TvSeriesEntry, Guid>>();
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

            // Production failure events use standard logging. The legacy sink remains registered
            // only for ErrorLogCleanupService until the file-specific surface is removed in #110.
            builder.Services.AddSingleton<ErrorEventPolicy>();
            builder.Services.AddSingleton(
                new ErrorDiagnosticsOptions(builder.Environment.IsDevelopment()));
            builder.Services.AddSingleton(typeof(ErrorEventLogger<>), typeof(ErrorEventLogger<>));

            builder.Services.AddSingleton<IErrorLogger, ErrorLogger>(sp =>
            {
                var configuration = new ErrorLoggerConfiguration
                {
                    BasePath = Path.Combine(AppContext.BaseDirectory, "Logs"),
                    Filename = "errors.log.ndjson"
                };

                return new ErrorLogger(configuration);
            });

            builder.Services.AddHostedService<ErrorLogCleanupService>();

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:3000",
                            "http://localhost:5173",
                            "http://localhost:8081",
                            "http://192.168.0.12:8081")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            #region JWT Auth

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
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }
                    };
                });
            #endregion

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
