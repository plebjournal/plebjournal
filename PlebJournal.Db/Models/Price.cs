using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PlebJournal.Db.Models;

[Index(nameof(Currency))]
public class Price : Entity
{
    public required DateTime Date { get; set; }
    public required decimal BtcPrice { get; set; }
    [MaxLength(10)] public required string Currency { get; set; } = "USD";
}