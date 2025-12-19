using Microsoft.EntityFrameworkCore;
using SyncStaff.MVC.Models;

namespace SyncStaff.MVC.Data;

// Concrete EF Core DbContext implementation wired into DI as `ISyncStaffDbContext`.
// Holds DbSets and applies entity configurations from this assembly.
public class SyncStaffDbContext(
    DbContextOptions<SyncStaffDbContext> options)
    : DbContext(options), ISyncStaffDbContext
{
    // Employees table mapping
    public DbSet<Employee> Employees { get; set; }

    // Detach a tracked entity to clear it from the change tracker.
    // Useful when an operation failed and we want to retry without stale tracked state.
    public void DetachEntity(object entity)
    {
        if (entity == null) return;
        var entry = Entry(entity);
        if (entry != null)
            entry.State = EntityState.Detached;
    }

    // Explicit interface implementation that forwards to EF's SaveChangesAsync.
    async Task<int> ISyncStaffDbContext.SaveChangesAsync(CancellationToken cancellationToken)
        => await base.SaveChangesAsync(cancellationToken);

    // Apply entity configurations (fluent API) from this assembly.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SyncStaffDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}