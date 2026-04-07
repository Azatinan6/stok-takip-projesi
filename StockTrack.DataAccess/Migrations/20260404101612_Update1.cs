using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Update1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RequestFormDetails_RequestFormId",
                table: "RequestFormDetails",
                column: "RequestFormId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestFormDetails_RequestForms_RequestFormId",
                table: "RequestFormDetails",
                column: "RequestFormId",
                principalTable: "RequestForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestFormDetails_RequestForms_RequestFormId",
                table: "RequestFormDetails");

            migrationBuilder.DropIndex(
                name: "IX_RequestFormDetails_RequestFormId",
                table: "RequestFormDetails");
        }
    }
}
