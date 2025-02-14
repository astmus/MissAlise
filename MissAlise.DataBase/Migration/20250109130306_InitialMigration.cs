using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MissAlise.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Itemid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: true),
                    Createddatetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Modifiedatetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Itemid);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Itemid = table.Column<int>(type: "integer", nullable: false),
                    Folderid = table.Column<int>(type: "integer", nullable: false),
                    Parentfolderid = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Path = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Itemid);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_Parentfolderid",
                        column: x => x.Parentfolderid,
                        principalTable: "Folders",
                        principalColumn: "Itemid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Folders_Items_Itemid",
                        column: x => x.Itemid,
                        principalTable: "Items",
                        principalColumn: "Itemid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Itemid = table.Column<int>(type: "integer", nullable: false),
                    Fileid = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Size = table.Column<int>(type: "integer", nullable: true),
                    Mimetype = table.Column<string>(type: "text", nullable: true),
                    Extension = table.Column<string>(type: "text", nullable: true),
                    FolderItemid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Itemid);
                    table.ForeignKey(
                        name: "FK_Files_Folders_FolderItemid",
                        column: x => x.FolderItemid,
                        principalTable: "Folders",
                        principalColumn: "Itemid");
                    table.ForeignKey(
                        name: "FK_Files_Items_Itemid",
                        column: x => x.Itemid,
                        principalTable: "Items",
                        principalColumn: "Itemid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GivenName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Mail = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PreferredLanguage = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Surname = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UserPrincipalName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageFolderItemid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Folders_StorageFolderItemid",
                        column: x => x.StorageFolderItemid,
                        principalTable: "Folders",
                        principalColumn: "Itemid");
                });

            migrationBuilder.CreateTable(
                name: "Audios",
                columns: table => new
                {
                    Itemid = table.Column<int>(type: "integer", nullable: false),
                    Audioid = table.Column<int>(type: "integer", nullable: false),
                    TrackTitle = table.Column<string>(type: "text", nullable: true),
                    Track = table.Column<int>(type: "integer", nullable: true),
                    Duration = table.Column<long>(type: "bigint", nullable: true),
                    Genre = table.Column<string>(type: "text", nullable: true),
                    Bitrate = table.Column<long>(type: "bigint", nullable: true),
                    Album = table.Column<string>(type: "text", nullable: true),
                    Artist = table.Column<string>(type: "text", nullable: true),
                    Disc = table.Column<int>(type: "integer", nullable: true),
                    Trackcount = table.Column<int>(type: "integer", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audios", x => x.Itemid);
                    table.ForeignKey(
                        name: "FK_Audios_Files_Itemid",
                        column: x => x.Itemid,
                        principalTable: "Files",
                        principalColumn: "Itemid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Itemid = table.Column<int>(type: "integer", nullable: false),
                    Photoid = table.Column<int>(type: "integer", nullable: false),
                    Cameramake = table.Column<string>(type: "text", nullable: true),
                    Cameramodel = table.Column<string>(type: "text", nullable: true),
                    Exposuredenominator = table.Column<double>(type: "double precision", nullable: true),
                    Exposurenumerator = table.Column<double>(type: "double precision", nullable: true),
                    Fnumber = table.Column<double>(type: "double precision", nullable: true),
                    Focallength = table.Column<double>(type: "double precision", nullable: true),
                    Iso = table.Column<int>(type: "integer", nullable: true),
                    Orientation = table.Column<int>(type: "integer", nullable: true),
                    Takendatetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Itemid);
                    table.ForeignKey(
                        name: "FK_Photos_Files_Itemid",
                        column: x => x.Itemid,
                        principalTable: "Files",
                        principalColumn: "Itemid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Itemid = table.Column<int>(type: "integer", nullable: false),
                    Videoid = table.Column<int>(type: "integer", nullable: false),
                    Audiobitspersample = table.Column<int>(type: "integer", nullable: true),
                    Audiochannels = table.Column<int>(type: "integer", nullable: true),
                    Audioformat = table.Column<string>(type: "text", nullable: true),
                    Audiosamplespersecond = table.Column<int>(type: "integer", nullable: true),
                    Bitrate = table.Column<int>(type: "integer", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Fourcc = table.Column<string>(type: "text", nullable: true),
                    Framerate = table.Column<double>(type: "double precision", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Itemid);
                    table.ForeignKey(
                        name: "FK_Videos_Files_Itemid",
                        column: x => x.Itemid,
                        principalTable: "Files",
                        principalColumn: "Itemid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderItemid",
                table: "Files",
                column: "FolderItemid");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_Parentfolderid",
                table: "Folders",
                column: "Parentfolderid");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StorageFolderItemid",
                table: "Users",
                column: "StorageFolderItemid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audios");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
