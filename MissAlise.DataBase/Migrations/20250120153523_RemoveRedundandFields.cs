using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissAlise.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundandFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Folders_FolderItemid",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_FolderItemid",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Videoid",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "Photoid",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "Folderid",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "FolderItemid",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Audioid",
                table: "Audios");

            migrationBuilder.RenameColumn(
                name: "Fileid",
                table: "Files",
                newName: "Folderid");

            migrationBuilder.CreateIndex(
                name: "IX_Files_Folderid",
                table: "Files",
                column: "Folderid");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Folders_Folderid",
                table: "Files",
                column: "Folderid",
                principalTable: "Folders",
                principalColumn: "Itemid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Folders_Folderid",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_Folderid",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "Folderid",
                table: "Files",
                newName: "Fileid");

            migrationBuilder.AddColumn<int>(
                name: "Videoid",
                table: "Videos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Photoid",
                table: "Photos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Folderid",
                table: "Folders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FolderItemid",
                table: "Files",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Audioid",
                table: "Audios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderItemid",
                table: "Files",
                column: "FolderItemid");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Folders_FolderItemid",
                table: "Files",
                column: "FolderItemid",
                principalTable: "Folders",
                principalColumn: "Itemid");
        }
    }
}
