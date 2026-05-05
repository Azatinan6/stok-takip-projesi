using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDepo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigUrl",
                table: "RequestProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConnectionType",
                table: "RequestProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DhcpdConf",
                table: "RequestProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "RequestProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperationType",
                table: "RequestProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReasonId",
                table: "RequestProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WpaSupplicantConf",
                table: "RequestProducts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigUrl",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "ConnectionType",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "DhcpdConf",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "OperationType",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "ReasonId",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "WpaSupplicantConf",
                table: "RequestProducts");
        }
    }
}
