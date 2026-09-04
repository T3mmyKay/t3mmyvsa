using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace T3mmyvsa.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260904020000_AddUserIsActive")]
public partial class AddUserIsActive : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "AspNetUsers",
            type: "bit",
            nullable: false,
            defaultValue: true);

        // PR #1 represented administrative deactivation with a permanent Identity lockout.
        // Preserve that state while separating it from real authentication lockouts.
        migrationBuilder.Sql(
            """
            UPDATE [AspNetUsers]
            SET [IsActive] = CASE
                WHEN [LockoutEnd] >= '9998-01-01T00:00:00+00:00' THEN CAST(0 AS bit)
                ELSE CAST(1 AS bit)
            END;

            UPDATE [AspNetUsers]
            SET [LockoutEnd] = NULL,
                [AccessFailedCount] = 0
            WHERE [IsActive] = 0
              AND [LockoutEnd] >= '9998-01-01T00:00:00+00:00';

            UPDATE [AspNetUsers]
            SET [LockoutEnabled] = CAST(1 AS bit);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Restore the previous administrative-deactivation representation for rollback.
        migrationBuilder.Sql(
            """
            UPDATE [AspNetUsers]
            SET [LockoutEnabled] = CAST(1 AS bit),
                [LockoutEnd] = '9999-12-31T23:59:59.9999999+00:00'
            WHERE [IsActive] = 0;
            """);

        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "AspNetUsers");
    }
}
