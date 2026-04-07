using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestForms_Hospitals_HospitalId",
                table: "RequestForms");

            migrationBuilder.DropIndex(
                name: "IX_RequestForms_HospitalId",
                table: "RequestForms");

            migrationBuilder.DropColumn(
                name: "HospitalId",
                table: "RequestForms");

            migrationBuilder.AlterColumn<int>(
                name: "LocationListId",
                table: "RequestForms",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "LocationListId",
                table: "RequestForms",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "HospitalId",
                table: "RequestForms",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestForms_HospitalId",
                table: "RequestForms",
                column: "HospitalId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestForms_Hospitals_HospitalId",
                table: "RequestForms",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "Id");
        }
    }
}
