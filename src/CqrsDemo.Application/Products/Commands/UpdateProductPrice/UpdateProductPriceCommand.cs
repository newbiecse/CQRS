using MediatR;

namespace CqrsDemo.Application.Products.Commands.UpdateProductPrice;

public sealed record UpdateProductPriceCommand(Guid ProductId, decimal NewPrice) : IRequest;
