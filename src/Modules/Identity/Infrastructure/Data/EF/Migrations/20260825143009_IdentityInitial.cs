using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FIAP.CloudGames.Infrastructure.Data.EF.Migrations.Identity
{
    /// <inheritdoc />
    public partial class IdentityInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "tb_Perfil",
                schema: "identity",
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
                name: "permissoes",
                schema: "identity",
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
                        principalSchema: "identity",
                        principalTable: "tb_Perfil",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cpf = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data_nascimento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    perfil_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_em_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    data_inativacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.id);
                    table.CheckConstraint("ck_usuarios_perfil_id_nao_vazio", "\"perfil_id\" <> '00000000-0000-0000-0000-000000000000'::uuid");
                    table.ForeignKey(
                        name: "fk_usuarios_perfis",
                        column: x => x.perfil_id,
                        principalSchema: "identity",
                        principalTable: "tb_Perfil",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_Autorizacao",
                schema: "identity",
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
                        name: "FK_tb_Autorizacao_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "identity",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_Tokens",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DataExpiracao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DataRevogacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_Tokens_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "identity",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "tb_Perfil",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Usuario" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Administrador" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_permissoes_perfil_id",
                schema: "identity",
                table: "permissoes",
                column: "perfil_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_Autorizacao_UsuarioId_JogoId",
                schema: "identity",
                table: "tb_Autorizacao",
                columns: new[] { "UsuarioId", "JogoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_Perfil_Nome",
                schema: "identity",
                table: "tb_Perfil",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_Tokens_TokenHash",
                schema: "identity",
                table: "tb_Tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_Tokens_UsuarioId",
                schema: "identity",
                table: "tb_Tokens",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_perfil_id",
                schema: "identity",
                table: "usuarios",
                column: "perfil_id");

            migrationBuilder.CreateIndex(
                name: "ux_usuarios_email",
                schema: "identity",
                table: "usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "permissoes",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "tb_Autorizacao",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "tb_Tokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "tb_Perfil",
                schema: "identity");
        }
    }
}
