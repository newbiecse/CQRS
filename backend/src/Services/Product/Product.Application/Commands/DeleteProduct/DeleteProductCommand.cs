using MediatR;

namespace Product.Application.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid ProductId) : IRequest;
