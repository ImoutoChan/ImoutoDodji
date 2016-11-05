using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations
{
    public partial class ParseStates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.RenameColumn(
            //    name: "FileSize",
            //    table: "Galleries",
            //    newName: "Size");

            migrationBuilder.Sql("PRAGMA foreign_keys = false;");
            migrationBuilder.CreateTable(
                name: "new_Galleries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CollectionId = table.Column<int>(nullable: false),
                    Size = table.Column<long>(nullable: false),
                    FilesCount = table.Column<int>(nullable: false),
                    Md5 = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Path = table.Column<string>(nullable: false),
                    PreviewPath = table.Column<string>(nullable: false),
                    StorageType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Galleries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Galleries_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.Sql("INSERT INTO new_Galleries SELECT * FROM Galleries");
            migrationBuilder.DropTable("Galleries");
            migrationBuilder.RenameTable("new_Galleries", newName: "Galleries");
            migrationBuilder.CreateIndex(
                name: "IX_Galleries_CollectionId",
                table: "Galleries",
                column: "CollectionId");
            migrationBuilder.Sql("PRAGMA foreign_keys = true;");



            migrationBuilder.AddColumn<bool>(
                name: "KeepRelativePath",
                table: "SourceFolders",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ObservationType",
                table: "SourceFolders",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Metadata",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlternativeNames = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    GalleryId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Source = table.Column<string>(nullable: true),
                    Translator = table.Column<string>(nullable: true),
                    ArchiveKey = table.Column<string>(nullable: true),
                    Category = table.Column<int>(nullable: true),
                    FileCount = table.Column<int>(nullable: true),
                    FileSize = table.Column<long>(nullable: true),
                    IsExpunged = table.Column<bool>(nullable: true),
                    PandaId = table.Column<int>(nullable: true),
                    PandaToken = table.Column<string>(nullable: true),
                    PostedDate = table.Column<DateTime>(nullable: true),
                    Rating = table.Column<double>(nullable: true),
                    ThumbUrl = table.Column<string>(nullable: true),
                    TorrentCount = table.Column<int>(nullable: true),
                    Uploader = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metadata_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParsingStates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateTimeCreated = table.Column<DateTime>(nullable: false),
                    DateTimeUpdated = table.Column<DateTime>(nullable: false),
                    GalleryId = table.Column<int>(nullable: false),
                    State = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsingStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParsingStates_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_GalleryId",
                table: "Metadata",
                column: "GalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsingStates_GalleryId",
                table: "ParsingStates",
                column: "GalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsingStates_State",
                table: "ParsingStates",
                column: "State");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Metadata");

            migrationBuilder.DropTable(
                name: "ParsingStates");

            migrationBuilder.DropColumn(
                name: "KeepRelativePath",
                table: "SourceFolders");

            migrationBuilder.DropColumn(
                name: "ObservationType",
                table: "SourceFolders");

            //migrationBuilder.RenameColumn(
            //    name: "Size",
            //    table: "Galleries",
            //    newName: "FileSize");

            migrationBuilder.Sql("PRAGMA foreign_keys = false;");
            migrationBuilder.CreateTable(
                name: "new_Galleries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CollectionId = table.Column<int>(nullable: false),
                    FileSize = table.Column<long>(nullable: false),
                    FilesCount = table.Column<int>(nullable: false),
                    Md5 = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Path = table.Column<string>(nullable: false),
                    PreviewPath = table.Column<string>(nullable: false),
                    StorageType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Galleries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Galleries_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.Sql("INSERT INTO new_Galleries SELECT * FROM Galleries");
            migrationBuilder.DropTable("Galleries");
            migrationBuilder.RenameTable("new_Galleries", newName: "Galleries");
            migrationBuilder.CreateIndex(
                name: "IX_Galleries_CollectionId",
                table: "Galleries",
                column: "CollectionId");
            migrationBuilder.Sql("PRAGMA foreign_keys = true;");
        }
    }
}
