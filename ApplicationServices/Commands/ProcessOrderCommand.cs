using DomainLayer.Models.Orders;
using MediatR;

namespace ApplicationServices.Commands
{
    public record ProcessOrderCommand(Order Order) : IRequest;
}