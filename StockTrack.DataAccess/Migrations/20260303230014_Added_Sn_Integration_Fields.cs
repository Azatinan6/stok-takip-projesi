using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Added_Sn_Integration_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SnPassword",
                table: "Hospitals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SnUsername",
                table: "Hospitals",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SnPassword",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "SnUsername",
                table: "Hospitals");
        }
    }
}
