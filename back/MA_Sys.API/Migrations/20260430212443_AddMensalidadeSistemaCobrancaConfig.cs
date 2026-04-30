using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_Sys.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMensalidadeSistemaCobrancaConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AceitaCartao",
                table: "Academias");

            migrationBuilder.DropColumn(
                name: "AceitaPix",
                table: "Academias");

            migrationBuilder.AddColumn<bool>(
                name: "AceitaCartao",
                table: "MensalidadesSistema",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AceitaPix",
                table: "MensalidadesSistema",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoAccessToken",
                table: "MensalidadesSistema",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoPublicKey",
                table: "MensalidadesSistema",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AceitaCartao",
                table: "MensalidadesSistema");

            migrationBuilder.DropColumn(
                name: "AceitaPix",
                table: "MensalidadesSistema");

            migrationBuilder.DropColumn(
                name: "MercadoPagoAccessToken",
                table: "MensalidadesSistema");

            migrationBuilder.DropColumn(
                name: "MercadoPagoPublicKey",
                table: "MensalidadesSistema");

            migrationBuilder.AddColumn<bool>(
                name: "AceitaCartao",
                table: "Academias",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AceitaPix",
                table: "Academias",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
