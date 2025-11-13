using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DtuFoodAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixMediaStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodTrucks_Images_ImageId",
                table: "FoodTrucks");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                table: "FoodTrucks",
                newName: "PageBannerId");

            migrationBuilder.RenameIndex(
                name: "IX_FoodTrucks_ImageId",
                table: "FoodTrucks",
                newName: "IX_FoodTrucks_PageBannerId");

            migrationBuilder.AddColumn<Guid>(
                name: "HomeBannerId",
                table: "FoodTrucks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodTrucks_HomeBannerId",
                table: "FoodTrucks",
                column: "HomeBannerId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodTrucks_Images_HomeBannerId",
                table: "FoodTrucks",
                column: "HomeBannerId",
                principalTable: "Images",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodTrucks_Images_PageBannerId",
                table: "FoodTrucks",
                column: "PageBannerId",
                principalTable: "Images",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodTrucks_Images_HomeBannerId",
                table: "FoodTrucks");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodTrucks_Images_PageBannerId",
                table: "FoodTrucks");

            migrationBuilder.DropIndex(
                name: "IX_FoodTrucks_HomeBannerId",
                table: "FoodTrucks");

            migrationBuilder.DropColumn(
                name: "HomeBannerId",
                table: "FoodTrucks");

            migrationBuilder.RenameColumn(
                name: "PageBannerId",
                table: "FoodTrucks",
                newName: "ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_FoodTrucks_PageBannerId",
                table: "FoodTrucks",
                newName: "IX_FoodTrucks_ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodTrucks_Images_ImageId",
                table: "FoodTrucks",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id");
        }
    }
}
