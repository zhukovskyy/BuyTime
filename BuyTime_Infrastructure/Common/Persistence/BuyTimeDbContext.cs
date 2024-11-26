using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Common.Persistence;

public class BuyTimeDbContext : DbContext
{
    public BuyTimeDbContext() : base()
    {
    }

    public BuyTimeDbContext(DbContextOptions<BuyTimeDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}