using Microsoft.EntityFrameworkCore;
using SyncStaff.MVC.Models;

namespace SyncStaff.MVC.Data;

public class SyncStaffDbContext(
    DbContextOptions<SyncStaffDbContext> options) 
    : DbContext(options), ISyncStaffDbContext
{
    public DbSet<Employee> Employees { get; set; }

    async Task<int> ISyncStaffDbContext.SaveChangesAsync(CancellationToken cancellationToken)
        => await base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SyncStaffDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}