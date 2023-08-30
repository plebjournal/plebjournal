using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlebJournal.Db.Models;

public class Note : Entity
{
    [Column(TypeName = "text")] public required string Text { get; set; }
    [MaxLength(50)] public string? Sentiment { get; set; }
    [MaxLength(10)] public required string Currency { get; set; } = "USD";
    public required decimal Price { get; set; }
    public required PlebUser PlebUser { get; set; }
}