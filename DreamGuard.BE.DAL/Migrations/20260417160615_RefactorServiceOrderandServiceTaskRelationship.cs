using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RefactorServiceOrderandServiceTaskRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceTasks_SoId",
                table: "ServiceTasks");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ServiceTasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTasks_SoId",
                table: "ServiceTasks",
                column: "SoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceTasks_SoId",
                table: "ServiceTasks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ServiceTasks");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTasks_SoId",
                table: "ServiceTasks",
                column: "SoId",
                unique: true);
        }
    }
}
