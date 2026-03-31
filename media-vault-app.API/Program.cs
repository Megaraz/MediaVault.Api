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
using media_vault_app.Infrastructure.API_Clients;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Services.API;
using System.Net.Http.Headers;

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

            var rawgConnectionString = builder.Configuration
                .GetConnectionString("Rawg") ??
                throw new InvalidOperationException("Connection string 'Rawg' not found.");


            var rawgApiKey = builder.Configuration["APIKeys:Rawg"] ??
                throw new InvalidOperationException("Rawg API key not found in configuration.");

            var rawgApiOptions = new RawgApiOptions(rawgConnectionString, rawgApiKey);

            builder.Services.AddSingleton(rawgApiOptions);

            builder.Services.AddHttpClient<IRawgApiClient, RawgApiClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<RawgApiOptions>();
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            #region TMDB API
            var tmdbConnectionString = builder.Configuration
                .GetConnectionString("tmdb") ??
                throw new InvalidOperationException("Connection string 'tmdb' not found.");

            var tmdbAccessToken = builder.Configuration["AccesTokens:tmdb"] ??
                throw new InvalidOperationException("tmdb Access Token not found in configuration.");

            builder.Services.AddHttpClient<ITmdbApiClient, TmdbApiClient>((serviceProvider, client) =>
            {
                client.BaseAddress = new Uri(tmdbConnectionString);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tmdbAccessToken);
            });

            #endregion


            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();

            builder.Services.AddScoped<IGenericRepo<User, Guid>, UserRepo>();
            builder.Services.AddScoped<IUserRepo, UserRepo>();

            builder.Services.AddScoped<IGenericRepo<MediaEntry, Guid>, MediaEntryRepo>();
            builder.Services.AddScoped<IMediaEntryRepo, MediaEntryRepo>();

            builder.Services.AddScoped<IMediaEntryReadService, MediaEntryReadService>();
            builder.Services.AddScoped<IMediaEntryWriteService, MediaEntryWriteService>();

            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IUserReadService, UserReadService>();
            builder.Services.AddScoped<IUserWriteService, UserWriteService>();

            builder.Services.AddScoped<IRawgApiService, RawgApiService>();

            builder.Services.AddScoped<ITmdbApiService, TmdbApiService>();


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
