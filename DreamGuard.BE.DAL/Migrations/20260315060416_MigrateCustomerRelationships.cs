using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MigrateCustomerRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Users_UserId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_BabyProfiles_Users_UserId",
                table: "BabyProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Users_UserId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteProducts_Users_UserId",
                table: "FavoriteProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVouchers_Users_UserId",
                table: "UserVouchers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserVouchers",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_UserVouchers_UserId",
                table: "UserVouchers",
                newName: "IX_UserVouchers_CustomerId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Orders",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                newName: "IX_Orders_CustomerId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "FavoriteProducts",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteProducts_UserId_ProductId",
                table: "FavoriteProducts",
                newName: "IX_FavoriteProducts_CustomerId_ProductId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Carts",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                newName: "IX_Carts_CustomerId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "BabyProfiles",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_BabyProfiles_UserId",
                table: "BabyProfiles",
                newName: "IX_BabyProfiles_CustomerId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Addresses",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses",
                newName: "IX_Addresses_CustomerId");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_Customers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServicePackages",
                columns: table => new
                {
                    ServicePackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    SuitableFor = table.Column<string>(type: "text", nullable: false),
                    ServiceContent = table.Column<string>(type: "text", nullable: false),
                    Benefits = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackages", x => x.ServicePackageId);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EstimatedDuration = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staffs", x => x.StaffId);
                    table.ForeignKey(
                        name: "FK_Staffs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ServicePackageMappings",
                columns: table => new
                {
                    ServicePackageMappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicePackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackageMappings", x => x.ServicePackageMappingId);
                    table.ForeignKey(
                        name: "FK_ServicePackageMappings_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "ServicePackageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicePackageMappings_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceOrders",
                columns: table => new
                {
                    SoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicePackageMappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CustomNote = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOrders", x => x.SoId);
                    table.ForeignKey(
                        name: "FK_ServiceOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceOrders_ServicePackageMappings_ServicePackageMappingId",
                        column: x => x.ServicePackageMappingId,
                        principalTable: "ServicePackageMappings",
                        principalColumn: "ServicePackageMappingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceOrders_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId");
                });

            migrationBuilder.CreateTable(
                name: "ServiceTasks",
                columns: table => new
                {
                    ServiceTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    SoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTasks", x => x.ServiceTaskId);
                    table.ForeignKey(
                        name: "FK_ServiceTasks_ServiceOrders_SoId",
                        column: x => x.SoId,
                        principalTable: "ServiceOrders",
                        principalColumn: "SoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceTasks_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceEvidences",
                columns: table => new
                {
                    SeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    EvidenceType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceEvidences", x => x.SeId);
                    table.ForeignKey(
                        name: "FK_ServiceEvidences_ServiceTasks_ServiceTaskId",
                        column: x => x.ServiceTaskId,
                        principalTable: "ServiceTasks",
                        principalColumn: "ServiceTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                table: "Customers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAssets_ServiceId",
                table: "ServiceAssets",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEvidences_ServiceTaskId",
                table: "ServiceEvidences",
                column: "ServiceTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_CustomerId",
                table: "ServiceOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_ServiceId",
                table: "ServiceOrders",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_ServicePackageMappingId",
                table: "ServiceOrders",
                column: "ServicePackageMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackageMappings_ServiceId",
                table: "ServicePackageMappings",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackageMappings_ServicePackageId",
                table: "ServicePackageMappings",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTasks_SoId",
                table: "ServiceTasks",
                column: "SoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTasks_StaffId",
                table: "ServiceTasks",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_UserId",
                table: "Staffs",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Customers_CustomerId",
                table: "Addresses",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BabyProfiles_Customers_CustomerId",
                table: "BabyProfiles",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Customers_CustomerId",
                table: "Carts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteProducts_Customers_CustomerId",
                table: "FavoriteProducts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVouchers_Customers_CustomerId",
                table: "UserVouchers",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Customers_CustomerId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_BabyProfiles_Customers_CustomerId",
                table: "BabyProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Customers_CustomerId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteProducts_Customers_CustomerId",
                table: "FavoriteProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVouchers_Customers_CustomerId",
                table: "UserVouchers");

            migrationBuilder.DropTable(
                name: "ServiceAssets");

            migrationBuilder.DropTable(
                name: "ServiceEvidences");

            migrationBuilder.DropTable(
                name: "ServiceTasks");

            migrationBuilder.DropTable(
                name: "ServiceOrders");

            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "ServicePackageMappings");

            migrationBuilder.DropTable(
                name: "ServicePackages");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "UserVouchers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserVouchers_CustomerId",
                table: "UserVouchers",
                newName: "IX_UserVouchers_UserId");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Orders",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                newName: "IX_Orders_UserId");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "FavoriteProducts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteProducts_CustomerId_ProductId",
                table: "FavoriteProducts",
                newName: "IX_FavoriteProducts_UserId_ProductId");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Carts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_CustomerId",
                table: "Carts",
                newName: "IX_Carts_UserId");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "BabyProfiles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_BabyProfiles_CustomerId",
                table: "BabyProfiles",
                newName: "IX_BabyProfiles_UserId");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Addresses",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Addresses_CustomerId",
                table: "Addresses",
                newName: "IX_Addresses_UserId");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Users",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Users_UserId",
                table: "Addresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BabyProfiles_Users_UserId",
                table: "BabyProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Users_UserId",
                table: "Carts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteProducts_Users_UserId",
                table: "FavoriteProducts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVouchers_Users_UserId",
                table: "UserVouchers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
