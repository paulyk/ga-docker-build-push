using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations
{
    public partial class PcvMeta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pcv_pcv_submodel_SubModelId",
                table: "pcv");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_submodel_pcv_model_ModelId",
                table: "pcv_submodel");

            migrationBuilder.DropTable(
                name: "pcv_submodel_component");

            migrationBuilder.DropIndex(
                name: "IX_pcv_submodel_ModelId",
                table: "pcv_submodel");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "pcv_submodel");

            migrationBuilder.RenameColumn(
                name: "SubModelId",
                table: "pcv",
                newName: "PcvTrimId");

            migrationBuilder.RenameIndex(
                name: "IX_pcv_SubModelId",
                table: "pcv",
                newName: "IX_pcv_PcvTrimId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "pcv",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "PcvDriveId",
                table: "pcv",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PcvEngineId",
                table: "pcv",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PcvModelId",
                table: "pcv",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PcvPaintId",
                table: "pcv",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PcvSeriesId",
                table: "pcv",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PcvSubmodelId",
                table: "pcv",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PcvTransmissionId",
                table: "pcv",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "component_station",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    ComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaveCDCComponent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component_station", x => x.Id);
                    table.ForeignKey(
                        name: "FK_component_station_component_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_component_station_production_station_StationId",
                        column: x => x.StationId,
                        principalTable: "production_station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pcv_drive",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pcv_drive", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pcv_engine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pcv_engine", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pcv_paint",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pcv_paint", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pcv_series",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pcv_series", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pcv_transmission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pcv_transmission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pcv_trim",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pcv_trim", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pcv_PcvDriveId",
                table: "pcv",
                column: "PcvDriveId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_PcvEngineId",
                table: "pcv",
                column: "PcvEngineId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_PcvModelId",
                table: "pcv",
                column: "PcvModelId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_PcvPaintId",
                table: "pcv",
                column: "PcvPaintId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_PcvSeriesId",
                table: "pcv",
                column: "PcvSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_PcvSubmodelId",
                table: "pcv",
                column: "PcvSubmodelId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_PcvTransmissionId",
                table: "pcv",
                column: "PcvTransmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_component_station_ComponentId_StationId",
                table: "component_station",
                columns: new[] { "ComponentId", "StationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_component_station_StationId",
                table: "component_station",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_drive_Code",
                table: "pcv_drive",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_drive_Name",
                table: "pcv_drive",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_engine_Code",
                table: "pcv_engine",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_engine_Name",
                table: "pcv_engine",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_paint_Code",
                table: "pcv_paint",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_paint_Name",
                table: "pcv_paint",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_series_Code",
                table: "pcv_series",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_series_Name",
                table: "pcv_series",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_transmission_Code",
                table: "pcv_transmission",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_transmission_Name",
                table: "pcv_transmission",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_trim_Code",
                table: "pcv_trim",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_trim_Name",
                table: "pcv_trim",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_pcv_drive_PcvDriveId",
                table: "pcv",
                column: "PcvDriveId",
                principalTable: "pcv_drive",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_pcv_engine_PcvEngineId",
                table: "pcv",
                column: "PcvEngineId",
                principalTable: "pcv_engine",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_pcv_model_PcvModelId",
                table: "pcv",
                column: "PcvModelId",
                principalTable: "pcv_model",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_pcv_paint_PcvPaintId",
                table: "pcv",
                column: "PcvPaintId",
                principalTable: "pcv_paint",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_pcv_series_PcvSeriesId",
                table: "pcv",
                column: "PcvSeriesId",
                principalTable: "pcv_series",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_pcv_submodel_PcvSubmodelId",
                table: "pcv",
                column: "PcvSubmodelId",
                principalTable: "pcv_submodel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_pcv_transmission_PcvTransmissionId",
                table: "pcv",
                column: "PcvTransmissionId",
                principalTable: "pcv_transmission",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_pcv_trim_PcvTrimId",
                table: "pcv",
                column: "PcvTrimId",
                principalTable: "pcv_trim",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pcv_pcv_drive_PcvDriveId",
                table: "pcv");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_pcv_engine_PcvEngineId",
                table: "pcv");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_pcv_model_PcvModelId",
                table: "pcv");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_pcv_paint_PcvPaintId",
                table: "pcv");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_pcv_series_PcvSeriesId",
                table: "pcv");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_pcv_submodel_PcvSubmodelId",
                table: "pcv");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_pcv_transmission_PcvTransmissionId",
                table: "pcv");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_pcv_trim_PcvTrimId",
                table: "pcv");

            migrationBuilder.DropTable(
                name: "component_station");

            migrationBuilder.DropTable(
                name: "pcv_drive");

            migrationBuilder.DropTable(
                name: "pcv_engine");

            migrationBuilder.DropTable(
                name: "pcv_paint");

            migrationBuilder.DropTable(
                name: "pcv_series");

            migrationBuilder.DropTable(
                name: "pcv_transmission");

            migrationBuilder.DropTable(
                name: "pcv_trim");

            migrationBuilder.DropIndex(
                name: "IX_pcv_PcvDriveId",
                table: "pcv");

            migrationBuilder.DropIndex(
                name: "IX_pcv_PcvEngineId",
                table: "pcv");

            migrationBuilder.DropIndex(
                name: "IX_pcv_PcvModelId",
                table: "pcv");

            migrationBuilder.DropIndex(
                name: "IX_pcv_PcvPaintId",
                table: "pcv");

            migrationBuilder.DropIndex(
                name: "IX_pcv_PcvSeriesId",
                table: "pcv");

            migrationBuilder.DropIndex(
                name: "IX_pcv_PcvSubmodelId",
                table: "pcv");

            migrationBuilder.DropIndex(
                name: "IX_pcv_PcvTransmissionId",
                table: "pcv");

            migrationBuilder.DropColumn(
                name: "PcvDriveId",
                table: "pcv");

            migrationBuilder.DropColumn(
                name: "PcvEngineId",
                table: "pcv");

            migrationBuilder.DropColumn(
                name: "PcvModelId",
                table: "pcv");

            migrationBuilder.DropColumn(
                name: "PcvPaintId",
                table: "pcv");

            migrationBuilder.DropColumn(
                name: "PcvSeriesId",
                table: "pcv");

            migrationBuilder.DropColumn(
                name: "PcvSubmodelId",
                table: "pcv");

            migrationBuilder.DropColumn(
                name: "PcvTransmissionId",
                table: "pcv");

            migrationBuilder.RenameColumn(
                name: "PcvTrimId",
                table: "pcv",
                newName: "SubModelId");

            migrationBuilder.RenameIndex(
                name: "IX_pcv_PcvTrimId",
                table: "pcv",
                newName: "IX_pcv_SubModelId");

            migrationBuilder.AddColumn<Guid>(
                name: "ModelId",
                table: "pcv_submodel",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "pcv",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateTable(
                name: "pcv_submodel_component",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    ComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmodelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pcv_submodel_component", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pcv_submodel_component_component_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pcv_submodel_component_pcv_submodel_SubmodelId",
                        column: x => x.SubmodelId,
                        principalTable: "pcv_submodel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pcv_submodel_ModelId",
                table: "pcv_submodel",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_submodel_component_ComponentId",
                table: "pcv_submodel_component",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_submodel_component_SubmodelId",
                table: "pcv_submodel_component",
                column: "SubmodelId");

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_pcv_submodel_SubModelId",
                table: "pcv",
                column: "SubModelId",
                principalTable: "pcv_submodel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_submodel_pcv_model_ModelId",
                table: "pcv_submodel",
                column: "ModelId",
                principalTable: "pcv_model",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
