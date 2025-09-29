//using SharedKernel;
//using Microsoft.EntityFrameworkCore;
//using Shared.Application.Services;

//namespace Shared.Infrastructure.Database;


//public sealed class ApplicationDbContext(
//    DbContextOptions<ApplicationDbContext> options,
//    IDomainEventsDispatcher domainEventsDispatcher)
//    : DbContext(options), IApplicationDbContext
//{
//    public DbSet<User> Users { get; set; }
//    public DbSet<UserProgress> UserProgress { get; set; }

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

//        modelBuilder.HasDefaultSchema(Schemas.Default);
//    }

//    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
//    {
//        // When should you publish domain events?
//        //
//        // 1. BEFORE calling SaveChangesAsync
//        //     - domain events are part of the same transaction
//        //     - immediate consistency
//        // 2. AFTER calling SaveChangesAsync
//        //     - domain events are a separate transaction
//        //     - eventual consistency
//        //     - handlers can fail

//        int result = await base.SaveChangesAsync(cancellationToken);

//        await PublishDomainEventsAsync();

//        return result;
//    }

//    private async Task PublishDomainEventsAsync()
//    {
//        var domainEvents = ChangeTracker
//            .Entries<Entity>()
//            .Select(entry => entry.Entity)
//            .SelectMany(entity =>
//            {
//                List<IDomainEvent> domainEvents = entity.DomainEvents.ToList();

//                entity.ClearDomainEvents();

//                return domainEvents;
//            })
//            .ToList();

//        await domainEventsDispatcher.DispatchAsync(domainEvents);
//    }
//}
