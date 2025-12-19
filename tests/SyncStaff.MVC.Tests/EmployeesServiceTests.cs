using System.Text;
using Microsoft.EntityFrameworkCore;
using SyncStaff.MVC.Data;
using SyncStaff.MVC.Services;
using Xunit;

namespace SyncStaff.MVC.Tests;

public class EmployeesServiceTests
{
    [Fact]
    public async Task ImportFromCsv_InsertsRows()
    {
        var options = new DbContextOptionsBuilder<SyncStaffDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb1")
            .Options;

        await using var ctx = new SyncStaffDbContext(options);

        var service = new EmployeesService(ctx);

        var csv = new StringBuilder();
        csv.AppendLine("Personnel_Records.Payroll_Number,Personnel_Records.Forenames,Personnel_Records.Surname,Personnel_Records.Date_of_Birth,Personnel_Records.Telephone,Personnel_Records.Mobile,Personnel_Records.Address,Personnel_Records.Address_2,Personnel_Records.Postcode,Personnel_Records.EMail_Home,Personnel_Records.Start_Date");
        csv.AppendLine("COOP08,John,William,26/01/1955,12345678,987654231,12 Foreman road,London,GU12 6JW,nomadic20@hotmail.co.uk,18/04/2013");
        csv.AppendLine("JACK13,Jerry,Jackson,11/5/1974,2050508,6987457,115 Spinney Road,Luton,LU33DF,gerry.jackson@bt.com,18/04/2013");

        using var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));

        var (inserted, failed) = await service.ImportFromCsvAsync(ms);

        Assert.Equal(2, inserted);
        Assert.Equal(0, failed);

        var all = await service.GetAllEmployeesAsync();
        Assert.Equal(2, all.Count);
    }
}
