using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.CloudGames.Infrastructure.Data.EF.Migrations.Logging
{
    /// <inheritdoc />
    public partial class LoggingInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "logging");

            migrationBuilder.CreateTable(
                name: "tb_LogJogos",
                schema: "logging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JogoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "varchar(200)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_LogJogos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_LogUsuarios",
                schema: "logging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "varchar(200)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_LogUsuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_LogJogos_JogoId",
                schema: "logging",
                table: "tb_LogJogos",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_LogUsuarios_UsuarioId",
                schema: "logging",
                table: "tb_LogUsuarios",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_LogJogos",
                schema: "logging");

            migrationBuilder.DropTable(
                name: "tb_LogUsuarios",
                schema: "logging");
        }
    }
}
