using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations
{
    /// <inheritdoc />
    public partial class VerifyVinToBuildStart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    update kit_timeline_event_type
    set 
        Code = 'BUILD_START',
        Description = 'Build Start'
    where Code = 'VERIFY_VIN'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    update kit_timeline_event_type
    set 
        Code = 'VERIFY_VIN',
        Description = 'Verify VIN'
    where Code = 'BUILD_START'
            ");

        }
    }
}
