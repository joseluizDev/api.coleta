namespace api.coleta.models.dtos;


public class QueryColeta
{
    public Guid? SafraID { get; set; }

    public string? Nome { get; set; }
    public Guid? ClienteID { get; set; }
    public Guid? FazendaID { get; set; }
    public Guid? TalhaoID { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10; // Opcional
}

public class QueryRelatorio
{
    public Guid? SafraID { get; set; }
    public Guid? ClienteID { get; set; }
    public Guid? FazendaID { get; set; }
    public Guid? TalhaoID { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}