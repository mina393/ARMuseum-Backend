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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            => optionsBuilder.UseSqlServer("Server=DESKTOP-3GCTNMS;Database=OurDatabaseModified;Trusted_Connection=True;TrustServerCertificate=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // مهم جدًا استدعاء الـ base implementation

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
