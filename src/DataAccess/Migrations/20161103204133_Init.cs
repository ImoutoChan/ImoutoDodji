using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Namespaces",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Namespaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DestinationFolders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CollectionId = table.Column<int>(nullable: false),
                    Path = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DestinationFolders_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Galleries",
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

            migrationBuilder.CreateTable(
                name: "SourceFolders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CollectionId = table.Column<int>(nullable: false),
                    Path = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SourceFolders_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BindedTags",
                columns: table => new
                {
                    TagId = table.Column<int>(nullable: false),
                    GalleryId = table.Column<int>(nullable: false),
                    NamespaceId = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BindedTags", x => new { x.TagId, x.GalleryId, x.NamespaceId });
                    table.ForeignKey(
                        name: "FK_BindedTags_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BindedTags_Namespaces_NamespaceId",
                        column: x => x.NamespaceId,
                        principalTable: "Namespaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BindedTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BindedTags_GalleryId",
                table: "BindedTags",
                column: "GalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_BindedTags_NamespaceId",
                table: "BindedTags",
                column: "NamespaceId");

            migrationBuilder.CreateIndex(
                name: "IX_BindedTags_TagId",
                table: "BindedTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_BindedTags_GalleryId_NamespaceId",
                table: "BindedTags",
                columns: new[] { "GalleryId", "NamespaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_BindedTags_TagId_NamespaceId",
                table: "BindedTags",
                columns: new[] { "TagId", "NamespaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_DestinationFolders_CollectionId",
                table: "DestinationFolders",
                column: "CollectionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_CollectionId",
                table: "Galleries",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Namespaces_Name",
                table: "Namespaces",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SourceFolders_CollectionId",
                table: "SourceFolders",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BindedTags");

            migrationBuilder.DropTable(
                name: "DestinationFolders");

            migrationBuilder.DropTable(
                name: "SourceFolders");

            migrationBuilder.DropTable(
                name: "Galleries");

            migrationBuilder.DropTable(
                name: "Namespaces");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Collections");
        }
    }
}
