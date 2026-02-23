using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCargoNameTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CargoNames",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "CargoNames",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "CargoNames",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "CargoNames",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CargoNames",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CargoNames",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "CargoNames",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "CargoNames",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CargoNames");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "CargoNames");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CargoNames");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "CargoNames");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CargoNames");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CargoNames");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "CargoNames");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "CargoNames");
        }
    }
}
