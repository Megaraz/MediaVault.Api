using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
//using Microsoft.VisualStudio.Web.CodeGeneration.Design;

namespace media_vault_app.Infrastructure;

//public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
//{
//    public AppDbContext CreateDbContext(string[] args)
//    {
//        var configuration = new ConfigurationBuilder()
//            .SetBasePath(Directory.GetCurrentDirectory())
//            .AddJsonFile("appsettings.json", optional: true)
//            .AddUserSecrets<Program>() // important if using user-secrets
//            .Build();

//        var connectionString = configuration.GetConnectionString("Default");

//        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
//        optionsBuilder.UseSqlite(connectionString);

//        return new AppDbContext(optionsBuilder.Options);
//    }
//}
