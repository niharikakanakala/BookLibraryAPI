using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookLibrary.WebAPI.Data;
using BookLibrary.WebAPI.Models;
using BookLibrary.WebAPI.Services;

namespace BookLibrary.WebAPI.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Book>>> GetAllBooks()
        {
            var books = await _bookService.GetAllBooks();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBookById(int id)
        {
            var book = await _bookService.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<Book>>> SearchBooksByTitle([FromQuery] string title)
        {
            var books = await _bookService.GetBooksByTitle(title);
            return Ok(books);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(Book book)
        {
            await _bookService.AddBook(book);
            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, Book book)
        {
            if (id != book.Id)
            {
                return BadRequest();
            }

            var existingBook = await _bookService.GetBookById(id);
            if (existingBook == null)
            {
                return NotFound();
            }

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.Genre = book.Genre;
            existingBook.PublicationYear = book.PublicationYear;
            existingBook.IsAvailable = book.IsAvailable;

            await _bookService.UpdateBook(existingBook);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _bookService.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }

            await _bookService.DeleteBook(id);

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllBooks()
        {
            await _bookService.DeleteAllBooks();
            return NoContent();
        }
    }
}
