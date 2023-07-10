using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePartnerStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KitStatusCode",
                table: "kit_timeline_event_type",
                newName: "PartnerStatusCode");

            migrationBuilder.RenameIndex(
                name: "IX_kit_timeline_event_type_KitStatusCode",
                table: "kit_timeline_event_type",
                newName: "IX_kit_timeline_event_type_PartnerStatusCode");

            migrationBuilder.AddColumn<DateTime>(
                name: "PartnerStatusUpdatedAt",
                table: "kit_timeline_event",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_kit_timeline_event_PartnerStatusUpdatedAt",
                table: "kit_timeline_event",
                column: "PartnerStatusUpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_kit_timeline_event_PartnerStatusUpdatedAt",
                table: "kit_timeline_event");

            migrationBuilder.DropColumn(
                name: "PartnerStatusUpdatedAt",
                table: "kit_timeline_event");

            migrationBuilder.RenameColumn(
                name: "PartnerStatusCode",
                table: "kit_timeline_event_type",
                newName: "KitStatusCode");

            migrationBuilder.RenameIndex(
                name: "IX_kit_timeline_event_type_PartnerStatusCode",
                table: "kit_timeline_event_type",
                newName: "IX_kit_timeline_event_type_KitStatusCode");
        }
    }
}
