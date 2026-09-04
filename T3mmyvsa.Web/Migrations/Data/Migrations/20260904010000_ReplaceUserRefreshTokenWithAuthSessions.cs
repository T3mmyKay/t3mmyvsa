using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using T3mmyvsa.Data;

#nullable disable

namespace T3mmyvsa.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260904010000_ReplaceUserRefreshTokenWithAuthSessions")]
public partial class ReplaceUserRefreshTokenWithAuthSessions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "RefreshToken", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "RefreshTokenExpiryTime", table: "AspNetUsers");

        migrationBuilder.CreateTable(
            name: "AuthSessions",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                UserId = table.Column<string>(maxLength: 450, nullable: false),
                RefreshTokenHash = table.Column<string>(maxLength: 88, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(nullable: false),
                LastUsedAt = table.Column<DateTimeOffset>(nullable: true),
                RevokedAt = table.Column<DateTimeOffset>(nullable: true),
                ReplacedBySessionId = table.Column<Guid>(nullable: true),
                IpAddress = table.Column<string>(maxLength: 64, nullable: true),
                UserAgent = table.Column<string>(maxLength: 512, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuthSessions", x => x.Id);
                table.ForeignKey(
                    name: "FK_AuthSessions_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AuthSessions_RefreshTokenHash",
            table: "AuthSessions",
            column: "RefreshTokenHash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AuthSessions_UserId_RevokedAt",
            table: "AuthSessions",
            columns: new[] { "UserId", "RevokedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AuthSessions");

        migrationBuilder.AddColumn<string>(
            name: "RefreshToken",
            table: "AspNetUsers",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "RefreshTokenExpiryTime",
            table: "AspNetUsers",
            nullable: true);
    }
}
