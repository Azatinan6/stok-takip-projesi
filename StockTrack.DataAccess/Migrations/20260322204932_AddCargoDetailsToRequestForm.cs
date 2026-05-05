using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCargoDetailsToRequestForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Adress",
                table: "RequestFormDetails",
                newName: "ReceiverDepartment");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "RequestFormDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "RequestFormDetails");

            migrationBuilder.RenameColumn(
                name: "ReceiverDepartment",
                table: "RequestFormDetails",
                newName: "Adress");
        }
    }
}
