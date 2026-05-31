using MediatR;

namespace CqrsDemo.Commands.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<Guid>;
