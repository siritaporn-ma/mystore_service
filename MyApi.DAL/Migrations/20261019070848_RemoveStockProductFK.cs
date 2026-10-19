using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStockProductFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stock_Product_ProductCode",
                table: "Stock");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Stock_Product_ProductCode",
                table: "Stock",
                column: "ProductCode",
                principalTable: "Product",
                principalColumn: "ProductCode",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
