using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForComboSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Combos_Slug",
                table: "Combos",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Combos_Slug",
                table: "Combos");
        }
    }
}
