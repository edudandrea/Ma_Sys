using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_Sys.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFederacaoFluxoCaixaManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AcademiaId",
                table: "Financeiros",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "Financeiros",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Financeiros_AcademiaId",
                table: "Financeiros",
                column: "AcademiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Financeiros_OwnerUserId",
                table: "Financeiros",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Financeiros_Academias_AcademiaId",
                table: "Financeiros",
                column: "AcademiaId",
                principalTable: "Academias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Financeiros_User_OwnerUserId",
                table: "Financeiros",
                column: "OwnerUserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Financeiros_Academias_AcademiaId",
                table: "Financeiros");

            migrationBuilder.DropForeignKey(
                name: "FK_Financeiros_User_OwnerUserId",
                table: "Financeiros");

            migrationBuilder.DropIndex(
                name: "IX_Financeiros_AcademiaId",
                table: "Financeiros");

            migrationBuilder.DropIndex(
                name: "IX_Financeiros_OwnerUserId",
                table: "Financeiros");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Financeiros");

            migrationBuilder.AlterColumn<int>(
                name: "AcademiaId",
                table: "Financeiros",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
