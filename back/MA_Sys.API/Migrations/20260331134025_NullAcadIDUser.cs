using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_SYS.Api.Migrations
{
    /// <inheritdoc />
    public partial class NullAcadIDUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Academias_AcademiaId",
                table: "User");

            migrationBuilder.AlterColumn<int>(
                name: "AcademiaId",
                table: "User",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Academias_AcademiaId",
                table: "User",
                column: "AcademiaId",
                principalTable: "Academias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Academias_AcademiaId",
                table: "User");

            migrationBuilder.AlterColumn<int>(
                name: "AcademiaId",
                table: "User",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Academias_AcademiaId",
                table: "User",
                column: "AcademiaId",
                principalTable: "Academias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
