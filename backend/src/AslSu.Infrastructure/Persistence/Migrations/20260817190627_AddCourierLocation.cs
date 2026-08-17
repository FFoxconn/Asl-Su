using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AslSu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCourierLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Couriers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LocationUpdatedAt",
                table: "Couriers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Couriers",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "LocationUpdatedAt",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Couriers");
        }
    }
}
