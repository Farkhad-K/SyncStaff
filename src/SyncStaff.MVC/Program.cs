using Microsoft.EntityFrameworkCore;
using SyncStaff.MVC.Data;
using SyncStaff.MVC.Abstractions;
using SyncStaff.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ISyncStaffDbContext, SyncStaffDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SyncStaff")));

builder.Services.AddScoped<IEmployeesService, EmployeesService>();
       
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
