using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MainIntegrationTests.Tests
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
        public async Task TestYourIntegrationScenario()
        {
            // Add your integration test scenario here
            // For example:
            var response = await _client.GetAsync("/api/books");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var books = JsonConvert.DeserializeObject<List<Book>>(content);
            books.Should().NotBeNull();
        }

        [Fact]
        public async Task TestAddBook()
        {
            var newBook = new Book
            {
                Title = "Sample Book",
                Author = "John Doe",
                Genre = "Fiction",
                PublicationYear = 2023,
                IsAvailable = true
            };

            var jsonString = JsonConvert.SerializeObject(newBook);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/books", httpContent);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task TestUpdateBook()
        {
            // Add test data to the database
            var context = _server.Host.Services.GetRequiredService<BookContext>();
            context.Books.Add(new Book
            {
                Title = "Test Book",
                Author = "Test Author",
                Genre = "Test Genre",
                PublicationYear = 2022,
                IsAvailable = true
            });
            context.SaveChanges();

            // Perform the GET request to /api/books to get the book ID
            var response = await _client.GetAsync("/api/books");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var books = JsonConvert.DeserializeObject<List<Book>>(content);
            books.Should().NotBeNull();
            books.Count.Should().Be(1);
            var bookId = books[0].Id;

            // Update the book
            var updatedBook = new Book
            {
                Id = bookId,
                Title = "Updated Book",
                Author = "Updated Author",
                Genre = "Updated Genre",
                PublicationYear = 2023,
                IsAvailable = false
            };
            var jsonString = JsonConvert.SerializeObject(updatedBook);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var updateResponse = await _client.PutAsync($"/api/books/{bookId}", httpContent);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the book was updated
            var updatedResponse = await _client.GetAsync($"/api/books/{bookId}");
            updatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedContent = await updatedResponse.Content.ReadAsStringAsync();
            var updatedBookResult = JsonConvert.DeserializeObject<Book>(updatedContent);
            updatedBookResult.Should().NotBeNull();
            updatedBookResult.Title.Should().Be("Updated Book");
            updatedBookResult.Author.Should().Be("Updated Author");
            updatedBookResult.Genre.Should().Be("Updated Genre");
            updatedBookResult.PublicationYear.Should().Be(2023);
            updatedBookResult.IsAvailable.Should().Be(false);
        }

        [Fact]
        public async Task TestGetBookByTitle()
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

            // Perform the GET request to /api/books?title={bookTitle}
            var response = await _client.GetAsync($"/api/books?title={testBook.Title}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Deserialize the response content and verify the result
            var content = await response.Content.ReadAsStringAsync();
            var books = JsonConvert.DeserializeObject<List<Book>>(content);
            books.Should().NotBeNull();
            books.Count.Should().Be(1);

            var book = books.FirstOrDefault();
            book.Should().NotBeNull();
            book.Title.Should().Be(testBook.Title);
            book.Author.Should().Be(testBook.Author);
            book.Genre.Should().Be(testBook.Genre);
            book.PublicationYear.Should().Be(testBook.PublicationYear);
            book.IsAvailable.Should().Be(testBook.IsAvailable);
        }

        [Fact]
        public async Task TestGetAllBooks()
        {
            // Add test data to the database
            var context = _server.Host.Services.GetRequiredService<BookContext>();
            context.Books.Add(new Book
            {
                Title = "Test Book",
                Author = "Test Author",
                Genre = "Test Genre",
                PublicationYear = 2022,
                IsAvailable = true
            });
            context.SaveChanges();

            // Perform the GET request to /api/books
            var response = await _client.GetAsync("/api/books");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Deserialize the response content and verify the results
            var content = await response.Content.ReadAsStringAsync();
            var books = JsonConvert.DeserializeObject<List<Book>>(content);
            books.Should().NotBeNull();
            books.Count.Should().Be(1);

            var testBook = books.FirstOrDefault();
            testBook.Should().NotBeNull();
            testBook.Title.Should().Be("Test Book");
            testBook.Author.Should().Be("Test Author");
            testBook.Genre.Should().Be("Test Genre");
            testBook.PublicationYear.Should().Be(2022);
            testBook.IsAvailable.Should().Be(true);
        }

        [Fact]
        public async Task TestDeleteBook()
        {
            // Add test data to the database
            var context = _server.Host.Services.GetRequiredService<BookContext>();
            context.Books.Add(new Book
            {
                Title = "Test Book",
                Author = "Test Author",
                Genre = "Test Genre",
                PublicationYear = 2022,
                IsAvailable = true
            });
            context.SaveChanges();

            // Perform the GET request to /api/books to get the book ID
            var response = await _client.GetAsync("/api/books");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var books = JsonConvert.DeserializeObject<List<Book>>(content);
            books.Should().NotBeNull();
            books.Count.Should().Be(1);
            var bookId = books[0].Id;

            // Delete the book
            var deleteResponse = await _client.DeleteAsync($"/api/books/{bookId}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the book was deleted
            var deletedResponse = await _client.GetAsync($"/api/books/{bookId}");
            deletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        
    }
}
