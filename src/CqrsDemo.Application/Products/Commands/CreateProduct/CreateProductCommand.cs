using MediatR;

namespace CqrsDemo.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<Guid>;
