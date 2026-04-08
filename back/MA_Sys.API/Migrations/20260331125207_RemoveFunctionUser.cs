using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_SYS.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFunctionUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Function",
                table: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Function",
                table: "User",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
