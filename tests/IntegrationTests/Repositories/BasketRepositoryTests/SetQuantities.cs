using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.UnitTests.Builders;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Repositories.BasketRepositoryTests
{
    public class SetQuantities : BaseEfRepoTestFixture
    {
        private readonly IAsyncRepository<Basket> _basketRepository;
        private readonly BasketBuilder _basketBuilder = new();

        public SetQuantities()
        {
            _basketRepository = GetRepository<Basket>();
        }

        [Fact]
        public async Task RemoveEmptyQuantities()
        {
            var basket = _basketBuilder.WithOneBasketItem();
            var basketService = new BasketService(_basketRepository, null);
            await _basketRepository.AddAsync(basket);

            await basketService.SetQuantities(_basketBuilder.BasketId, new Dictionary<string, int> { { _basketBuilder.BasketId.ToString(), 0 } });

            Assert.Equal(0, basket.Items.Count);
        }
    }
}
