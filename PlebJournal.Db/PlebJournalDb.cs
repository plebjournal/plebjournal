using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlebJournal.Db.Models;

namespace PlebJournal.Db;

using Microsoft.AspNetCore.Identity;

public class PlebUser : IdentityUser<Guid>
{
    public DbSet<Transaction> Transactions { get; set; }
}

public class Role : IdentityRole<Guid> { }

public class PlebJournalDb : IdentityDbContext<PlebUser, Role, Guid>
{
    public required DbSet<Transaction> Transactions { get; set; }
    public required DbSet<Price> Prices { get; set; }
    public required DbSet<CurrentPrice> CurrentPrices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("User ID=postgres;Password=password;Host=localhost;Port=5469;");

}