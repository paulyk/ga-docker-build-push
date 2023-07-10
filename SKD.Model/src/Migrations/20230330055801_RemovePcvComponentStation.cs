using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations {
    /// <inheritdoc />
    public partial class RemovePcvComponentStation : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {

            // remove duplicate PcvId, ComponentId, ProductionStationId
            migrationBuilder.Sql(@"
delete from pcv_component
where id in (
    select id
    from (
        select id, row_number() over (partition by pcvId, componentId order by id) as row_num
        from pcv_component
    ) as t
    where t.row_num > 1
)");
            migrationBuilder.DropForeignKey(
                name: "FK_pcv_component_production_station_ProductionStationId",
                table: "pcv_component");

            migrationBuilder.DropIndex(
                name: "IX_pcv_component_PcvId_ComponentId_ProductionStationId",
                table: "pcv_component");

            migrationBuilder.DropIndex(
                name: "IX_pcv_component_ProductionStationId",
                table: "pcv_component");

            migrationBuilder.DropColumn(
                name: "ProductionStationId",
                table: "pcv_component");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_component_PcvId_ComponentId",
                table: "pcv_component",
                columns: new[] { "PcvId", "ComponentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropIndex(
                name: "IX_pcv_component_PcvId_ComponentId",
                table: "pcv_component");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductionStationId",
                table: "pcv_component",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_pcv_component_PcvId_ComponentId_ProductionStationId",
                table: "pcv_component",
                columns: new[] { "PcvId", "ComponentId", "ProductionStationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_component_ProductionStationId",
                table: "pcv_component",
                column: "ProductionStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_component_production_station_ProductionStationId",
                table: "pcv_component",
                column: "ProductionStationId",
                principalTable: "production_station",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
