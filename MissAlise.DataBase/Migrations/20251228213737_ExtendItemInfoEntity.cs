using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissAlise.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class ExtendItemInfoEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Files");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModifiedDateTime",
                table: "ItemInfo",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ItemInfo",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhotoId",
                table: "ItemInfo",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoId",
                table: "ItemInfo",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemInfo_PhotoId",
                table: "ItemInfo",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInfo_VideoId",
                table: "ItemInfo",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInfo_Photos_PhotoId",
                table: "ItemInfo",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInfo_Videos_VideoId",
                table: "ItemInfo",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemInfo_Photos_PhotoId",
                table: "ItemInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemInfo_Videos_VideoId",
                table: "ItemInfo");

            migrationBuilder.DropIndex(
                name: "IX_ItemInfo_PhotoId",
                table: "ItemInfo");

            migrationBuilder.DropIndex(
                name: "IX_ItemInfo_VideoId",
                table: "ItemInfo");

            migrationBuilder.DropColumn(
                name: "LastModifiedDateTime",
                table: "ItemInfo");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ItemInfo");

            migrationBuilder.DropColumn(
                name: "PhotoId",
                table: "ItemInfo");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "ItemInfo");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
