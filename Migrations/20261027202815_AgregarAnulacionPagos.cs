using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SICOP.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAnulacionPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Anulado",
                table: "Pagos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "Pagos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                table: "Pagos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anulado",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                table: "Pagos");
        }
    }
}
