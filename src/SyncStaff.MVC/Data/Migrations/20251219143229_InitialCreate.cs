using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncStaff.MVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Forenames = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    DateofBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Telephone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    Mobile = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Address2 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Postcode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CreatedAt",
                table: "Employees",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PayrollNumber",
                table: "Employees",
                column: "PayrollNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Surname",
                table: "Employees",
                column: "Surname");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
