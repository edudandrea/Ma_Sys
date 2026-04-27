using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_SYS.Api.Migrations
{
    /// <inheritdoc />
    public partial class DtaPGMatricula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Pagamentos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MatriculaId",
                table: "Pagamentos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPagamento",
                table: "Matriculas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MensalidadePaga",
                table: "Matriculas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Pagamentos");

            migrationBuilder.DropColumn(
                name: "MatriculaId",
                table: "Pagamentos");

            migrationBuilder.DropColumn(
                name: "DataPagamento",
                table: "Matriculas");

            migrationBuilder.DropColumn(
                name: "MensalidadePaga",
                table: "Matriculas");
        }
    }
}
