using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MigrateServiceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_Services_ServiceId",
                table: "ServiceOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ServicePackageMappings_Services_ServiceId",
                table: "ServicePackageMappings");

            migrationBuilder.DropTable(
                name: "ServiceAssets");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_ServiceId",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "ServiceOrders");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "ServicePackageMappings",
                newName: "ProductTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ServicePackageMappings_ServiceId",
                table: "ServicePackageMappings",
                newName: "IX_ServicePackageMappings_ProductTypeId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckOut",
                table: "ServiceTasks",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckIn",
                table: "ServiceTasks",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "StaffNote",
                table: "ServiceTasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "ServicePackages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "ServiceOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "ServiceEvidences",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SoId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductTypes",
                columns: table => new
                {
                    ProductTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductTypeName = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => x.ProductTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SoId",
                table: "Payments",
                column: "SoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ServiceOrders_SoId",
                table: "Payments",
                column: "SoId",
                principalTable: "ServiceOrders",
                principalColumn: "SoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePackageMappings_ProductTypes_ProductTypeId",
                table: "ServicePackageMappings",
                column: "ProductTypeId",
                principalTable: "ProductTypes",
                principalColumn: "ProductTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ServiceOrders_SoId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_ServicePackageMappings_ProductTypes_ProductTypeId",
                table: "ServicePackageMappings");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SoId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StaffNote",
                table: "ServiceTasks");

            migrationBuilder.DropColumn(
                name: "status",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "ReceiverName",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "ServiceEvidences");

            migrationBuilder.DropColumn(
                name: "SoId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "ProductTypeId",
                table: "ServicePackageMappings",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ServicePackageMappings_ProductTypeId",
                table: "ServicePackageMappings",
                newName: "IX_ServicePackageMappings_ServiceId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckOut",
                table: "ServiceTasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckIn",
                table: "ServiceTasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ServicePackages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ServiceOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceId",
                table: "ServiceOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EstimatedDuration = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceAssets_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_ServiceId",
                table: "ServiceOrders",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAssets_ServiceId",
                table: "ServiceAssets",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_Services_ServiceId",
                table: "ServiceOrders",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePackageMappings_Services_ServiceId",
                table: "ServicePackageMappings",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
