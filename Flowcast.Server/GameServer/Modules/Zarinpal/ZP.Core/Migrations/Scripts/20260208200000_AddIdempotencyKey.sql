-- Add IdempotencyKey column and unique index (run on DB: Zarinpal / table: PaymentRequests)
-- Run in SSMS or: sqlcmd -S "(localdb)\mssqllocaldb" -d Zarinpal -i "path\to\thisfile.sql"

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PaymentRequests]') AND name = 'IdempotencyKey')
    ALTER TABLE [dbo].[PaymentRequests] ADD [IdempotencyKey] nvarchar(128) NULL;
GO

SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PaymentRequests_IdempotencyKey')
    CREATE UNIQUE NONCLUSTERED INDEX [IX_PaymentRequests_IdempotencyKey]
    ON [dbo].[PaymentRequests] ([IdempotencyKey] ASC)
    WHERE [IdempotencyKey] IS NOT NULL;
GO

-- After running above, register the migration so "dotnet ef database update" does not re-apply it:
-- INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
-- VALUES (N'20260208200000_AddIdempotencyKey', N'10.0.1');
