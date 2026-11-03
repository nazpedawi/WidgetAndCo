using ApplicationServices.Commands;
using ApplicationServices.Commands.Handlers;
using ApplicationServices.DTO;
using ApplicationServices.Extensions;
using ApplicationServices.Queries;
using ApplicationServices.Queries.Handlers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Tests
{
    public class MediatRRegistrationTests
    {
        [Test]
        public void AddApplicationServices_RegistersCommandAndQueryHandlers()
        {
            var services = new ServiceCollection();

            services.AddApplicationServices();

            Assert.Multiple(() =>
            {
                Assert.That(services.Any(d =>
                    d.ServiceType == typeof(IRequestHandler<PlaceOrderCommand, string>) &&
                    d.ImplementationType == typeof(PlaceOrderCommandHandler)), Is.True);

                Assert.That(services.Any(d =>
                    d.ServiceType == typeof(IRequestHandler<GetOrdersByCustomerQuery, IEnumerable<OrderDto>>) &&
                    d.ImplementationType == typeof(GetOrdersByCustomerQueryHandler)), Is.True);
            });
        }
    }
}