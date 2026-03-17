using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddComboToFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FavoriteProducts_CustomerId_ProductId",
                table: "FavoriteProducts");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "FavoriteProducts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ComboId",
                table: "FavoriteProducts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteProducts_ComboId",
                table: "FavoriteProducts",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteProducts_CustomerId_ComboId",
                table: "FavoriteProducts",
                columns: new[] { "CustomerId", "ComboId" },
                unique: true,
                filter: "\"ComboId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteProducts_CustomerId_ProductId",
                table: "FavoriteProducts",
                columns: new[] { "CustomerId", "ProductId" },
                unique: true,
                filter: "\"ProductId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FavoriteProduct_ProductOrCombo",
                table: "FavoriteProducts",
                sql: "(\"ProductId\" IS NOT NULL AND \"ComboId\" IS NULL) OR (\"ProductId\" IS NULL AND \"ComboId\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteProducts_Combos_ComboId",
                table: "FavoriteProducts",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteProducts_Combos_ComboId",
                table: "FavoriteProducts");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteProducts_ComboId",
                table: "FavoriteProducts");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteProducts_CustomerId_ComboId",
                table: "FavoriteProducts");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteProducts_CustomerId_ProductId",
                table: "FavoriteProducts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FavoriteProduct_ProductOrCombo",
                table: "FavoriteProducts");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "FavoriteProducts");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "FavoriteProducts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteProducts_CustomerId_ProductId",
                table: "FavoriteProducts",
                columns: new[] { "CustomerId", "ProductId" },
                unique: true);
        }
    }
}
