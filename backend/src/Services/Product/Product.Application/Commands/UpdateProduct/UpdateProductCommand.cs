using MediatR;

namespace Product.Application.Commands.UpdateProduct;

public sealed record UpdateProductCommand(Guid ProductId, string Name, decimal Price) : IRequest;
