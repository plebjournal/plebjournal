using System.ComponentModel.DataAnnotations;

namespace PlebJournal.Db.Models;

public class Entity
{
    [Key] public Guid Id { get; set; }
    [Required] public DateTime Created { get; set; } = DateTime.UtcNow;
    [Required] public DateTime Updated { get; set; } = DateTime.UtcNow;
}