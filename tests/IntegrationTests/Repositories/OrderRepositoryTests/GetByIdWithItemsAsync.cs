using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.UnitTests.Builders;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Repositories.OrderRepositoryTests
{
    public class GetByIdWithItemsAsync : BaseEfRepoTestFixture
    {
        private readonly OrderRepository _orderRepository;
        private OrderBuilder OrderBuilder { get; } = new();

        public GetByIdWithItemsAsync()
        {
            _orderRepository = new OrderRepository(CatalogContext);
        }

        [Fact(Skip = "Do not work with real database")]
        public async Task GetOrderAndItemsByOrderIdWhenMultipleOrdersPresent()
        {
            //Arrange
            var itemOneUnitPrice = 5.50m;
            var itemOneUnits = 2;
            var itemTwoUnitPrice = 7.50m;
            var itemTwoUnits = 5;

            var firstOrder = OrderBuilder.WithDefaultValues();
            await CatalogContext.Orders.AddAsync(firstOrder);
            await CatalogContext.SaveChangesAsync();

            var secondOrderItems = new List<OrderItem>
            {
                new(OrderBuilder.TestCatalogItemOrdered, itemOneUnitPrice, itemOneUnits),
                new(OrderBuilder.TestCatalogItemOrdered, itemTwoUnitPrice, itemTwoUnits)
            };
            var secondOrder = OrderBuilder.WithItems(secondOrderItems);
            await CatalogContext.Orders.AddAsync(secondOrder);
            await CatalogContext.SaveChangesAsync();
            var secondOrderId = secondOrder.Id;

            //Act
            var orderFromRepo = await _orderRepository.GetByIdWithItemsAsync(secondOrderId);

            //Assert
            Assert.Equal(secondOrderId, orderFromRepo.Id);
            Assert.Equal(secondOrder.OrderItems.Count, orderFromRepo.OrderItems.Count);
            Assert.Equal(1, orderFromRepo.OrderItems.Count(x => x.UnitPrice == itemOneUnitPrice));
            Assert.Equal(1, orderFromRepo.OrderItems.Count(x => x.UnitPrice == itemTwoUnitPrice));
            Assert.Equal(itemOneUnits, orderFromRepo.OrderItems.SingleOrDefault(x => x.UnitPrice == itemOneUnitPrice).Units);
            Assert.Equal(itemTwoUnits, orderFromRepo.OrderItems.SingleOrDefault(x => x.UnitPrice == itemTwoUnitPrice).Units);
        }
    }
}
