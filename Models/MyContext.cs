using Microsoft.EntityFrameworkCore;

namespace BeltExam2.Models
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions options) : base(options) {}
        public DbSet<User> Users {get;set;}
        public DbSet <DojoAct> DojoActs {get;set;}
        public DbSet <UserDojoAct> UserDojoActs {get;set;}

    }
}