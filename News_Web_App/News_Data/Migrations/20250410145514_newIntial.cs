using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace News_Data.Migrations
{
    /// <inheritdoc />
    public partial class newIntial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowInfo",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowInfo",
                table: "AspNetUsers");
        }
    }
}
