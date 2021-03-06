using System.Threading.Tasks;
using DShop.Common.Handlers;
using DShop.Common.RabbitMq;
using DShop.Services.Customers.Domain;
using DShop.Services.Customers.Messages.Events;
using DShop.Services.Customers.Repositories;

namespace DShop.Services.Customers.Handlers.Products
{
    public class ProductUpdatedHandler : IEventHandler<ProductUpdated>
    {
        private readonly IHandler _handler;
        private readonly ICartsRepository _cartsRepository;
        private readonly IProductsRepository _productsRepository;

        public ProductUpdatedHandler(IHandler handler, 
            ICartsRepository cartsRepository,
            IProductsRepository productsRepository)
        {
            _handler = handler;
            _cartsRepository = cartsRepository;
            _productsRepository = productsRepository;
        }

        public async Task HandleAsync(ProductUpdated @event, ICorrelationContext context)
            => await _handler.Handle(async () => 
            {
                var product = new Product(@event.Id, @event.Name, @event.Price);
                await _productsRepository.UpdateAsync(product);
                var carts = await _cartsRepository.GetAllWithProduct(product.Id);
                foreach (var cart in carts)
                {
                    cart.UpdateProduct(product);
                }
                await _cartsRepository.UpdateManyAsync(carts);
            })
            .ExecuteAsync();
    }
}