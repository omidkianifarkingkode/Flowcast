namespace Shared.Infrastructure.Database;

public record struct DbContextSetupOptions(string ConnectionString, string ModuleName, bool UseInMemoryDb);
