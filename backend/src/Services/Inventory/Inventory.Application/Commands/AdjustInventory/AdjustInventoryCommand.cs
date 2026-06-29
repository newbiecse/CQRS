using MediatR;

namespace Inventory.Application.Commands.AdjustInventory;

public sealed record AdjustInventoryCommand(Guid ProductId, int OnHand) : IRequest;
