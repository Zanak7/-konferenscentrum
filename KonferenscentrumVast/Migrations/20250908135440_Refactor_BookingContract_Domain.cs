using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonferenscentrumVast.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_BookingContract_Domain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "BookingContracts");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "BookingContracts",
                newName: "Terms");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "BookingContracts",
                newName: "FacilityName");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "BookingContracts",
                newName: "CustomerName");

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "BookingContracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "BookingContracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractNumber",
                table: "BookingContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "BookingContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "BookingContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "BookingContracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDueDate",
                table: "BookingContracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SignedAt",
                table: "BookingContracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BookingContracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "BookingContracts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "BookingContracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "BookingContracts");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "BookingContracts");

            migrationBuilder.DropColumn(
                name: "ContractNumber",
                table: "BookingContracts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "BookingContracts");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "BookingContracts");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "BookingContracts");

            migrationBuilder.DropColumn(
                name: "PaymentDueDate",
                table: "BookingContracts");

            migrationBuilder.DropColumn(
                name: "SignedAt",
                table: "BookingContracts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BookingContracts");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "BookingContracts");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "BookingContracts");

            migrationBuilder.RenameColumn(
                name: "Terms",
                table: "BookingContracts",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "FacilityName",
                table: "BookingContracts",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "CustomerName",
                table: "BookingContracts",
                newName: "ContentType");

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "BookingContracts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
