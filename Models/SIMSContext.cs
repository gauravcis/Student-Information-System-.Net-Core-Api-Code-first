using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS.Models
{
    public class SIMSContext : DbContext
    {

        public SIMSContext(DbContextOptions<SIMSContext> options):base(options)
        {

        }

        public DbSet<Student> Students { get; set; }

        public DbSet<RefreshToken> refreshTokens { get; set; }
    }
}
