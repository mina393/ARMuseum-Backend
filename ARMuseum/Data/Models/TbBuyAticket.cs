using ARMuseum.Dtos;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ARMuseum.Data.Models
{
    public partial class OurDbContext : IdentityDbContext<ApplicationUser>
    {
        public OurDbContext()
        {
        }

        public OurDbContext(DbContextOptions<OurDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TbBuyAticket> TbBuyAtickets { get; set; }
        public virtual DbSet<TbMuseum> TbMuseums { get; set; }
        public virtual DbSet<TbTicket> TbTickets { get; set; }
        public virtual DbSet<TbTicketPrice> TbTicketPrices { get; set; }
        public virtual DbSet<TbUser> TbUsers { get; set; }

        // The connection string is now configured in Program.cs to be loaded 
        // from appsettings.json or user secrets, making this method unnecessary and unsafe.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This line is very important

            modelBuilder.Entity<TbBuyAticket>(entity =>
            {
                entity.Property(e => e.TOrderId).ValueGeneratedNever();

                entity.HasOne(d => d.MIdNavigation).WithMany(p => p.TbBuyAtickets)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TbBuyATicket_TbMuseum");

                entity.HasOne(d => d.Ticket).WithMany(p => p.TbBuyAtickets)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TbBuyATicket_TbTicket");

                entity.HasOne(d => d.UIdNavigation).WithMany(p => p.TbBuyAtickets)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TbBuyATicket_TbUser");
            });

            modelBuilder.Entity<TbTicketPrice>(entity =>
            {
                entity.HasOne(d => d.Ticket).WithMany(p => p.TbTicketPrices)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TbTicketPrice_TbTicket");
            });

            modelBuilder.Entity<TbUser>(entity =>
            {
                entity.Property(e => e.UEmail).IsFixedLength();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}