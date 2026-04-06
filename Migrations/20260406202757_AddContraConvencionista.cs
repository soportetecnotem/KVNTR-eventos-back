using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventosBack.Migrations
{
    /// <inheritdoc />
    public partial class AddContraConvencionista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contrasena",
                table: "Convencionistas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contrasena",
                table: "Convencionistas");
        }
    }
}
