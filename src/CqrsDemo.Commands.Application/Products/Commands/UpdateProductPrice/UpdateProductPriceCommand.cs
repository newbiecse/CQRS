using MediatR;

namespace CqrsDemo.Commands.Application.Products.Commands.UpdateProductPrice;

public sealed record UpdateProductPriceCommand(Guid ProductId, decimal NewPrice) : IRequest;
