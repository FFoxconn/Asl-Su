using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AslSu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreTgoMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TgoStoreId",
                table: "Stores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TgoStoreId",
                table: "Stores");
        }
    }
}
