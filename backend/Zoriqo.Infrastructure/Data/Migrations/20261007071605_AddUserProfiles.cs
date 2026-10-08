using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zoriqo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "zoriqo");

            migrationBuilder.CreateTable(
                name: "user_profiles",
                schema: "zoriqo",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    headline = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    about = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_profiles_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalSchema: "zoriqo_identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.Sql(
    """
            INSERT INTO zoriqo.user_profiles
                (user_id, created_at_utc, updated_at_utc, version)
            SELECT
                u."Id",
                u."CreatedAtUtc",
                NOW(),
                1
            FROM zoriqo_identity."AspNetUsers" AS u
            ON CONFLICT (user_id) DO NOTHING;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_profiles",
                schema: "zoriqo");
        }
    }
}
