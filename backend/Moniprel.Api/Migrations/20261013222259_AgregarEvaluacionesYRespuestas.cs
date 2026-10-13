using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Moniprel.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEvaluacionesYRespuestas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "evaluaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    PeriodoEvaluacionId = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFinalizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Finalizada = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_evaluaciones_periodos_evaluacion_PeriodoEvaluacionId",
                        column: x => x.PeriodoEvaluacionId,
                        principalTable: "periodos_evaluacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_evaluaciones_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "respuestas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EvaluacionId = table.Column<int>(type: "integer", nullable: false),
                    PreguntaId = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_respuestas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_respuestas_evaluaciones_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "evaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_respuestas_preguntas_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "preguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_evaluaciones_PeriodoEvaluacionId",
                table: "evaluaciones",
                column: "PeriodoEvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_evaluaciones_UsuarioId_PeriodoEvaluacionId",
                table: "evaluaciones",
                columns: new[] { "UsuarioId", "PeriodoEvaluacionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_respuestas_EvaluacionId_PreguntaId",
                table: "respuestas",
                columns: new[] { "EvaluacionId", "PreguntaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_respuestas_PreguntaId",
                table: "respuestas",
                column: "PreguntaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "respuestas");

            migrationBuilder.DropTable(
                name: "evaluaciones");
        }
    }
}
