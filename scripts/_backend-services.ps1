# Shared list of backend services (APIs + workers). Used by start-backend.ps1 / stop-backend.ps1.
$script:BackendServices = @(
    @{ Name = "audit-projection";        Project = "backend\src\Services\Audit\Audit.Projection.Worker\Audit.Projection.Worker.csproj" }
    @{ Name = "reporting-projection";    Project = "backend\src\Services\Reporting\Reporting.Projection.Worker\Reporting.Projection.Worker.csproj" }
    @{ Name = "user-projection";         Project = "backend\src\Services\User\User.Projection.Worker\User.Projection.Worker.csproj" }
    @{ Name = "product-projection";      Project = "backend\src\Services\Product\Product.Projection.Worker\Product.Projection.Worker.csproj" }
    @{ Name = "cart-projection";         Project = "backend\src\Services\Cart\Cart.Projection.Worker\Cart.Projection.Worker.csproj" }
    @{ Name = "order-projection";         Project = "backend\src\Services\Order\Order.Projection.Worker\Order.Projection.Worker.csproj" }
    @{ Name = "order-integration";        Project = "backend\src\Services\Order\Order.Integration.Worker\Order.Integration.Worker.csproj" }
    @{ Name = "payment-projection";       Project = "backend\src\Services\Payment\Payment.Projection.Worker\Payment.Projection.Worker.csproj" }
    @{ Name = "checkout-saga-worker";    Project = "backend\src\Services\Saga\CheckoutSaga.Worker\CheckoutSaga.Worker.csproj" }
    @{ Name = "inventory-integration";   Project = "backend\src\Services\Inventory\Inventory.Integration.Worker\Inventory.Integration.Worker.csproj" }
    @{ Name = "auth-api";                Project = "backend\src\Services\Auth\Auth.Api\Auth.Api.csproj" }
    @{ Name = "product-commands";        Project = "backend\src\Services\Product\Product.Commands.Api\Product.Commands.Api.csproj" }
    @{ Name = "product-queries";          Project = "backend\src\Services\Product\Product.Queries.Api\Product.Queries.Api.csproj" }
    @{ Name = "cart-commands";            Project = "backend\src\Services\Cart\Cart.Commands.Api\Cart.Commands.Api.csproj" }
    @{ Name = "cart-queries";             Project = "backend\src\Services\Cart\Cart.Queries.Api\Cart.Queries.Api.csproj" }
    @{ Name = "order-commands";           Project = "backend\src\Services\Order\Order.Commands.Api\Order.Commands.Api.csproj" }
    @{ Name = "order-queries";            Project = "backend\src\Services\Order\Order.Queries.Api\Order.Queries.Api.csproj" }
    @{ Name = "payment-commands";         Project = "backend\src\Services\Payment\Payment.Commands.Api\Payment.Commands.Api.csproj" }
    @{ Name = "payment-queries";          Project = "backend\src\Services\Payment\Payment.Queries.Api\Payment.Queries.Api.csproj" }
    @{ Name = "user-commands";            Project = "backend\src\Services\User\User.Commands.Api\User.Commands.Api.csproj" }
    @{ Name = "user-queries";             Project = "backend\src\Services\User\User.Queries.Api\User.Queries.Api.csproj" }
    @{ Name = "inventory-commands";       Project = "backend\src\Services\Inventory\Inventory.Commands.Api\Inventory.Commands.Api.csproj" }
    @{ Name = "inventory-queries";        Project = "backend\src\Services\Inventory\Inventory.Queries.Api\Inventory.Queries.Api.csproj" }
    @{ Name = "reporting-queries";        Project = "backend\src\Services\Reporting\Reporting.Queries.Api\Reporting.Queries.Api.csproj" }
    @{ Name = "checkout-saga-api";        Project = "backend\src\Services\Saga\CheckoutSaga.Api\CheckoutSaga.Api.csproj" }
    @{ Name = "shop-admin-api";           Project = "backend\src\Gateway\Shop.Admin.Api\Shop.Admin.Api.csproj" }
    @{ Name = "shop-gateway-api";         Project = "backend\src\Gateway\Shop.Gateway.Api\Shop.Gateway.Api.csproj" }
)
