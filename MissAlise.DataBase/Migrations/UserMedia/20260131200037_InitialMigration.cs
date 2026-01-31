using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissAlise.DataBase.Migrations.UserMedia
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<byte>(type: "smallint", nullable: false),
                    Provider = table.Column<byte>(type: "smallint", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RemoteItemId = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DriveId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ParentPath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    MimeType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TakenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    DurationSeconds = table.Column<double>(type: "double precision", nullable: true),
                    HashAlgorithm = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    HashValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaLibraries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<byte>(type: "smallint", nullable: false),
                    DeltaToken = table.Column<string>(type: "text", nullable: true),
                    LastSyncStartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSyncFinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSyncStatus = table.Column<int>(type: "integer", nullable: false),
                    LastSyncError = table.Column<string>(type: "text", nullable: true),
                    LastFullSyncAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastDeltaSyncAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaLibraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Audios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TrackTitle = table.Column<string>(type: "varchar(256)", nullable: true),
                    Track = table.Column<int>(type: "integer", nullable: true),
                    Genre = table.Column<string>(type: "varchar(64)", nullable: true),
                    Bitrate = table.Column<long>(type: "bigint", nullable: true),
                    Album = table.Column<string>(type: "varchar(256)", nullable: true),
                    Artist = table.Column<string>(type: "varchar(256)", nullable: true),
                    Disc = table.Column<int>(type: "integer", nullable: true),
                    TrackCount = table.Column<int>(type: "integer", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audios_MediaItems_Id",
                        column: x => x.Id,
                        principalTable: "MediaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Cameramake = table.Column<string>(type: "text", nullable: true),
                    Cameramodel = table.Column<string>(type: "text", nullable: true),
                    Exposuredenominator = table.Column<double>(type: "double precision", nullable: true),
                    Exposurenumerator = table.Column<double>(type: "double precision", nullable: true),
                    Fnumber = table.Column<double>(type: "double precision", nullable: true),
                    Focallength = table.Column<double>(type: "double precision", nullable: true),
                    Iso = table.Column<int>(type: "integer", nullable: true),
                    Orientation = table.Column<int>(type: "integer", nullable: true),
                    Takendatetime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_MediaItems_Id",
                        column: x => x.Id,
                        principalTable: "MediaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Audiobitspersample = table.Column<int>(type: "integer", nullable: true),
                    Audiochannels = table.Column<int>(type: "integer", nullable: true),
                    Audioformat = table.Column<string>(type: "varchar(64)", nullable: true),
                    Audiosamplespersecond = table.Column<int>(type: "integer", nullable: true),
                    Bitrate = table.Column<int>(type: "integer", nullable: true),
                    Fourcc = table.Column<string>(type: "varchar(16)", nullable: true),
                    Framerate = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Videos_MediaItems_Id",
                        column: x => x.Id,
                        principalTable: "MediaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaItems_Kind_IsDeleted_ModifiedAt",
                table: "MediaItems",
                columns: new[] { "Kind", "IsDeleted", "ModifiedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaItems_Kind_IsDeleted_TakenAt",
                table: "MediaItems",
                columns: new[] { "Kind", "IsDeleted", "TakenAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaItems_Provider_RemoteItemId_DriveId_OwnerId",
                table: "MediaItems",
                columns: new[] { "Provider", "RemoteItemId", "DriveId", "OwnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaLibraries_OwnerId_Provider",
                table: "MediaLibraries",
                columns: new[] { "OwnerId", "Provider" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audios");

            migrationBuilder.DropTable(
                name: "MediaLibraries");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "MediaItems");
        }
    }
}
