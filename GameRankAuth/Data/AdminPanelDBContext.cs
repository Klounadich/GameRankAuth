using Microsoft.EntityFrameworkCore;

namespace GameRankAuth.Data;

public class AdminPanelDBContext: DbContext
{
    public AdminPanelDBContext(DbContextOptions<AdminPanelDBContext> options) : base(options) { }
}