using MediatR;

namespace Product.Application.Commands.UpdateProductPrice;

public sealed record UpdateProductPriceCommand(Guid ProductId, decimal NewPrice) : IRequest;
