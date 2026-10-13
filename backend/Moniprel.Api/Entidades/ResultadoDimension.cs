namespace Moniprel.Api.Entidades;

public class ResultadoDimension
{
    public int Id { get; set; }

    public int ResultadoId { get; set; }

    public Resultado Resultado { get; set; } = null!;

    public int DimensionId { get; set; }

    public Dimension Dimension { get; set; } = null!;

    public decimal Puntaje { get; set; }
}