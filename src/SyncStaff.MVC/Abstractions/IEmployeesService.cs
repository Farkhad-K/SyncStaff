using SyncStaff.MVC.Models;
namespace SyncStaff.MVC.Abstractions;

public interface IEmployeesService
{
    Task<(int Inserted, int Failed)> ImportFromCsvAsync(Stream csvStream, CancellationToken cancellationToken = default);
    Task<List<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);
    Task UpdateEmployeeAsync(Employee employee, CancellationToken cancellationToken = default);
    Task DeleteEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default);
}
