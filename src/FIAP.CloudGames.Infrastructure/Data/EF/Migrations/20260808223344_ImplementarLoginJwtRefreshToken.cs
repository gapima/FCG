using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FIAP.CloudGames.Infrastructure.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class ImplementarLoginJwtRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // O modelo anterior armazenava tokens brutos sem vínculo com um usuário.
            // Não há conversão segura desses registros para refresh tokens com hash.
            migrationBuilder.Sql("DELETE FROM \"tb_Tokens\";");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "tb_Tokens");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "data_nascimento",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "cpf",
                table: "usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "tb_Tokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId",
                table: "tb_Tokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                """
                ALTER TABLE "tb_Tokens" ALTER COLUMN "TokenHash" DROP DEFAULT;
                ALTER TABLE "tb_Tokens" ALTER COLUMN "UsuarioId" DROP DEFAULT;
                """);

            migrationBuilder.InsertData(
                table: "tb_Perfil",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Usuario" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Administrador" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_Tokens_TokenHash",
                table: "tb_Tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_Tokens_UsuarioId",
                table: "tb_Tokens",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_Tokens_usuarios_UsuarioId",
                table: "tb_Tokens",
                column: "UsuarioId",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"tb_Tokens\";");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_Tokens_usuarios_UsuarioId",
                table: "tb_Tokens");

            migrationBuilder.DropIndex(
                name: "IX_tb_Tokens_TokenHash",
                table: "tb_Tokens");

            migrationBuilder.DropIndex(
                name: "IX_tb_Tokens_UsuarioId",
                table: "tb_Tokens");

            migrationBuilder.Sql(
                """
                DO $migration$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM "usuarios"
                        WHERE "perfil_id" IN (
                            '11111111-1111-1111-1111-111111111111'::uuid,
                            '22222222-2222-2222-2222-222222222222'::uuid))
                    THEN
                        RAISE EXCEPTION 'O rollback não pode remover os perfis do sistema enquanto existirem usuários associados a eles.';
                    END IF;
                END
                $migration$;
                """);

            migrationBuilder.DeleteData(
                table: "tb_Perfil",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "tb_Perfil",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "tb_Tokens");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "tb_Tokens");

            migrationBuilder.Sql(
                """
                UPDATE "usuarios" SET "cpf" = '' WHERE "cpf" IS NULL;
                UPDATE "usuarios"
                SET "data_nascimento" = '0001-01-01 00:00:00+00'
                WHERE "data_nascimento" IS NULL;
                """);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "data_nascimento",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cpf",
                table: "usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "tb_Tokens",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
