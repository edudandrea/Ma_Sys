using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_Sys.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePagamentoAcademiaGatewayFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "PagamentosAcademias",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormaPagamentoNome",
                table: "PagamentosAcademias",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MensalidadeSistemaId",
                table: "PagamentosAcademias",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosAcademias_MensalidadeSistemaId",
                table: "PagamentosAcademias",
                column: "MensalidadeSistemaId");

            migrationBuilder.AddForeignKey(
                name: "FK_PagamentosAcademias_MensalidadesSistema_MensalidadeSistemaId",
                table: "PagamentosAcademias",
                column: "MensalidadeSistemaId",
                principalTable: "MensalidadesSistema",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PagamentosAcademias_MensalidadesSistema_MensalidadeSistemaId",
                table: "PagamentosAcademias");

            migrationBuilder.DropIndex(
                name: "IX_PagamentosAcademias_MensalidadeSistemaId",
                table: "PagamentosAcademias");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "PagamentosAcademias");

            migrationBuilder.DropColumn(
                name: "FormaPagamentoNome",
                table: "PagamentosAcademias");

            migrationBuilder.DropColumn(
                name: "MensalidadeSistemaId",
                table: "PagamentosAcademias");
        }
    }
}
