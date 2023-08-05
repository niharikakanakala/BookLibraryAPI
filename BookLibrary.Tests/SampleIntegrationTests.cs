using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Text;
using BookLibrary.WebAPI.Data;
using BookLibrary.WebAPI.Models;
using BookLibrary.WebAPI;

namespace SampleIntegrationTests.Tests
{
    public class BookLibraryIntegrationTests
    {
        private TestServer _server;
        private HttpClient _client;

        public BookLibraryIntegrationTests()
        {
            SetUpClient();
        }

        private void SetUpClient()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    var context = new BookContext(new DbContextOptionsBuilder<BookContext>()
                        .UseSqlite("DataSource=:memory:")
                        .EnableSensitiveDataLogging()
                        .Options);

                    services.RemoveAll(typeof(BookContext));
                    services.AddSingleton(context);

                    context.Database.OpenConnection();
                    context.Database.EnsureCreated();

                    context.SaveChanges();

                    // Clear local context cache
                    foreach (var entity in context.ChangeTracker.Entries().ToList())
                    {
                        entity.State = EntityState.Detached;
                    }
                });

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task TestGetBookById()
        {
            // Add test data to the database
            var context = _server.Host.Services.GetRequiredService<BookContext>();
            var testBook = new Book
            {
                Title = "Test Book",
                Author = "Test Author",
                Genre = "Test Genre",
                PublicationYear = 2022,
                IsAvailable = true
            };
            context.Books.Add(testBook);
            context.SaveChanges();

            // Perform the GET request to /api/books/{bookId}
            var response = await _client.GetAsync($"/api/books/{testBook.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Deserialize the response content and verify the result
            var content = await response.Content.ReadAsStringAsync();
            var book = JsonConvert.DeserializeObject<Book>(content);
            book.Should().NotBeNull();
            book.Title.Should().Be(testBook.Title);
            book.Author.Should().Be(testBook.Author);
            book.Genre.Should().Be(testBook.Genre);
            book.PublicationYear.Should().Be(testBook.PublicationYear);
            book.IsAvailable.Should().Be(testBook.IsAvailable);
        }

        [Fact]
        public async Task TestDeleteAllBooks()
        {
            // Add test data to the database
            var context = _server.Host.Services.GetRequiredService<BookContext>();
            context.Books.Add(new Book
            {
                Title = "Test Book 1",
                Author = "Test Author 1",
                Genre = "Test Genre 1",
                PublicationYear = 2022,
                IsAvailable = true
            });

            context.Books.Add(new Book
            {
                Title = "Test Book 2",
                Author = "Test Author 2",
                Genre = "Test Genre 2",
                PublicationYear = 2023,
                IsAvailable = false
            });

            context.SaveChanges();

            // Perform the DELETE request to /api/books to delete all books
            var deleteResponse = await _client.DeleteAsync("/api/books");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify that all books were deleted
            var getAllResponse = await _client.GetAsync("/api/books");
            getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await getAllResponse.Content.ReadAsStringAsync();
            var books = JsonConvert.DeserializeObject<List<Book>>(content);
            books.Should().NotBeNull();
            books.Count.Should().Be(0);
        }


        [Fact]
        public async Task TestSortBooksByPropertyAscending()
        {
            var books = new List<Book>
            {
                new Book { Title = "Book C", Author = "Author C", Genre = "Genre C", PublicationYear = 2020, IsAvailable = true },
                new Book { Title = "Book A", Author = "Author A", Genre = "Genre A", PublicationYear = 2019, IsAvailable = true },
                new Book { Title = "Book B", Author = "Author B", Genre = "Genre B", PublicationYear = 2021, IsAvailable = false }
            };

            await AddBooksToDatabase(books);

            await TestSortBooks("/api/books/sort?sortBy=title&sortOrder=asc", b => b.Title);
            await TestSortBooks("/api/books/sort?sortBy=author&sortOrder=asc", b => b.Author);
            await TestSortBooks("/api/books/sort?sortBy=genre&sortOrder=asc", b => b.Genre);
            await TestSortBooks("/api/books/sort?sortBy=publicationyear&sortOrder=asc", b => b.PublicationYear);
        }

        private async Task AddBooksToDatabase(List<Book> books)
        {
            var context = _server.Host.Services.GetRequiredService<BookContext>();
            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }

        private async Task TestSortBooks(string url, Func<Book, object> propertySelector)
        {
            var response = await _client.GetAsync(url);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var sortedBooks = JsonConvert.DeserializeObject<List<Book>>(content);
            sortedBooks.Should().NotBeNull();
            sortedBooks.Count.Should().Be(3);

            var expectedSortedBooks = sortedBooks.OrderBy(propertySelector).ToList();
            sortedBooks.Should().BeEquivalentTo(expectedSortedBooks);
        }

    }
}
