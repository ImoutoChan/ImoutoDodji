using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations
{
    public partial class ParsingGallery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParsedGalleries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArchiverKey = table.Column<string>(nullable: true),
                    Category = table.Column<int>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    FileCount = table.Column<int>(nullable: false),
                    FileSize = table.Column<long>(nullable: false),
                    Fjord = table.Column<bool>(nullable: false),
                    GalleryId = table.Column<int>(nullable: false),
                    IsExpunged = table.Column<bool>(nullable: false),
                    ParsingStateId = table.Column<int>(nullable: false),
                    PostedDate = table.Column<DateTime>(nullable: false),
                    Rating = table.Column<double>(nullable: false),
                    Source = table.Column<int>(nullable: false),
                    TagStrings = table.Column<string>(nullable: false),
                    Thumb = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    TitleJpn = table.Column<string>(nullable: false),
                    Token = table.Column<string>(nullable: true),
                    Torrentcount = table.Column<int>(nullable: false),
                    Uploader = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedGalleries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParsedGalleries_ParsingStates_ParsingStateId",
                        column: x => x.ParsingStateId,
                        principalTable: "ParsingStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SearchResults",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(nullable: true),
                    GalleryId = table.Column<int>(nullable: false),
                    IsSelected = table.Column<bool>(nullable: false),
                    ParsingStateId = table.Column<int>(nullable: false),
                    PreviewUrl = table.Column<string>(nullable: true),
                    Source = table.Column<int>(nullable: false),
                    Token = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchResults_ParsingStates_ParsingStateId",
                        column: x => x.ParsingStateId,
                        principalTable: "ParsingStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParsedGalleries_ParsingStateId",
                table: "ParsedGalleries",
                column: "ParsingStateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchResults_ParsingStateId",
                table: "SearchResults",
                column: "ParsingStateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParsedGalleries");

            migrationBuilder.DropTable(
                name: "SearchResults");
        }
    }
}
