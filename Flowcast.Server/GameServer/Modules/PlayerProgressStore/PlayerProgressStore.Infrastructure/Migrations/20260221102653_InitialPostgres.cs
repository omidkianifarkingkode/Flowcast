using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerProgressStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
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
                    PlayerId = table.Column<string>(type: "text", nullable: false),
                    Namespace = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    Progress = table.Column<long>(type: "bigint", nullable: false),
                    Document = table.Column<byte[]>(type: "bytea", nullable: false),
                    Hash = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
