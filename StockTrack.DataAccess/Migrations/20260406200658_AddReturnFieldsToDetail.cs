using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnFieldsToDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "RequestFormDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWastage",
                table: "RequestFormDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReturnReason",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnedBy",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnedDate",
                table: "RequestFormDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnedSerialNumber",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "IsWastage",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "ReturnReason",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "ReturnedBy",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "ReturnedDate",
                table: "RequestFormDetails");

            migrationBuilder.DropColumn(
                name: "ReturnedSerialNumber",
                table: "RequestFormDetails");
        }
    }
}
