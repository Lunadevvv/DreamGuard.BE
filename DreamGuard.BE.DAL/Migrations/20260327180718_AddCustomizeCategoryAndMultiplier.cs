using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomizeCategoryAndMultiplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "OverridePrice",
                table: "VariantCustomizeTypes",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddColumn<double>(
                name: "OverrideMultiplier",
                table: "VariantCustomizeTypes",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CalculationMode",
                table: "ProductCustomizeTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "ProductCustomizeTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "DefaultMultiplier",
                table: "ProductCustomizeTypes",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverrideMultiplier",
                table: "VariantCustomizeTypes");

            migrationBuilder.DropColumn(
                name: "CalculationMode",
                table: "ProductCustomizeTypes");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ProductCustomizeTypes");

            migrationBuilder.DropColumn(
                name: "DefaultMultiplier",
                table: "ProductCustomizeTypes");

            migrationBuilder.AlterColumn<decimal>(
                name: "OverridePrice",
                table: "VariantCustomizeTypes",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);
        }
    }
}
