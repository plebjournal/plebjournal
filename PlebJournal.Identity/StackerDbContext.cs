using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
    
namespace PlebJournal.Identity;

public class StackerDbContext: IdentityDbContext
{
    public StackerDbContext(DbContextOptions options) : base(options)
    {
    }
}