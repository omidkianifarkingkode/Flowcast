using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZP.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ZainPal");

            migrationBuilder.RenameTable(
                name: "PaymentRequests",
                newName: "PaymentRequests",
                newSchema: "ZainPal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "PaymentRequests",
                schema: "ZainPal",
                newName: "PaymentRequests");
        }
    }
}
