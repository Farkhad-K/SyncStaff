using System.Text;
using Microsoft.EntityFrameworkCore;
using SyncStaff.MVC.Abstractions;
using SyncStaff.MVC.Data;
using SyncStaff.MVC.Models;

namespace SyncStaff.MVC.Services;

public class EmployeesService(ISyncStaffDbContext db) : IEmployeesService
{
    public async Task<List<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
        => await db.Employees
                        .AsNoTracking()
                        .OrderBy(e => e.Surname)
                        .ToListAsync(cancellationToken);

    public async Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default)
        => await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

    public async Task UpdateEmployeeAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        var existing = await db.Employees.FirstOrDefaultAsync(e => e.Id == employee.Id, cancellationToken);
        if (existing is null)
            return;

        existing.Forenames = employee.Forenames;
        existing.Surname = employee.Surname;
        existing.Email = employee.Email;
        existing.Telephone = employee.Telephone;
        existing.Mobile = employee.Mobile;
        existing.Address = employee.Address;
        existing.Address2 = employee.Address2;
        existing.Postcode = employee.Postcode;
        existing.StartDate = employee.StartDate;
        existing.ModifiedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(int Inserted, int Failed)> ImportFromCsvAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var inserted = 0;
        var failed = 0;

        using var reader = new StreamReader(csvStream);
        string? header = await reader.ReadLineAsync(cancellationToken);
        if (header is null)
            return (0, 0);

        // Determine header positions (best-effort)
        var headers = header.Split(',').Select(h => h.Trim().Trim('"')).ToArray();

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = SplitCsvLine(line);
            try
            {
                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < Math.Min(headers.Length, parts.Length); i++)
                {
                    map[headers[i]] = parts[i].Trim();
                }

                var dobStr = GetField(map, "Personnel_Records.Date_of_Birth", "Date_of_Birth", "DateOfBirth");
                var startStr = GetField(map, "Personnel_Records.Start_Date", "Start_Date", "StartDate");

                var createdAt = DateTimeOffset.UtcNow;

                var emp = new Employee
                {
                    PayrollNumber = GetField(map, "Personnel_Records.Payroll_Number", "PayrollNumber"),
                    Forenames = GetField(map, "Personnel_Records.Forenames", "Forenames"),
                    Surname = GetField(map, "Personnel_Records.Surname", "Surname"),
                    DateofBirth = ParseDateOnly(dobStr),
                    Telephone = GetField(map, "Personnel_Records.Telephone", "Telephone", "Phone"),
                    Mobile = GetField(map, "Personnel_Records.Mobile", "Mobile"),
                    Address = GetField(map, "Personnel_Records.Address", "Address"),
                    Address2 = GetField(map, "Personnel_Records.Address_2", "Address_2", "Address2"),
                    Postcode = GetField(map, "Personnel_Records.Postcode", "Postcode"),
                    Email = GetField(map, "Personnel_Records.EMail_Home", "EMail_Home", "Email"),
                    StartDate = ParseDateOnly(startStr),
                    CreatedAt = createdAt,
                    ModifiedAt = createdAt
                };

                db.Employees.Add(emp);
                await db.SaveChangesAsync(cancellationToken);
                inserted++;
            }
            catch
            {
                failed++;
            }
        }

        return (inserted, failed);
    }

    public async Task DeleteEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var existingEmp = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
        if (existingEmp == null)
            return;

        db.Employees.Remove(existingEmp);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string GetField(Dictionary<string, string> map, params string[] keys)
    {
        foreach (var k in keys)
        {
            if (map.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v))
                return v;
        }
        return string.Empty;
    }
    
    private static DateOnly ParseDateOnly(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return DateOnly.FromDateTime(DateTime.MinValue);

        input = input.Trim('"', ' ');
        var formats = new[] { "dd/MM/yyyy", "d/M/yyyy", "d/MM/yyyy", "dd/M/yyyy", "yyyy-MM-dd" };
        if (DateOnly.TryParseExact(input, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
            return d;

        if (DateOnly.TryParse(input, out d))
            return d;

        return DateOnly.FromDateTime(DateTime.MinValue);
    }

    // Very small CSV splitter that handles quoted fields with commas
    private static string[] SplitCsvLine(string line)
    {
        var parts = new List<string>();
        bool inQuotes = false;
        var cur = new StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    cur.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                parts.Add(cur.ToString());
                cur.Clear();
            }
            else
            {
                cur.Append(c);
            }
        }
        parts.Add(cur.ToString());
        return parts.ToArray();
    }
}
