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
        if (csvStream is null) throw new ArgumentNullException(nameof(csvStream));

        int inserted = 0;
        int failed = 0;

        // 1) Parse CSV into memory (validated rows only)
        var parsed = new List<Employee>();
        using var reader = new StreamReader(csvStream, Encoding.UTF8);
        string? header = await reader.ReadLineAsync(cancellationToken);
        if (header is null)
            return (0, 0); // empty file

        var headers = header.Split(',').Select(h => h.Trim().Trim('"')).ToArray();

        string? line;
        var seenPayrollsInCsv = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = SplitCsvLine(line);
            try
            {
                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < Math.Min(headers.Length, parts.Length); i++)
                    map[headers[i]] = parts[i].Trim().Trim('"');

                var payroll = GetField(map, "Personnel_Records.Payroll_Number", "PayrollNumber")?.Trim() ?? string.Empty;
                var forenames = GetField(map, "Personnel_Records.Forenames", "Forenames")?.Trim() ?? string.Empty;
                var surname = GetField(map, "Personnel_Records.Surname", "Surname")?.Trim() ?? string.Empty;

                // required fields check
                if (string.IsNullOrWhiteSpace(payroll) || string.IsNullOrWhiteSpace(forenames) || string.IsNullOrWhiteSpace(surname))
                {
                    failed++;
                    continue;
                }

                // skip duplicates inside CSV
                if (!seenPayrollsInCsv.Add(payroll))
                {
                    failed++; // duplicate in CSV
                    continue;
                }

                var dobStr = GetField(map, "Personnel_Records.Date_of_Birth", "Date_of_Birth", "DateOfBirth");
                var startStr = GetField(map, "Personnel_Records.Start_Date", "Start_Date", "StartDate");

                var now = DateTimeOffset.UtcNow;

                var emp = new Employee
                {
                    PayrollNumber = payroll,
                    Forenames = forenames,
                    Surname = surname,
                    DateofBirth = ParseDateOnly(dobStr),
                    Telephone = GetField(map, "Personnel_Records.Telephone", "Telephone", "Phone"),
                    Mobile = GetField(map, "Personnel_Records.Mobile", "Mobile"),
                    Address = GetField(map, "Personnel_Records.Address", "Address"),
                    Address2 = GetField(map, "Personnel_Records.Address_2", "Address_2", "Address2"),
                    Postcode = GetField(map, "Personnel_Records.Postcode", "Postcode"),
                    Email = GetField(map, "Personnel_Records.EMail_Home", "EMail_Home", "Email"),
                    StartDate = ParseDateOnly(startStr),
                    CreatedAt = now,
                    ModifiedAt = now
                };

                parsed.Add(emp);
            }
            catch
            {
                failed++;
            }
        }

        if (!parsed.Any())
            return (0, failed);

        // 2) Query DB once for existing payroll numbers (case-insensitive)
        var incomingPayrolls = parsed.Select(p => p.PayrollNumber!.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        // Protect against empty list in Contains() (some providers don't like empty lists)
        var existingPayrolls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (incomingPayrolls.Any())
        {
            var dbPayrolls = await db.Employees
                .Where(e => incomingPayrolls.Contains(e.PayrollNumber!))
                .Select(e => e.PayrollNumber!)
                .ToListAsync(cancellationToken);

            foreach (var p in dbPayrolls)
                existingPayrolls.Add(p);
        }

        // 3) Filter out existing ones and prepare to insert (duplicates already removed from CSV)
        var toInsert = new List<Employee>();
        foreach (var e in parsed)
        {
            var pn = e.PayrollNumber?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(pn) || existingPayrolls.Contains(pn))
            {
                // already counted as failed for CSV issues; but if it's an existing DB record, count it as failed here
                failed++;
                continue;
            }
            toInsert.Add(e);
        }

        // 4) Bulk insert remaining rows
        if (toInsert.Any())
        {
            db.Employees.AddRange(toInsert);
            try
            {
                await db.SaveChangesAsync(cancellationToken);
                inserted += toInsert.Count;
            }
            catch (DbUpdateException)
            {
                // fallback to per-row insert to salvage partial successes
                foreach (var e in toInsert)
                {
                    try
                    {
                        db.Employees.Add(e);
                        await db.SaveChangesAsync(cancellationToken);
                        inserted++;
                    }
                    catch
                    {
                        // detach the entity if it failed so next SaveChanges is clean
                        try { db.DetachEntity(e); } catch { /* ignore */ }
                        failed++;
                    }
                }
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

    // Small CSV splitter that handles quoted fields with commas
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
