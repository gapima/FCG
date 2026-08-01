using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.CloudGames.Infrastructure.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirPerfilIdUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "usuarios"
                ALTER COLUMN "perfil_id" DROP DEFAULT;
                """);

            migrationBuilder.Sql(
                """
                DO $migration$
                DECLARE
                    valor_perfil text;
                    perfil_id_convertido uuid;
                BEGIN
                    FOR valor_perfil IN
                        SELECT DISTINCT btrim("perfil_id")
                        FROM "usuarios"
                    LOOP
                        IF valor_perfil IS NULL OR valor_perfil = '' THEN
                            RAISE EXCEPTION 'A coluna usuarios.perfil_id contém valores nulos ou vazios. Corrija os dados antes de reaplicar a migration.';
                        END IF;

                        BEGIN
                            perfil_id_convertido := valor_perfil::uuid;
                        EXCEPTION
                            WHEN invalid_text_representation THEN
                                RAISE EXCEPTION 'A coluna usuarios.perfil_id contém valores que não são UUIDs válidos. Corrija os dados antes de reaplicar a migration.';
                        END;

                        IF perfil_id_convertido = '00000000-0000-0000-0000-000000000000'::uuid THEN
                            RAISE EXCEPTION 'A coluna usuarios.perfil_id contém UUID vazio. Corrija os dados antes de reaplicar a migration.';
                        END IF;
                    END LOOP;
                END
                $migration$;
                """);

            migrationBuilder.Sql(
                """
                DO $migration$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM "usuarios" AS usuario
                        LEFT JOIN "tb_Perfil" AS perfil
                            ON perfil."Id" = btrim(usuario."perfil_id")::uuid
                        WHERE perfil."Id" IS NULL
                    ) THEN
                        RAISE EXCEPTION 'A coluna usuarios.perfil_id contém UUIDs sem perfil correspondente em tb_Perfil. Corrija os registros órfãos antes de reaplicar a migration.';
                    END IF;
                END
                $migration$;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE "usuarios"
                ALTER COLUMN "perfil_id" TYPE uuid
                USING btrim("perfil_id")::uuid;

                ALTER TABLE "usuarios"
                ALTER COLUMN "perfil_id" SET NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_perfil_id",
                table: "usuarios",
                column: "perfil_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_usuarios_perfil_id_nao_vazio",
                table: "usuarios",
                sql: "\"perfil_id\" <> '00000000-0000-0000-0000-000000000000'::uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_usuarios_perfis",
                table: "usuarios",
                column: "perfil_id",
                principalTable: "tb_Perfil",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_usuarios_perfis",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "ix_usuarios_perfil_id",
                table: "usuarios");

            migrationBuilder.DropCheckConstraint(
                name: "ck_usuarios_perfil_id_nao_vazio",
                table: "usuarios");

            migrationBuilder.Sql(
                """
                DO $migration$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "usuarios") THEN
                        RAISE EXCEPTION 'O rollback não pode ser realizado com segurança enquanto existirem usuários com PerfilId em UUID.';
                    END IF;
                END
                $migration$;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE "usuarios"
                ALTER COLUMN "perfil_id" TYPE character varying(20)
                USING "perfil_id"::text;

                ALTER TABLE "usuarios"
                ALTER COLUMN "perfil_id" SET DEFAULT '';
                """);
        }
    }
}
