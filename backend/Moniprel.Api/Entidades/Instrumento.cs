namespace Moniprel.Api.Entidades;

public class Instrumento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Dimension> Dimensiones { get; set; }
        = new List<Dimension>();

    public ICollection<PeriodoEvaluacion> Periodos { get; set; }
        = new List<PeriodoEvaluacion>();
}