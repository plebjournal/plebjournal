using System.ComponentModel.DataAnnotations;

namespace PlebJournal.Db.Models;

public class CurrentPrice : Entity
{
    public required decimal BtcPrice { get; set; }

    [MaxLength(10)] public required string Currency { get; set; } = "USD";
}