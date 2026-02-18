using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitCreateShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Store = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PurchaseToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Signature = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProductId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Receipt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchaseAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsSandbox = table.Column<bool>(type: "bit", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Meta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseValidationAttempts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseId = table.Column<string>(type: "varchar(40)", nullable: false),
                    AttemptNo = table.Column<int>(type: "int", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FinishedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseValidationAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseValidationAttempts_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_Filter",
                table: "Purchases",
                columns: new[] { "Store", "Id", "State", "IsSandbox" });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_Id",
                table: "Purchases",
                column: "Id",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_Store_OrderId",
                table: "Purchases",
                columns: new[] { "Store", "OrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseValidationAttempts_PurchaseId",
                table: "PurchaseValidationAttempts",
                column: "PurchaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseValidationAttempts");

            migrationBuilder.DropTable(
                name: "Purchases");
        }
    }
}
