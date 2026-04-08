using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_SYS.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNullProf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Professores_Academias_AcademiaId",
                table: "Professores");

            migrationBuilder.AlterColumn<int>(
                name: "AcademiaId",
                table: "Professores",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Professores_Academias_AcademiaId",
                table: "Professores",
                column: "AcademiaId",
                principalTable: "Academias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Professores_Academias_AcademiaId",
                table: "Professores");

            migrationBuilder.AlterColumn<int>(
                name: "AcademiaId",
                table: "Professores",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Professores_Academias_AcademiaId",
                table: "Professores",
                column: "AcademiaId",
                principalTable: "Academias",
                principalColumn: "Id");
        }
    }
}
