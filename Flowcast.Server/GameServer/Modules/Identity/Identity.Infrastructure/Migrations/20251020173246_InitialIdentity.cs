using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "Identity",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LastLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginRegion = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "IdentityLoginAudits",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Region = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    DeviceOs = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DeviceModel = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DeviceLanguage = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    AppVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    TzOffsetMinutes = table.Column<int>(type: "int", nullable: true),
                    ClientTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityLoginAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SigningKeys",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeyId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Algorithm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PublicKeyPem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivateKeyPem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotBeforeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SigningKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Identities",
                schema: "Identity",
                columns: table => new
                {
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LoginAllowed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastMeta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identities", x => x.IdentityId);
                    table.ForeignKey(
                        name: "FK_Identities_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "Identity",
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Identities_Accounts_AccountId1",
                        column: x => x.AccountId1,
                        principalSchema: "Identity",
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Identities_AccountId",
                schema: "Identity",
                table: "Identities",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Identities_AccountId1",
                schema: "Identity",
                table: "Identities",
                column: "AccountId1");

            migrationBuilder.CreateIndex(
                name: "IX_Identities_Provider_Subject",
                schema: "Identity",
                table: "Identities",
                columns: new[] { "Provider", "Subject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityLoginAudits_AccountId_LoginAtUtc",
                schema: "Identity",
                table: "IdentityLoginAudits",
                columns: new[] { "AccountId", "LoginAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SigningKeys_IsActive",
                schema: "Identity",
                table: "SigningKeys",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SigningKeys_KeyId",
                schema: "Identity",
                table: "SigningKeys",
                column: "KeyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SigningKeys_NotBeforeUtc_ExpiresAtUtc",
                schema: "Identity",
                table: "SigningKeys",
                columns: new[] { "NotBeforeUtc", "ExpiresAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Identities",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "IdentityLoginAudits",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "SigningKeys",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "Identity");
        }
    }
}
