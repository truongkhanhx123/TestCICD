using BookData.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookData
{
    public class BookDbContext : DbContext
    {
        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<FavoriteBook> FavoriteBooks { get; set; }
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
