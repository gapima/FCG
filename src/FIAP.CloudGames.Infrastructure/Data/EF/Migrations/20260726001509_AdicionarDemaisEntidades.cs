using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.CloudGames.Infrastructure.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDemaisEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "nome",
                table: "usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "usuarios",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320);

            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "cpf",
                table: "usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "data_inativacao",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "data_nascimento",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "perfil_id",
                table: "usuarios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "senha_hash",
                table: "usuarios",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "jogos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    faixa_etaria = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    preco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    data_cadastro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jogos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_LogUsuarios",
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
                    table.ForeignKey(
                        name: "FK_tb_LogUsuarios_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_Perfil",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "varchar(200)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Perfil", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_Tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataRevogacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Tokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "aquisicoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    jogo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_aquisicao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aquisicoes", x => new { x.id, x.usuario_id, x.jogo_id });
                    table.ForeignKey(
                        name: "fk_aquisicoes_jogos",
                        column: x => x.jogo_id,
                        principalTable: "jogos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aquisicoes_usuarios",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_Autorizacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    JogoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Autorizacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_Autorizacao_jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "jogos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_Autorizacao_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_LogJogos",
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
                    table.ForeignKey(
                        name: "FK_tb_LogJogos_jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "jogos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rel_CategoriaJogo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JogoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rel_CategoriaJogo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rel_CategoriaJogo_jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "jogos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rel_CategoriaJogo_tb_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "tb_Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "permissoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    perfil_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissoes", x => new { x.id, x.perfil_id });
                    table.ForeignKey(
                        name: "FK_permissoes_tb_Perfil_perfil_id",
                        column: x => x.perfil_id,
                        principalTable: "tb_Perfil",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aquisicoes_jogo_id",
                table: "aquisicoes",
                column: "jogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_aquisicoes_usuario_id",
                table: "aquisicoes",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_permissoes_perfil_id",
                table: "permissoes",
                column: "perfil_id");

            migrationBuilder.CreateIndex(
                name: "IX_rel_CategoriaJogo_CategoriaId",
                table: "rel_CategoriaJogo",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_rel_CategoriaJogo_JogoId_CategoriaId",
                table: "rel_CategoriaJogo",
                columns: new[] { "JogoId", "CategoriaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_Autorizacao_JogoId",
                table: "tb_Autorizacao",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_Autorizacao_UsuarioId_JogoId",
                table: "tb_Autorizacao",
                columns: new[] { "UsuarioId", "JogoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_LogJogos_JogoId",
                table: "tb_LogJogos",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_LogUsuarios_UsuarioId",
                table: "tb_LogUsuarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_Perfil_Nome",
                table: "tb_Perfil",
                column: "Nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aquisicoes");

            migrationBuilder.DropTable(
                name: "permissoes");

            migrationBuilder.DropTable(
                name: "rel_CategoriaJogo");

            migrationBuilder.DropTable(
                name: "tb_Autorizacao");

            migrationBuilder.DropTable(
                name: "tb_LogJogos");

            migrationBuilder.DropTable(
                name: "tb_LogUsuarios");

            migrationBuilder.DropTable(
                name: "tb_Tokens");

            migrationBuilder.DropTable(
                name: "tb_Perfil");

            migrationBuilder.DropTable(
                name: "tb_Categorias");

            migrationBuilder.DropTable(
                name: "jogos");

            migrationBuilder.DropColumn(
                name: "ativo",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "cpf",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "data_inativacao",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "data_nascimento",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "perfil_id",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "senha_hash",
                table: "usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "nome",
                table: "usuarios",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "usuarios",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);
        }
    }
}
