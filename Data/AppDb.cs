using Microsoft.EntityFrameworkCore;
using PaperCutDash.Models;

namespace PaperCutDash.Data;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> opt) : base(opt) { }
    public DbSet<Run> Runs => Set<Run>();
    public DbSet<Comment> Comments => Set<Comment>();
}