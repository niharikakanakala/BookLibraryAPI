using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibrary.WebAPI.Models;

namespace BookLibrary.WebAPI.Services
{
    public interface IBookService
    {
        Task<List<Book>> GetAllBooks();
        Task<Book> GetBookById(int id);
        Task<List<Book>> GetBooksByTitle(string title); 
        Task AddBook(Book book);
        Task UpdateBook(Book book);
        Task DeleteBook(int id);
        Task DeleteAllBooks();
        Task<Book> GetBookByTitle(string title); 
        Task<List<Book>> SortBooksByTitle(string sortOrder);
        Task<List<Book>> SortBooksByAuthor(string sortOrder);
        Task<List<Book>> SortBooksByGenre(string sortOrder);
        Task<List<Book>> SortBooksByPublicationYear(string sortOrder);
        Task<List<Book>> SortBooksByAvailability(string sortOrder);
    }
}
