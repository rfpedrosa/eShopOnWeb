using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.eShopWeb.IntegrationTests.Repositories
{
    // Based on https://github.com/ardalis/CleanArchitecture/blob/master/tests/CleanArchitecture.IntegrationTests/Data/BaseEfRepoTestFixture.cs
    // and https://www.davepaquette.com/archive/2016/11/27/integration-testing-with-entity-framework-core-and-sql-server.aspx
    public abstract class BaseEfRepoTestFixture : IDisposable
    {
        private static readonly IConfigurationRoot Configuration;

        private readonly CatalogContext _catalogContext;

        static BaseEfRepoTestFixture()
        {
            Configuration = BuildConfig();
        }

        protected BaseEfRepoTestFixture()
        {
            var dbOptions = CreateNewContextOptions(GetType().Name);

            _catalogContext = new CatalogContext(dbOptions);

            // EnsureCreated totally bypasses migrations and just creates the schema for you, you can't mix this with migrations.
            // EnsureCreated is designed for testing or rapid prototyping where you are ok with dropping and re-creating the database each time.
            // If you are using migrations and want to have them automatically applied on app start, then you can use context.Database.Migrate() instead.
            _catalogContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _catalogContext.Database.EnsureDeleted();
        }

        private static DbContextOptions<CatalogContext> CreateNewContextOptions(string testName)
        {
            var dbSuffix = $"{testName}_{Guid.NewGuid()}";

            // Create a fresh service provider, and therefore a fresh
            // Npgsql database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .BuildServiceProvider();

            // Create a new options instance telling the context to use an
            // Npgsql database and the new service provider.
            var postgresConnectionString = $"Host=localhost;Port=5432;Database=integration_tests_{dbSuffix};Username=postgres;Password=reallyStrongPwd123;Maximum Pool Size=1";
            var builder = new DbContextOptionsBuilder<CatalogContext>();
            builder.UseNpgsql(postgresConnectionString)
                .UseInternalServiceProvider(serviceProvider)
                .UseSnakeCaseNamingConvention(); // https://www.npgsql.org/efcore/modeling/table-column-naming.html

            return builder.Options;
        }

        private static IConfigurationRoot BuildConfig()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json", true, reloadOnChange: false);

            if (environmentName != null)
            {
                config.AddJsonFile($"appsettings.{environmentName}.json", true, reloadOnChange: false);
            }

            config.AddEnvironmentVariables();

            return config.Build();
        }

        protected IAsyncRepository<T> GetRepository<T>() 
            where T : BaseEntity, IAggregateRoot
        {
            return new EfRepository<T>(_catalogContext);
        }
    }
}