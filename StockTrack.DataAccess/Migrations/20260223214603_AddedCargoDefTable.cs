using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedCargoDefTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Hospital",
                table: "Hospital");

            migrationBuilder.RenameTable(
                name: "Hospital",
                newName: "Hospitals");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Hospitals",
                table: "Hospitals",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CargoDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefinitionType = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoDefinitions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CargoDefinitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Hospitals",
                table: "Hospitals");

            migrationBuilder.RenameTable(
                name: "Hospitals",
                newName: "Hospital");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Hospital",
                table: "Hospital",
                column: "Id");
        }
    }
}
