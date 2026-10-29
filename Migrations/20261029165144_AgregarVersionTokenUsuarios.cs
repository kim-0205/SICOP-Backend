
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SICOP.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVersionTokenUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agrega la versión del token a los usuarios.
            // Los usuarios existentes comenzarán con versión 1.
            migrationBuilder.AddColumn<int>(
                name: "VersionToken",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Elimina la columna si se revierte la migración.
            migrationBuilder.DropColumn(
                name: "VersionToken",
                table: "Usuarios");
        }
    }
}
