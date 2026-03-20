using media_vault_app.API.Security;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Services.MediaEntry;
using media_vault_app.Application.Services.User;
using media_vault_app.Infrastructure;
using media_vault_app.Infrastructure.Repos;
using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.Interfaces;

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

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();

            builder.Services.AddScoped<IMediaEntryReadService, MediaEntryReadService>();
            builder.Services.AddScoped<IMediaEntryWriteService, MediaEntryWriteService>();

            builder.Services.AddScoped<IUserRepo, UserRepo>();
            builder.Services.AddScoped<IUserReadService, UserReadService>();
            builder.Services.AddScoped<IUserWriteService, UserWriteService>();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.MapStaticAssets();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
