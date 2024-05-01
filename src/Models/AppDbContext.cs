
using Microsoft.EntityFrameworkCore;

namespace Src.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options){}
        public DbSet<Users> UserModel {get; set;}  
    }

    

}