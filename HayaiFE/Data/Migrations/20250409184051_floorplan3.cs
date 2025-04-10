using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HayaiFE.Migrations
{
    /// <inheritdoc />
    public partial class floorplan3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeatNumbers",
                table: "BlocksInfo");

            migrationBuilder.AddColumn<string>(
                name: "EndingSeatNumber",
                table: "BlocksInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartingSeatNumber",
                table: "BlocksInfo",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndingSeatNumber",
                table: "BlocksInfo");

            migrationBuilder.DropColumn(
                name: "StartingSeatNumber",
                table: "BlocksInfo");

            migrationBuilder.AddColumn<string>(
                name: "SeatNumbers",
                table: "BlocksInfo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
