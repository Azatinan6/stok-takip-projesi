using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRepoLocationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ProductMainRepoLocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ProductMainRepoLocations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ProductMainRepoLocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "ProductMainRepoLocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ProductMainRepoLocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProductMainRepoLocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductMainRepoLocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "ProductMainRepoLocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "ProductMainRepoLocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductStatusId",
                table: "ProductMainRepoLocations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProductMainRepoLocations");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ProductMainRepoLocations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ProductMainRepoLocations");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "ProductMainRepoLocations");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ProductMainRepoLocations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProductMainRepoLocations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductMainRepoLocations");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "ProductMainRepoLocations");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "ProductMainRepoLocations");

            migrationBuilder.DropColumn(
                name: "ProductStatusId",
                table: "ProductMainRepoLocations");
        }
    }
}
