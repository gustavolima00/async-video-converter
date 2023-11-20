using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "raw_videos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    path = table.Column<string>(type: "text", nullable: false),
                    extract_subtitle_status = table.Column<string>(type: "varchar(255)", nullable: false),
                    extract_tracks_status = table.Column<string>(type: "varchar(255)", nullable: false),
                    user_uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_videos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "webhook_users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    webhook_url = table.Column<string>(type: "text", nullable: false),
                    events = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "converted_videos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    raw_video_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_converted_videos", x => x.id);
                    table.ForeignKey(
                        name: "FK_converted_videos_raw_videos_raw_video_id",
                        column: x => x.raw_video_id,
                        principalTable: "raw_videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "converted_subtitles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    converted_video_id = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false),
                    link = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_converted_subtitles", x => x.id);
                    table.ForeignKey(
                        name: "FK_converted_subtitles_converted_videos_converted_video_id",
                        column: x => x.converted_video_id,
                        principalTable: "converted_videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "converted_video_tracks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    converted_video_id = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false),
                    link = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_converted_video_tracks", x => x.id);
                    table.ForeignKey(
                        name: "FK_converted_video_tracks_converted_videos_converted_video_id",
                        column: x => x.converted_video_id,
                        principalTable: "converted_videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_converted_subtitles_converted_video_id",
                table: "converted_subtitles",
                column: "converted_video_id");

            migrationBuilder.CreateIndex(
                name: "IX_converted_video_tracks_converted_video_id",
                table: "converted_video_tracks",
                column: "converted_video_id");

            migrationBuilder.CreateIndex(
                name: "IX_converted_videos_raw_video_id",
                table: "converted_videos",
                column: "raw_video_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "converted_subtitles");

            migrationBuilder.DropTable(
                name: "converted_video_tracks");

            migrationBuilder.DropTable(
                name: "webhook_users");

            migrationBuilder.DropTable(
                name: "converted_videos");

            migrationBuilder.DropTable(
                name: "raw_videos");
        }
    }
}
