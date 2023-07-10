using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations {
    /// <inheritdoc />
    public partial class KitStatusCode : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "KitStatusCode",
                table: "kit_timeline_event_type",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE kit_timeline_event_type
                SET KitStatusCode = 'FPCR'
                WHERE Code = 'CUSTOM_RECEIVED'

                UPDATE kit_timeline_event_type
                SET KitStatusCode = 'FPBP'
                WHERE Code = 'PLAN_BUILD'

                UPDATE kit_timeline_event_type
                SET KitStatusCode = 'FPBS'
                WHERE Code = 'VERIFY_VIN'

                UPDATE kit_timeline_event_type
                SET KitStatusCode = 'FPBC'
                WHERE Code = 'BUILD_COMPLETED'

                UPDATE kit_timeline_event_type
                SET KitStatusCode = 'FPGR'
                WHERE Code = 'GATE_RELEASED'

                UPDATE kit_timeline_event_type
                SET KitStatusCode = 'FPWS'
                WHERE Code = 'WHOLE_SALE'
            ");

            // migrationBuilder.AlterColumn  set the column to be not nullable
            migrationBuilder.AlterColumn<string>(
                name: "KitStatusCode",
                table: "kit_timeline_event_type",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true,
                oldDefaultValue: "");


            migrationBuilder.CreateIndex(
                name: "IX_kit_timeline_event_type_KitStatusCode",
                table: "kit_timeline_event_type",
                column: "KitStatusCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropIndex(
                name: "IX_kit_timeline_event_type_KitStatusCode",
                table: "kit_timeline_event_type");

            migrationBuilder.DropColumn(
                name: "KitStatusCode",
                table: "kit_timeline_event_type");
        }
    }
}
