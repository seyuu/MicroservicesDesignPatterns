using Microsoft.EntityFrameworkCore;

namespace Stock.Models
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
            
        }

        public DbSet<Stock> Stocks { get; set; }
    }
}
