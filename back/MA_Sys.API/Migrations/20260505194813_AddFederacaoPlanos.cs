using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_Sys.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFederacaoPlanos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Planos_Academias_AcademiaId",
                table: "Planos");

            migrationBuilder.AlterColumn<int>(
                name: "AcademiaId",
                table: "Planos",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "Planos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Planos_OwnerUserId",
                table: "Planos",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Planos_Academias_AcademiaId",
                table: "Planos",
                column: "AcademiaId",
                principalTable: "Academias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Planos_User_OwnerUserId",
                table: "Planos",
                column: "OwnerUserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Planos_Academias_AcademiaId",
                table: "Planos");

            migrationBuilder.DropForeignKey(
                name: "FK_Planos_User_OwnerUserId",
                table: "Planos");

            migrationBuilder.DropIndex(
                name: "IX_Planos_OwnerUserId",
                table: "Planos");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Planos");

            migrationBuilder.AlterColumn<int>(
                name: "AcademiaId",
                table: "Planos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Planos_Academias_AcademiaId",
                table: "Planos",
                column: "AcademiaId",
                principalTable: "Academias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
