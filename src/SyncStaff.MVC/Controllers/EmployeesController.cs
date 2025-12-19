using Microsoft.AspNetCore.Mvc;
using SyncStaff.MVC.Abstractions;
using SyncStaff.MVC.Models;

namespace SyncStaff.MVC.Controllers;

public class EmployeesController(IEmployeesService employeesService) : Controller
{
    public async Task<IActionResult> Overview(CancellationToken abortionToken = default)
    {
        var employees = await employeesService.GetAllEmployeesAsync(abortionToken);
        return View("~/Views/Employees/Overview.cshtml", employees);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0) return BadRequest();
        var employee = await employeesService.GetEmployeeByIdAsync(id, cancellationToken);
        if (employee is null) return NotFound();
        return View("~/Views/Employees/Details.cshtml", employee);
    }

    [HttpPost]
    [RequestSizeLimit(10_000_000)]
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
    public async Task<IActionResult> UpdateEmployee([FromForm] Employee model, CancellationToken abortionToken = default)
    {
        if (model is null)
            return BadRequest();

        await employeesService.UpdateEmployeeAsync(model, abortionToken);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteEmployee([FromQuery] int employeeId, CancellationToken abortionToken = default)
    {
        if (employeeId <= 0)
            return BadRequest();

        await employeesService.DeleteEmployeeAsync(employeeId, abortionToken);
        return Ok();
    }
}