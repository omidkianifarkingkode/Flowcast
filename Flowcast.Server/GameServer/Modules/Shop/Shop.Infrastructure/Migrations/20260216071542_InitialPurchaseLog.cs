using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPurchaseLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Shop");

            migrationBuilder.CreateTable(
                name: "Purchases",
                schema: "Shop",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(2024)", maxLength: 2024, nullable: false),
                    Store = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PurchaseToken = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Signature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Receipt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchaseAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsSandbox = table.Column<bool>(type: "bit", nullable: false),
                    State = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUser = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifierUser = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseValidationAttempts",
                schema: "Shop",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseId = table.Column<string>(type: "varchar(40)", nullable: false),
                    AttemptNo = table.Column<int>(type: "int", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FinishedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseValidationAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseValidationAttempts_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalSchema: "Shop",
                        principalTable: "Purchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_Filter",
                schema: "Shop",
                table: "Purchases",
                columns: new[] { "Store", "Id", "State", "IsSandbox" });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_Id",
                schema: "Shop",
                table: "Purchases",
                column: "Id",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_Store_OrderId",
                schema: "Shop",
                table: "Purchases",
                columns: new[] { "Store", "OrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseValidationAttempts_PurchaseId",
                schema: "Shop",
                table: "PurchaseValidationAttempts",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseValidationAttempts_PurchaseId_AttemptNo",
                schema: "Shop",
                table: "PurchaseValidationAttempts",
                columns: new[] { "PurchaseId", "AttemptNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseValidationAttempts",
                schema: "Shop");

            migrationBuilder.DropTable(
                name: "Purchases",
                schema: "Shop");
        }
    }
}
