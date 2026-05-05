using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_Sys.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFederacaoFormasPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AcademiaId",
                table: "FormaPagamentos",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "FormaPagamentos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormaPagamentos_AcademiaId",
                table: "FormaPagamentos",
                column: "AcademiaId");

            migrationBuilder.CreateIndex(
                name: "IX_FormaPagamentos_OwnerUserId",
                table: "FormaPagamentos",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormaPagamentos_Academias_AcademiaId",
                table: "FormaPagamentos",
                column: "AcademiaId",
                principalTable: "Academias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FormaPagamentos_User_OwnerUserId",
                table: "FormaPagamentos",
                column: "OwnerUserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormaPagamentos_Academias_AcademiaId",
                table: "FormaPagamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_FormaPagamentos_User_OwnerUserId",
                table: "FormaPagamentos");

            migrationBuilder.DropIndex(
                name: "IX_FormaPagamentos_AcademiaId",
                table: "FormaPagamentos");

            migrationBuilder.DropIndex(
                name: "IX_FormaPagamentos_OwnerUserId",
                table: "FormaPagamentos");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "FormaPagamentos");

            migrationBuilder.AlterColumn<int>(
                name: "AcademiaId",
                table: "FormaPagamentos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
