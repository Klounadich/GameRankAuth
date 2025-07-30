using Microsoft.EntityFrameworkCore;
using GameRankAuth.Models;
namespace GameRankAuth.Data;

public class AdminPanelDBContext: DbContext
{
    public AdminPanelDBContext(DbContextOptions<AdminPanelDBContext> options) : base(options) { }
    public DbSet<DataForAdmin> DataForAdmins { get; set; }
}