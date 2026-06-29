using MediatR;

namespace Inventory.Application.Commands.InitializeInventory;

public sealed record InitializeInventoryCommand(Guid ProductId, string ProductName, int InitialOnHand = 100)
    : IRequest;
