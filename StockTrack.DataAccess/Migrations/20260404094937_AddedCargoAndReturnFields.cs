using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedCargoAndReturnFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ControlResultId",
                table: "RequestProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSerialNumberMatched",
                table: "RequestProducts",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsWaste",
                table: "RequestProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProductStatusId",
                table: "RequestProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceivedQuantity",
                table: "RequestProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedSerialNumber",
                table: "RequestProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WasteDescription",
                table: "RequestProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CargoPreparerUserId",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ControlResultId",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "IsSerialNumberMatched",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "IsWaste",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "ProductStatusId",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "ReceivedSerialNumber",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "WasteDescription",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "CargoPreparerUserId",
                table: "RequestFormDetails");
        }
    }
}
