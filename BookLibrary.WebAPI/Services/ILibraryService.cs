using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibrary.WebAPI.Models;

namespace BookLibrary.WebAPI.Services
{
    public interface IBookService
    {
        Task<List<Book>> GetAllBooks();
        Task<Book> GetBookById(int id);
        Task<List<Book>> GetBooksByTitle(string title); // Keep this method for searching books by title
        Task AddBook(Book book);
        Task UpdateBook(Book book);
        Task DeleteBook(int id);
        Task DeleteAllBooks();
        Task<Book> GetBookByTitle(string title); // Update the return type to Task<Book>
    }
}
