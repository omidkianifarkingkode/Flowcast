using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZP.Core.Migrations
{
    public partial class AddIdempotencyKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "PaymentRequests",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_IdempotencyKey",
                table: "PaymentRequests",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_IdempotencyKey",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "PaymentRequests");
        }
    }
}
