using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_SYS.Api.Migrations
{
    /// <inheritdoc />
    public partial class NewCollumAlunosObs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Obs",
                table: "Alunos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Professores_AcademiaId",
                table: "Professores",
                column: "AcademiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Modalidades_AcademiaId",
                table: "Modalidades",
                column: "AcademiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Modalidades_Academias_AcademiaId",
                table: "Modalidades",
                column: "AcademiaId",
                principalTable: "Academias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_Modalidades_Academias_AcademiaId",
                table: "Modalidades");

            migrationBuilder.DropForeignKey(
                name: "FK_Professores_Academias_AcademiaId",
                table: "Professores");

            migrationBuilder.DropIndex(
                name: "IX_Professores_AcademiaId",
                table: "Professores");

            migrationBuilder.DropIndex(
                name: "IX_Modalidades_AcademiaId",
                table: "Modalidades");

            migrationBuilder.DropColumn(
                name: "Obs",
                table: "Alunos");
        }
    }
}
