using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKitVinImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_kit_vin_kit_vin_import_KitVinImportId",
                table: "kit_vin");

            migrationBuilder.DropTable(
                name: "kit_vin_import");

            migrationBuilder.DropIndex(
                name: "IX_kit_vin_KitVinImportId",
                table: "kit_vin");

            migrationBuilder.DropColumn(
                name: "KitVinImportId",
                table: "kit_vin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KitVinImportId",
                table: "kit_vin",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "kit_vin_import",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PartnerPlantCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sequence = table.Column<int>(type: "int", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kit_vin_import", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kit_vin_import_plant_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kit_vin_KitVinImportId",
                table: "kit_vin",
                column: "KitVinImportId");

            migrationBuilder.CreateIndex(
                name: "IX_kit_vin_import_PlantId_Sequence",
                table: "kit_vin_import",
                columns: new[] { "PlantId", "Sequence" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_kit_vin_kit_vin_import_KitVinImportId",
                table: "kit_vin",
                column: "KitVinImportId",
                principalTable: "kit_vin_import",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
