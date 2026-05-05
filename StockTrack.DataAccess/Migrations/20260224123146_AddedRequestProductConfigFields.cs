using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedRequestProductConfigFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestProducts",
                table: "RequestProducts");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RequestProducts",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsOfficeDelivery",
                table: "RequestForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShipAfterReturn",
                table: "RequestForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestProducts",
                table: "RequestProducts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RequestProducts_RequestFormId",
                table: "RequestProducts",
                column: "RequestFormId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestProducts",
                table: "RequestProducts");

            migrationBuilder.DropIndex(
                name: "IX_RequestProducts_RequestFormId",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RequestProducts");

            migrationBuilder.DropColumn(
                name: "IsOfficeDelivery",
                table: "RequestForms");

            migrationBuilder.DropColumn(
                name: "IsShipAfterReturn",
                table: "RequestForms");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestProducts",
                table: "RequestProducts",
                columns: new[] { "RequestFormId", "ProductId" });
        }
    }
}
