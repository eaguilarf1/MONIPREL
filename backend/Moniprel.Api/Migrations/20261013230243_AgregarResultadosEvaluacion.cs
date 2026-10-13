using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Moniprel.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarResultadosEvaluacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "resultados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EvaluacionId = table.Column<int>(type: "integer", nullable: false),
                    PuntajeGeneral = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    NivelRiesgo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaCalculo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resultados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_resultados_evaluaciones_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "evaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resultados_dimension",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResultadoId = table.Column<int>(type: "integer", nullable: false),
                    DimensionId = table.Column<int>(type: "integer", nullable: false),
                    Puntaje = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resultados_dimension", x => x.Id);
                    table.ForeignKey(
                        name: "FK_resultados_dimension_dimensiones_DimensionId",
                        column: x => x.DimensionId,
                        principalTable: "dimensiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_resultados_dimension_resultados_ResultadoId",
                        column: x => x.ResultadoId,
                        principalTable: "resultados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_resultados_EvaluacionId",
                table: "resultados",
                column: "EvaluacionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resultados_dimension_DimensionId",
                table: "resultados_dimension",
                column: "DimensionId");

            migrationBuilder.CreateIndex(
                name: "IX_resultados_dimension_ResultadoId_DimensionId",
                table: "resultados_dimension",
                columns: new[] { "ResultadoId", "DimensionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "resultados_dimension");

            migrationBuilder.DropTable(
                name: "resultados");
        }
    }
}
