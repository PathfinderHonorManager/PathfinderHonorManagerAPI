using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PathfinderHonorManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchemaWithProperDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    sequence_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "club",
                columns: table => new
                {
                    club_id = table.Column<Guid>(type: "uuid", nullable: false),
                    club_code = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_club", x => x.club_id);
                });

            migrationBuilder.CreateTable(
                name: "honor",
                columns: table => new
                {
                    honor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    patch_path = table.Column<string>(type: "text", nullable: true),
                    wiki_path = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_honor", x => x.honor_id);
                });

            migrationBuilder.CreateTable(
                name: "pathfinder_class",
                columns: table => new
                {
                    grade = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pathfinder_class", x => x.grade);
                });

            migrationBuilder.CreateTable(
                name: "pathfinder_honor_status",
                columns: table => new
                {
                    status_code = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pathfinder_honor_status", x => x.status_code);
                });

            migrationBuilder.CreateTable(
                name: "achievement",
                columns: table => new
                {
                    achievement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    sequence_order = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_achievement", x => x.achievement_id);
                    table.ForeignKey(
                        name: "FK_achievement_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_achievement_pathfinder_class_grade",
                        column: x => x.grade,
                        principalTable: "pathfinder_class",
                        principalColumn: "grade",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pathfinder",
                columns: table => new
                {
                    pathfinder_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    grade = table.Column<int>(type: "integer", nullable: true),
                    club_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: true),
                    create_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pathfinder", x => x.pathfinder_id);
                    table.ForeignKey(
                        name: "FK_pathfinder_club_club_id",
                        column: x => x.club_id,
                        principalTable: "club",
                        principalColumn: "club_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pathfinder_pathfinder_class_grade",
                        column: x => x.grade,
                        principalTable: "pathfinder_class",
                        principalColumn: "grade",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pathfinder_achievement",
                columns: table => new
                {
                    pathfinder_achievement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pathfinder_id = table.Column<Guid>(type: "uuid", nullable: false),
                    achievement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_achieved = table.Column<bool>(type: "boolean", nullable: false),
                    create_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    achieve_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pathfinder_achievement", x => x.pathfinder_achievement_id);
                    table.ForeignKey(
                        name: "FK_pathfinder_achievement_achievement_achievement_id",
                        column: x => x.achievement_id,
                        principalTable: "achievement",
                        principalColumn: "achievement_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pathfinder_achievement_pathfinder_pathfinder_id",
                        column: x => x.pathfinder_id,
                        principalTable: "pathfinder",
                        principalColumn: "pathfinder_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pathfinder_honor",
                columns: table => new
                {
                    pathfinder_honor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    honor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_code = table.Column<int>(type: "integer", nullable: false),
                    create_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    earn_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pathfinder_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pathfinder_honor", x => x.pathfinder_honor_id);
                    table.ForeignKey(
                        name: "FK_pathfinder_honor_honor_honor_id",
                        column: x => x.honor_id,
                        principalTable: "honor",
                        principalColumn: "honor_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pathfinder_honor_pathfinder_honor_status_status_code",
                        column: x => x.status_code,
                        principalTable: "pathfinder_honor_status",
                        principalColumn: "status_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pathfinder_honor_pathfinder_pathfinder_id",
                        column: x => x.pathfinder_id,
                        principalTable: "pathfinder",
                        principalColumn: "pathfinder_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_achievement_category_id",
                table: "achievement",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_achievement_grade",
                table: "achievement",
                column: "grade");

            migrationBuilder.CreateIndex(
                name: "IX_pathfinder_club_id",
                table: "pathfinder",
                column: "club_id");

            migrationBuilder.CreateIndex(
                name: "IX_pathfinder_email",
                table: "pathfinder",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pathfinder_grade",
                table: "pathfinder",
                column: "grade");

            migrationBuilder.CreateIndex(
                name: "IX_pathfinder_achievement_achievement_id",
                table: "pathfinder_achievement",
                column: "achievement_id");

            migrationBuilder.CreateIndex(
                name: "IX_pathfinder_achievement_pathfinder_id",
                table: "pathfinder_achievement",
                column: "pathfinder_id");

            migrationBuilder.CreateIndex(
                name: "IX_pathfinder_honor_honor_id",
                table: "pathfinder_honor",
                column: "honor_id");

            migrationBuilder.CreateIndex(
                name: "IX_pathfinder_honor_pathfinder_id",
                table: "pathfinder_honor",
                column: "pathfinder_id");

            migrationBuilder.CreateIndex(
                name: "IX_pathfinder_honor_status_code",
                table: "pathfinder_honor",
                column: "status_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pathfinder_achievement");

            migrationBuilder.DropTable(
                name: "pathfinder_honor");

            migrationBuilder.DropTable(
                name: "achievement");

            migrationBuilder.DropTable(
                name: "honor");

            migrationBuilder.DropTable(
                name: "pathfinder_honor_status");

            migrationBuilder.DropTable(
                name: "pathfinder");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "club");

            migrationBuilder.DropTable(
                name: "pathfinder_class");
        }
    }
}
