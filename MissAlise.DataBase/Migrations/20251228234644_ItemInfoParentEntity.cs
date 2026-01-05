using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissAlise.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class ItemInfoParentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemInfo_ItemInfo_ParentId",
                table: "ItemInfo");

            migrationBuilder.CreateTable(
                name: "ParentInfo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DriveId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentInfo_ItemInfo_Id",
                        column: x => x.Id,
                        principalTable: "ItemInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInfo_ParentInfo_ParentId",
                table: "ItemInfo",
                column: "ParentId",
                principalTable: "ParentInfo",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemInfo_ParentInfo_ParentId",
                table: "ItemInfo");

            migrationBuilder.DropTable(
                name: "ParentInfo");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInfo_ItemInfo_ParentId",
                table: "ItemInfo",
                column: "ParentId",
                principalTable: "ItemInfo",
                principalColumn: "Id");
        }
    }
}
