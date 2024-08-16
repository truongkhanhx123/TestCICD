using Microsoft.EntityFrameworkCore;
using testbook.Data;

namespace testbook.ModelData
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)

        {
        }
        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<FavoriteBook> FavoriteBooks { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FavoriteBook>()
                .HasKey(fb => new { fb.UserId, fb.BookId });

            modelBuilder.Entity<FavoriteBook>()
                .HasOne(fb => fb.User)
                .WithMany(u => u.FavoriteBooks)
                .HasForeignKey(fb => fb.UserId);

            modelBuilder.Entity<FavoriteBook>()
                .HasOne(fb => fb.Book)
                .WithMany(b => b.FavoriteBooks)
                .HasForeignKey(fb => fb.BookId);
        }
    }
}