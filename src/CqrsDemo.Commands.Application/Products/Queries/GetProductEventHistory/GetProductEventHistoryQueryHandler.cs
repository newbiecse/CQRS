using CqrsDemo.Commands.Application.Abstractions;
using MediatR;

namespace CqrsDemo.Commands.Application.Products.Queries.GetProductEventHistory;

public sealed class GetProductEventHistoryQueryHandler(IProductWriteUnitOfWork unitOfWork)
    : IRequestHandler<GetProductEventHistoryQuery, IReadOnlyList<StoredEventDto>?>
{
    public async Task<IReadOnlyList<StoredEventDto>?> Handle(
        GetProductEventHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var product = await unitOfWork.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        return await unitOfWork.GetEventHistoryAsync(request.ProductId, cancellationToken);
    }
}
