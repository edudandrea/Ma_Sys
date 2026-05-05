using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_Sys.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFederacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FederacaoId",
                table: "User",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Federacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: true),
                    Cidade = table.Column<string>(type: "TEXT", nullable: true),
                    Estado = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Telefone = table.Column<string>(type: "TEXT", nullable: true),
                    Responsavel = table.Column<string>(type: "TEXT", nullable: true),
                    RedeSocial = table.Column<string>(type: "TEXT", nullable: true),
                    LogoUrl = table.Column<string>(type: "TEXT", nullable: true),
                    OwnerUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Federacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Federacoes_User_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_FederacaoId",
                table: "User",
                column: "FederacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Federacoes_OwnerUserId",
                table: "Federacoes",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Federacoes_FederacaoId",
                table: "User",
                column: "FederacaoId",
                principalTable: "Federacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Federacoes_FederacaoId",
                table: "User");

            migrationBuilder.DropTable(
                name: "Federacoes");

            migrationBuilder.DropIndex(
                name: "IX_User_FederacaoId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "FederacaoId",
                table: "User");
        }
    }
}
