using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlebJournal.Identity;

public class StackerDbContextFactory : IDesignTimeDbContextFactory<StackerDbContext>
{
    public StackerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<StackerDbContext>();
        optionsBuilder.UseNpgsql();
        return new StackerDbContext(optionsBuilder.Options);
    }
}