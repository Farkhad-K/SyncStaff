using Microsoft.AspNetCore.Mvc;
using SyncStaff.MVC.Abstractions;
using SyncStaff.MVC.Models;

namespace SyncStaff.MVC.Controllers;

// Controller responsible for employee-related UI endpoints.
// Delegates business logic to `IEmployeesService` and returns views or status codes.
public class EmployeesController(IEmployeesService employeesService) : Controller
{
    // Show the main overview page listing all employees.
    // Returns the `Overview` view populated with a list of employees.
    public async Task<IActionResult> Overview(CancellationToken abortionToken = default)
    {
        var employees = await employeesService.GetAllEmployeesAsync(abortionToken);
        return View("~/Views/Employees/Overview.cshtml", employees);
    }

    [HttpGet]
    // Return the details page for a single employee id.
    // Returns `BadRequest` for invalid ids, `NotFound` when missing, otherwise the `Details` view.
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0) return BadRequest();
        var employee = await employeesService.GetEmployeeByIdAsync(id, cancellationToken);
        if (employee is null) return NotFound();
        return View("~/Views/Employees/Details.cshtml", employee);
    }

    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    // Handle CSV import upload from the overview page.
    // Accepts a file, delegates parsing/insertion to the service and then redisplays the overview with a message.
    public async Task<IActionResult> Import(IFormFile file, CancellationToken abortionToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded");

        using var stream = file.OpenReadStream();
        var (inserted, failed) = await employeesService.ImportFromCsvAsync(stream, abortionToken);

        var employees = await employeesService.GetAllEmployeesAsync(abortionToken);
        ViewBag.Message = $"Inserted {inserted} rows. Failed {failed} rows.";
        return View("~/Views/Employees/Overview.cshtml", employees);
    }

    [HttpPut]
    // Updates a single employee record from form data. Returns 400 for bad input and 200 on success.
    public async Task<IActionResult> UpdateEmployee([FromForm] Employee model, CancellationToken abortionToken = default)
    {
        if (model is null)
            return BadRequest();

        await employeesService.UpdateEmployeeAsync(model, abortionToken);
        return Ok();
    }

    [HttpDelete]
    // Deletes an employee by id. Expects `employeeId` as query param; validates input before calling service.
    public async Task<IActionResult> DeleteEmployee([FromQuery] int employeeId, CancellationToken abortionToken = default)
    {
        if (employeeId <= 0)
            return BadRequest();

        await employeesService.DeleteEmployeeAsync(employeeId, abortionToken);
        return Ok();
    }
}