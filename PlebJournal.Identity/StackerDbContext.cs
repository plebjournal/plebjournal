using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
    
namespace PlebJournal.Identity;

public class StackerDbContext: IdentityDbContext
{
    public StackerDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql("User ID=postgres;Password=password;Host=localhost;Port=2121;Database=stacker;");
    }
}