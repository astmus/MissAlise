using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissAlise.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class SizeTypeFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Folders_Parentfolderid",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "Mimetype",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Items",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Mimetype",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Folders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Folders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "Files",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Folders_Parentfolderid",
                table: "Folders",
                column: "Parentfolderid",
                principalTable: "Folders",
                principalColumn: "Itemid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Folders_Parentfolderid",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Items",
                newName: "Title");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Items",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Folders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Folders",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "Files",
                type: "integer",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mimetype",
                table: "Files",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Files",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Folders_Parentfolderid",
                table: "Folders",
                column: "Parentfolderid",
                principalTable: "Folders",
                principalColumn: "Itemid",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
