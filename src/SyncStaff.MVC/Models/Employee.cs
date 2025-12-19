namespace SyncStaff.MVC.Models;

// Domain model representing an employee record stored in the Employees table.
public class Employee
{
    // Primary key
    public int Id { get; set; }

    // Unique payroll identifier used to correlate external data imports.
    public required string PayrollNumber { get; set; }

    // Person name fields
    public required string Forenames { get; set; }
    public required string Surname { get; set; }

    // Stored as SQL `date` via conversion in configuration
    public DateOnly DateofBirth { get; set; }

    // Contact details
    public required string Email { get; set; }
    public string? Telephone { get; set; }
    public string? Mobile { get; set; }

    // Postal address fields
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? Postcode { get; set; }

    // Employment start date (stored as SQL `date`)
    public DateOnly StartDate { get; set; }

    // Audit timestamps stored as `datetimeoffset` in the DB
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}