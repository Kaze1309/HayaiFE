using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HayaiFE.Models;
namespace HayaiFE.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {

        public DbSet<SfLicense> SyncfusionLicenses { get; set; }

        public DbSet<BlockData> BlocksInfo { get; set; }
    }
}
