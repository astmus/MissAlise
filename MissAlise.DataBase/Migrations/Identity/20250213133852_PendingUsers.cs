using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissAlise.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class PendingUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Folder",
                columns: table => new
                {
                    Itemid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Parentfolderid = table.Column<int>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ModifieDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folder", x => x.Itemid);
                    table.ForeignKey(
                        name: "FK_Folder_Folder_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Folder",
                        principalColumn: "Itemid");
                });

            migrationBuilder.CreateTable(
                name: "File",
                columns: table => new
                {
                    Itemid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Caption = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: true),
                    Extension = table.Column<string>(type: "TEXT", nullable: true),
                    Folderid = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ModifieDateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_File", x => x.Itemid);
                    table.ForeignKey(
                        name: "FK_File_Folder_Folderid",
                        column: x => x.Folderid,
                        principalTable: "Folder",
                        principalColumn: "Itemid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PendingUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    GivenName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Mail = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    PreferredLanguage = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    Surname = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserPrincipalName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    storagefolderid = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingUsers_Folder_storagefolderid",
                        column: x => x.storagefolderid,
                        principalTable: "Folder",
                        principalColumn: "Itemid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_File_Folderid",
                table: "File",
                column: "Folderid");

            migrationBuilder.CreateIndex(
                name: "IX_Folder_ParentId",
                table: "Folder",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingUsers_storagefolderid",
                table: "PendingUsers",
                column: "storagefolderid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "File");

            migrationBuilder.DropTable(
                name: "PendingUsers");

            migrationBuilder.DropTable(
                name: "Folder");
        }
    }
}
