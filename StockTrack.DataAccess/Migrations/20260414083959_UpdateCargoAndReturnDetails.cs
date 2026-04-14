using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCargoAndReturnDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigUrl",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConnectionType",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ControlResult",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EthMac",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductCondition",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceivedQuantity",
                table: "RequestFormDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SendReason",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WlanMac",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ZayiatQuantity",
                table: "RequestFormDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "ProductSerialNumbers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigUrl",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "ConnectionType",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "ControlResult",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "EthMac",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "ProductCondition",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "SendReason",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "WlanMac",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "ZayiatQuantity",
                table: "RequestFormDetails");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "ProductSerialNumbers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
