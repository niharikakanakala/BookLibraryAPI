using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ContactManagementSystem.WebAPI.Data;
using ContactManagementSystem.WebAPI.Models;
using ContactManagementSystem.WebAPI;
using Xunit;

namespace SampleIntegrationTests.Tests
{
    public class ContactManagementSystemIntegrationTests
    {
        private TestServer _server;
        private HttpClient _client;

        public ContactManagementSystemIntegrationTests()
        {
            SetUpClient();
        }

        private void SetUpClient()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    var context = new ContactContext(new DbContextOptionsBuilder<ContactContext>()
                        .UseSqlite("DataSource=:memory:")
                        .EnableSensitiveDataLogging()
                        .Options);

                    services.RemoveAll(typeof(ContactContext));
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
public async Task TestGetContactById()
{
    // Add test data to the database
    var context = _server.Host.Services.GetRequiredService<ContactContext>();
    var testContact = new Contact
    {
        Name = "Test Contact",
        Email = "test@example.com",
        PhoneNumber = "123-456-7890",
        Address = "Test Address",
        City = "Test City",
        Country = "Test Country"
    };
    context.Contacts.Add(testContact);
    context.SaveChanges();

    // Perform the GET request to /api/contacts/{contactId}
    var response = await _client.GetAsync($"/api/contacts/{testContact.Id}");
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    // Deserialize the response content and verify the result
    var content = await response.Content.ReadAsStringAsync();
    var contact = JsonConvert.DeserializeObject<Contact>(content);
    contact.Should().NotBeNull();
    contact.Name.Should().Be(testContact.Name);
    contact.Email.Should().Be(testContact.Email);
    contact.PhoneNumber.Should().Be(testContact.PhoneNumber);
    contact.Address.Should().Be(testContact.Address);
    contact.City.Should().Be(testContact.City);
    contact.Country.Should().Be(testContact.Country);
}

[Fact]
public async Task TestDeleteAllContacts()
{
    // Add test data to the database
    var context = _server.Host.Services.GetRequiredService<ContactContext>();
    context.Contacts.Add(new Contact
    {
        Name = "Test Contact 1",
        Email = "test1@example.com",
        PhoneNumber = "111-111-1111",
        Address = "Test Address 1",
        City = "Test City 1",
        Country = "Test Country 1"
    });

    context.Contacts.Add(new Contact
    {
        Name = "Test Contact 2",
        Email = "test2@example.com",
        PhoneNumber = "222-222-2222",
        Address = "Test Address 2",
        City = "Test City 2",
        Country = "Test Country 2"
    });

    context.SaveChanges();

    // Perform the DELETE request to /api/contacts to delete all contacts
    var deleteResponse = await _client.DeleteAsync("/api/contacts");
    deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

    // Verify that all contacts were deleted
    var getAllResponse = await _client.GetAsync("/api/contacts");
    getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

    var content = await getAllResponse.Content.ReadAsStringAsync();
    var contacts = JsonConvert.DeserializeObject<List<Contact>>(content);
    contacts.Should().NotBeNull();
    contacts.Count.Should().Be(0);
}


    }
}
