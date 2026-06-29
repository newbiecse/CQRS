-- Master script: run each database script in order.
-- Requires sqlcmd or SSMS. Example:
--   sqlcmd -S localhost -E -i scripts\sql\create-all-databases.sql
--
-- Or use: .\scripts\initialize-databases.ps1

:r CqrsDemo_Product_Write.sql
GO
:r CqrsDemo_Cart_Write.sql
GO
:r CqrsDemo_Order_Write.sql
GO
:r CqrsDemo_Payment_Write.sql
GO
:r CqrsDemo_User_Write.sql
GO
:r CqrsDemo_Product_Read.sql
GO
:r CqrsDemo_Cart_Read.sql
GO
:r CqrsDemo_Order_Read.sql
GO
:r CqrsDemo_Payment_Read.sql
GO
:r CqrsDemo_User_Read.sql
GO
:r CqrsDemo_Saga.sql
GO
:r CqrsDemo_Reporting.sql
GO
:r CqrsDemo_Read.sql
GO
