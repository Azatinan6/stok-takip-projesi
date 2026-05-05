using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNewStatusTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StatusTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 11, "Ofisten Teslim Alınacak" },
                    { 13, "İade Geldiği Zaman Kargolanacak" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StatusTypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "StatusTypes",
                keyColumn: "Id",
                keyValue: 13);
        }
    }
}
