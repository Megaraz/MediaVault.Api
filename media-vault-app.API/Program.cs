using media_vault_app.API.Security;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Services.MediaEntry;
using media_vault_app.Application.Services.User;
using media_vault_app.Domain.Entities;
using media_vault_app.Infrastructure;
using media_vault_app.Infrastructure.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Rasmus.SharedKernel.Interfaces;
using media_vault_app.Application.Services.Auth;
using media_vault_app.Application.Services.API;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using media_vault_app.Application.Mappers.MediaEntry;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using media_vault_app.Application.Mappers.User;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.DTOs.User.Request;
using Rasmus.SharedKernel.Interfaces.Validators;
using media_vault_app.Application.Validators.User;
using media_vault_app.Application.Validators.MediaEntry;
using System.Text.Json.Serialization;
using media_vault_app.Infrastructure.API.Clients;
using media_vault_app.Application.Interfaces.Clients;

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

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();

            #region Mappers

            builder.Services.AddScoped<
                IMapEntityToDto<MediaEntry, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto>,
                MediaEntryEntityMapper>();

            builder.Services.AddScoped<
                IMapDtoToEntity<MediaEntry, MediaEntryDetailedDto, MediaEntryCreateDto, MediaEntryUpdateDto, Guid>,
                MediaEntryDtoMapper>();

            builder.Services.AddScoped<
                IMapEntityToDto<User, Guid, UserDetailedDto, UserMinimalDto>,
                UserEntityMapper>();

            builder.Services.AddScoped<
                IMapDtoToEntity<User, UserDetailedDto, UserRegisterDto, UserUpdateDto, Guid>,
                UserDtoMapper>();


            #endregion

            #region Validators

            builder.Services.AddScoped<
                IDtoValidator<Guid, MediaEntryCreateDto, MediaEntryUpdateDto>,
                MediaEntryDtoValidator>();

            builder.Services.AddScoped<
                IDtoValidator<Guid, UserRegisterDto, UserUpdateDto>,
                UserDtoValidator>();

            #endregion


            builder.Services.AddScoped<IUserRepo, UserRepo>();

            builder.Services.AddScoped<IMediaEntryRepo, MediaEntryRepo>();

            builder.Services.AddScoped<IMediaEntryReadService, MediaEntryReadService>();
            builder.Services.AddScoped<IMediaEntryWriteService, MediaEntryWriteService>();

            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IUserReadService, UserReadService>();
            builder.Services.AddScoped<IUserWriteService, UserWriteService>();

            builder.Services.AddScoped<IRawgApiService, RawgApiService>();

            builder.Services.AddScoped<ITmdbApiService, TmdbApiService>();

            builder.Services.AddScoped<IGoogleBooksApiService, GoogleBooksApiService>();


            builder.Services.AddControllers();

            builder.Services.AddOpenApi();

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "media-vault-auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;

                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    };
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.MapStaticAssets();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
