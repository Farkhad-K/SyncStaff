using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SyncStaff.MVC.Models;

namespace SyncStaff.MVC.Data;

// Lightweight abstraction over EF Core DbContext used by the application.
// This interface is targeted so services can depend on a minimal surface instead of the concrete DbContext.
public interface ISyncStaffDbContext
{
    // Expose the EF Database facade for raw operations/transactions when needed.
    DatabaseFacade Database { get; }

    // Employees DbSet used by repository/service code.
    DbSet<Employee> Employees { get; set; }

    // Detach an entity from the change tracker (useful when a failed SaveChanges leaves a tracked state).
    void DetachEntity(object entity);

    // Async save changes wrapper so callers can provide a CancellationToken and the concrete context can control persistence.
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}