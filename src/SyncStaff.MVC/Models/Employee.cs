namespace SyncStaff.MVC.Models;

public class Employee
{
    public int Id { get; set; }
    public required string PayrollNumber { get; set; }
    public required string Forenames { get; set; }
    public required string Surname { get; set; }
    public DateOnly DateofBirth { get; set; }
    public required string Email { get; set; }
    public string? Telephone { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? Postcode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}