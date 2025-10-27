using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerProgressStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPlayerProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PlayerProgress");

            migrationBuilder.CreateTable(
                name: "PlayerNamespaces",
                schema: "PlayerProgress",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Namespace = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Progress = table.Column<long>(type: "bigint", nullable: false),
                    Document = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Hash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerNamespaces", x => new { x.PlayerId, x.Namespace });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerNamespaces",
                schema: "PlayerProgress");
        }
    }
}
