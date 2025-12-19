using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SyncStaff.MVC.Models;

namespace SyncStaff.MVC.Data;

public interface ISyncStaffDbContext
{
    DatabaseFacade Database { get; }
    DbSet<Employee> Employees { get; set; }

    void DetachEntity(object entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}