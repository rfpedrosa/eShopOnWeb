using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.UnitTests.Builders;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Repositories.BasketRepositoryTests
{
    public class SetQuantities
    {
        private readonly CatalogContext _catalogContext;
        private readonly IAsyncRepository<Basket> _basketRepository;
        private readonly BasketBuilder BasketBuilder = new BasketBuilder();

        public SetQuantities()
        {
            // Create a fresh service provider, and therefore a fresh
            // Npgsql database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .BuildServiceProvider();
            
            // Create a new options instance telling the context to use an
            // Npgsql database and the new service provider.
            // var postgresConnectionString = string.Format(Configuration.GetConnectionString("IntegrationTestsDb"), dbSuffix);
            const string postgresConnectionString = "Host=localhost;Port=5433;Database=integration_tests;Username=postgres;Password=reallyStrongPwd123;Maximum Pool Size=1";
            var builder = new DbContextOptionsBuilder<CatalogContext>();
            builder.UseNpgsql(postgresConnectionString)
                .UseInternalServiceProvider(serviceProvider)
                .UseSnakeCaseNamingConvention(); // https://www.npgsql.org/efcore/modeling/table-column-naming.html

            var dbOptions = builder.Options;
            
            _catalogContext = new CatalogContext(dbOptions);
            
            // EnsureCreated totally bypasses migrations and just creates the schema for you, you can't mix this with migrations.
            // EnsureCreated is designed for testing or rapid prototyping where you are ok with dropping and re-creating the database each time.
            // If you are using migrations and want to have them automatically applied on app start, then you can use context.Database.Migrate() instead.
            _catalogContext.Database.EnsureCreated();
            
            _basketRepository = new EfRepository<Basket>(_catalogContext);
        }

        [Fact]
        public async Task RemoveEmptyQuantities()
        {
            var basket = BasketBuilder.WithOneBasketItem();
            var basketService = new BasketService(_basketRepository, null);
            await _basketRepository.AddAsync(basket);
            await _catalogContext.SaveChangesAsync();

            await basketService.SetQuantities(BasketBuilder.BasketId, new Dictionary<string, int>() { { BasketBuilder.BasketId.ToString(), 0 } });

            Assert.Equal(0, basket.Items.Count);
        }
    }
}
