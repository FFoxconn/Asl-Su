using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AslSu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleStatusToInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnSale",
                table: "StoreProductInventories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LastSyncedIsOnSale",
                table: "StoreProductInventories",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaleStatusLastSyncedAt",
                table: "StoreProductInventories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnsaleReasonCode",
                table: "StoreProductInventories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnSale",
                table: "StoreProductInventories");

            migrationBuilder.DropColumn(
                name: "LastSyncedIsOnSale",
                table: "StoreProductInventories");

            migrationBuilder.DropColumn(
                name: "SaleStatusLastSyncedAt",
                table: "StoreProductInventories");

            migrationBuilder.DropColumn(
                name: "UnsaleReasonCode",
                table: "StoreProductInventories");
        }
    }
}
