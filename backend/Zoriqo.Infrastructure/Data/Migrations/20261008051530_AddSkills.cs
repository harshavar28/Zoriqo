using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Zoriqo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "skills",
                schema: "zoriqo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_skills",
                schema: "zoriqo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_skills", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_skills_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalSchema: "zoriqo_identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_skills_skills_skill_id",
                        column: x => x.skill_id,
                        principalSchema: "zoriqo",
                        principalTable: "skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "zoriqo",
                table: "skills",
                columns: new[] { "id", "created_at_utc", "is_active", "name", "normalized_name", "updated_at_utc" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-4111-8111-111111111101"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), true, "Interior painting", "INTERIOR PAINTING", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-4111-8111-111111111102"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), true, "Wall finishing", "WALL FINISHING", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-4111-8111-111111111103"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), true, "Colour selection", "COLOUR SELECTION", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-4111-8111-111111111104"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), true, "Carpentry", "CARPENTRY", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-4111-8111-111111111105"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), true, "Plumbing", "PLUMBING", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-4111-8111-111111111106"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), true, "Electrical work", "ELECTRICAL WORK", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-4111-8111-111111111107"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), true, "Web development", "WEB DEVELOPMENT", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-4111-8111-111111111108"), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), true, "Graphic design", "GRAPHIC DESIGN", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "ux_skills_normalized_name",
                schema: "zoriqo",
                table: "skills",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_skills_active_skill_user",
                schema: "zoriqo",
                table: "user_skills",
                columns: new[] { "skill_id", "user_id" },
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ux_user_skills_user_skill",
                schema: "zoriqo",
                table: "user_skills",
                columns: new[] { "user_id", "skill_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_skills",
                schema: "zoriqo");

            migrationBuilder.DropTable(
                name: "skills",
                schema: "zoriqo");
        }
    }
}
