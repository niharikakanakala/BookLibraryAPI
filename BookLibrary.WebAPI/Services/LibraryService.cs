using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BookLibrary.WebAPI.Data;
using BookLibrary.WebAPI.Models;
using BookLibrary.WebAPI.Services;

namespace BookLibrary.WebAPI.Services
{
    public class BookService : IBookService
    {
        private readonly BookContext _bookContext;

        public BookService(BookContext bookContext)
        {
            _bookContext = bookContext;
        }

        public async Task<List<Book>> GetAllBooks()
        {
            return await _bookContext.Books.ToListAsync();
        }

        public async Task<Book> GetBookById(int id)
        {
            return await _bookContext.Books.FindAsync(id);
        }

        public async Task<List<Book>> GetBooksByTitle(string title)
        {
            return await _bookContext.Books
                .Where(b => b.Title.Contains(title))
                .ToListAsync();
        }

        public async Task AddBook(Book book)
        {
            _bookContext.Books.Add(book);
            await _bookContext.SaveChangesAsync();
        }

        public async Task UpdateBook(Book book)
        {
            _bookContext.Books.Update(book);
            await _bookContext.SaveChangesAsync();
        }

        public async Task DeleteBook(int id)
        {
            var book = await _bookContext.Books.FindAsync(id);
            if (book != null)
            {
                _bookContext.Books.Remove(book);
                await _bookContext.SaveChangesAsync();
            }
        }

        public async Task DeleteAllBooks()
        {
            // Delete all books from the database
            _bookContext.Books.RemoveRange(_bookContext.Books);
            await _bookContext.SaveChangesAsync();
        }

        public async Task<Book> GetBookByTitle(string title)
        {
            return await _bookContext.Books
                .FirstOrDefaultAsync(b => b.Title == title);
        }
    }
}
