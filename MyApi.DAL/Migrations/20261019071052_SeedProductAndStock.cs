using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyApi.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeedProductAndStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "ProductCode", "CreatedBy", "CreatedDate", "Price", "ProductName", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { "P01", "system", new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Utc), 15m, "Pen", null, null },
                    { "P02", "system", new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Utc), 45m, "Notebook", null, null },
                    { "P03", "system", new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Utc), 9m, "Eraser", null, null }
                });

            migrationBuilder.InsertData(
                table: "Stock",
                columns: new[] { "ProductCode", "Balance", "CreatedBy", "CreatedDate", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { "P01", 50, "system", new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { "P02", 20, "system", new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { "P03", 99, "system", new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "ProductCode",
                keyValue: "P01");

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "ProductCode",
                keyValue: "P02");

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "ProductCode",
                keyValue: "P03");

            migrationBuilder.DeleteData(
                table: "Stock",
                keyColumn: "ProductCode",
                keyValue: "P01");

            migrationBuilder.DeleteData(
                table: "Stock",
                keyColumn: "ProductCode",
                keyValue: "P02");

            migrationBuilder.DeleteData(
                table: "Stock",
                keyColumn: "ProductCode",
                keyValue: "P03");
        }
    }
}
