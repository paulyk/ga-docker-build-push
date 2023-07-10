using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations
{
    /// <inheritdoc />
    public partial class LotBomNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lot_bom_BomId",
                table: "lot");

            migrationBuilder.AlterColumn<Guid>(
                name: "BomId",
                table: "lot",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_lot_bom_BomId",
                table: "lot",
                column: "BomId",
                principalTable: "bom",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lot_bom_BomId",
                table: "lot");

            migrationBuilder.AlterColumn<Guid>(
                name: "BomId",
                table: "lot",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_lot_bom_BomId",
                table: "lot",
                column: "BomId",
                principalTable: "bom",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
