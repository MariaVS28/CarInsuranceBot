using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace CarInsuranceBot.DAL
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}

            //var a = Directory.GetCurrentDirectory();

            DotNetEnv.Env.Load(@"..\.env");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var azureDbPassword = Environment.GetEnvironmentVariable("AZURE_DB_PASSWORD");
            var connectionString = configuration.GetConnectionString("AzureDb");
            connectionString = connectionString!.Replace("[Password]", azureDbPassword);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
