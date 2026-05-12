using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUuidToBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                table: "Vehicles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                table: "VehiclePhotos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                table: "Rentals",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Uuid",
                table: "Vehicles",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePhotos_Uuid",
                table: "VehiclePhotos",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Uuid",
                table: "Users",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_Uuid",
                table: "Rentals",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Uuid",
                table: "Employees",
                column: "Uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Uuid",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_VehiclePhotos_Uuid",
                table: "VehiclePhotos");

            migrationBuilder.DropIndex(
                name: "IX_Users_Uuid",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_Uuid",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Uuid",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "VehiclePhotos");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "Customers");
        }
    }
}
