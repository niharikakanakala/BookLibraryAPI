using Microsoft.EntityFrameworkCore;
using BookLibrary.WebAPI.Models;

namespace BookLibrary.WebAPI.Data
{
    public class BookContext : DbContext
    {
        public BookContext(DbContextOptions<BookContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration for entities

            base.OnModelCreating(modelBuilder);
        }
    }
}
