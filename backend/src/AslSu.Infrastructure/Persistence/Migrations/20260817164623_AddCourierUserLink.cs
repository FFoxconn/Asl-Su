using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AslSu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCourierUserLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Couriers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Couriers_UserId",
                table: "Couriers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Couriers_Users_UserId",
                table: "Couriers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Couriers_Users_UserId",
                table: "Couriers");

            migrationBuilder.DropIndex(
                name: "IX_Couriers_UserId",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Couriers");
        }
    }
}
