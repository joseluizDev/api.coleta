
using System.ComponentModel.DataAnnotations;

public abstract class Entity
{
    protected Entity()
    {
        Id = Guid.NewGuid();
        DataInclusao = DateTime.Now;
    }

    [Key]
    public Guid Id { get; set; }
    public DateTime DataInclusao { get; private set; }
}