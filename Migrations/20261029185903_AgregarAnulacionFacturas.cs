using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SICOP.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAnulacionFacturas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Anulada",
                table: "FacturasCompra",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AnuladaPorUsuarioId",
                table: "FacturasCompra",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "FacturasCompra",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                table: "FacturasCompra",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anulada",
                table: "FacturasCompra");

            migrationBuilder.DropColumn(
                name: "AnuladaPorUsuarioId",
                table: "FacturasCompra");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "FacturasCompra");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                table: "FacturasCompra");
        }
    }
}
