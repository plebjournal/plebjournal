using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PlebJournal.Db.Models;
using PlebJournal.Db.Seed;

namespace PlebJournal.Db;

using Microsoft.AspNetCore.Identity;

public class PlebUser : IdentityUser<Guid>
{
    public PlebUser(string userName) : base(userName)
    {
        Transactions = new List<Transaction>();
        UserSettings = new List<UserSetting>();
    }
    public List<Transaction> Transactions { get; set; }
    public List<UserSetting> UserSettings { get; set; }
}

public class Role : IdentityRole<Guid> { }

public class PlebJournalDb : IdentityDbContext<PlebUser, Role, Guid>
{
    public PlebJournalDb(DbContextOptions<PlebJournalDb> opts) : base(opts) { }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Price> Prices { get; set; }
    public DbSet<CurrentPrice> CurrentPrices { get; set; }
    public DbSet<UserSetting> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Price>().HasData(SeedData.LoadSeedData());
        base.OnModelCreating(builder);
    }
}

public class PlebJournalDbFactory : IDesignTimeDbContextFactory<PlebJournalDb>
{
    public PlebJournalDb CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlebJournalDb>();
        optionsBuilder.UseNpgsql("User ID=postgres;Password=password;Host=localhost;Port=5469;");

        return new PlebJournalDb(optionsBuilder.Options);
    }
}