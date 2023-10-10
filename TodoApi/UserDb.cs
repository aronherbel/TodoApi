using Microsoft.EntityFrameworkCore;

namespace TodoApi
{
    public class UserDb : DbContext 
    { 
    
        public UserDb(DbContextOptions<UserDb> options)
            : base(options) { }

        public DbSet<User> UserTable => Set<User>();

    }
}
