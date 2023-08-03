using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PlebJournal.Db.Models;

[Index(nameof(Date))]
public class Transaction : Entity
{
    public required PlebUser PlebUser { get; set; }

    public required DateTime Date { get; set; }

    [MaxLength(50)]
    public required string Type { get; set; }

    public required decimal BtcAmount { get; set; }

    public decimal? FiatAmount { get; set; }

    [MaxLength(50)]
    public string? FiatCode { get; set; }
}