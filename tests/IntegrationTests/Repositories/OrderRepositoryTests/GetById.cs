﻿using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.UnitTests.Builders;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.eShopWeb.IntegrationTests.Repositories.OrderRepositoryTests
{
    public class GetById : BaseEfRepoTestFixture
    {
        private readonly OrderRepository _orderRepository;
        private OrderBuilder OrderBuilder { get; } = new();
        private readonly ITestOutputHelper _output;
        public GetById(ITestOutputHelper output)
        {
            _output = output;
            _orderRepository = new OrderRepository(CatalogContext);
        }

        [Fact]
        public async Task GetsExistingOrder()
        {
            var existingOrder = OrderBuilder.WithDefaultValues();
            await CatalogContext.Orders.AddAsync(existingOrder);
            await CatalogContext.SaveChangesAsync();
            var orderId = existingOrder.Id;
            _output.WriteLine($"OrderId: {orderId}");

            var orderFromRepo = await _orderRepository.GetByIdAsync(orderId);
            Assert.Equal(OrderBuilder.TestBuyerId, orderFromRepo.BuyerId);

            // Note: Using InMemoryDatabase OrderItems is available. Will be null if using SQL DB.
            var firstItem = orderFromRepo.OrderItems.FirstOrDefault();
            Assert.Equal(OrderBuilder.TestUnits, firstItem!.Units);
        }
    }
}
