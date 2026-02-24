using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialFieldToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Material",
                table: "Products");
        }
    }
}
