using System.ComponentModel.DataAnnotations;

namespace PlebJournal.Db.Models;

public class UserSetting : Entity
{
    public required PlebUser PlebUser { get; set; }
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string Value { get; set; }
}