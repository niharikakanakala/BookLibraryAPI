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

namespace MainIntegrationTests.Tests
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
        public async Task TestYourIntegrationScenario()
        {
            // Add your integration test scenario here
            // For example:
            var response = await _client.GetAsync("/api/contacts");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var contacts = JsonConvert.DeserializeObject<List<Contact>>(content);
            contacts.Should().NotBeNull();
        }

        [Fact]
        public async Task TestAddContact()
        {
            var newContact = new Contact
            {
                Name = "John Doe",
                Email = "johndoe@example.com",
                PhoneNumber = "123-456-7890",
                Address = "123 Main St",
                City = "New York",
                Country = "USA"
            };

            var jsonString = JsonConvert.SerializeObject(newContact);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/contacts", httpContent);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
public async Task TestUpdateContact()
{
    // Add test data to the database
    var context = _server.Host.Services.GetRequiredService<ContactContext>();
    context.Contacts.Add(new Contact
    {
        Name = "Test Contact",
        Email = "test@example.com",
        PhoneNumber = "123-456-7890",
        Address = "Test Address",
        City = "Test City",
        Country = "Test Country"
    });
    context.SaveChanges();

    // Perform the GET request to /api/contacts to get the contact ID
    var response = await _client.GetAsync("/api/contacts");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    var contacts = JsonConvert.DeserializeObject<List<Contact>>(content);
    contacts.Should().NotBeNull();
    contacts.Count.Should().Be(1);
    var contactId = contacts[0].Id;

    // Update the contact
    var updatedContact = new Contact
    {
        Id = contactId,
        Name = "Updated Contact",
        Email = "updated@example.com",
        PhoneNumber = "987-654-3210",
        Address = "Updated Address", // Fixed the address here
        City = "Updated City",
        Country = "Updated Country"
    };
    var jsonString = JsonConvert.SerializeObject(updatedContact);
    var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
    var updateResponse = await _client.PutAsync($"/api/contacts/{contactId}", httpContent);
    updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

    // Verify the contact was updated
    var updatedResponse = await _client.GetAsync($"/api/contacts/{contactId}");
    updatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    var updatedContent = await updatedResponse.Content.ReadAsStringAsync();
    var updatedContactResult = JsonConvert.DeserializeObject<Contact>(updatedContent);
    updatedContactResult.Should().NotBeNull();
    updatedContactResult.Name.Should().Be("Updated Contact");
    updatedContactResult.Email.Should().Be("updated@example.com");
    updatedContactResult.PhoneNumber.Should().Be("987-654-3210");
    updatedContactResult.Address.Should().Be("Updated Address");
    updatedContactResult.City.Should().Be("Updated City");
    updatedContactResult.Country.Should().Be("Updated Country");
}

[Fact]
public async Task TestGetContactByName()
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

    // Perform the GET request to /api/contacts?name={contactName}
    var response = await _client.GetAsync($"/api/contacts?name={testContact.Name}");
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    // Deserialize the response content and verify the result
    var content = await response.Content.ReadAsStringAsync();
    var contacts = JsonConvert.DeserializeObject<List<Contact>>(content);
    contacts.Should().NotBeNull();
    contacts.Count.Should().Be(1);

    var contact = contacts.FirstOrDefault();
    contact.Should().NotBeNull();
    contact.Name.Should().Be(testContact.Name);
    contact.Email.Should().Be(testContact.Email);
    contact.PhoneNumber.Should().Be(testContact.PhoneNumber);
    contact.Address.Should().Be(testContact.Address);
    contact.City.Should().Be(testContact.City);
    contact.Country.Should().Be(testContact.Country);
}


        [Fact]
        public async Task TestGetAllContacts()
        {
            // Add test data to the database
            var context = _server.Host.Services.GetRequiredService<ContactContext>();
            context.Contacts.Add(new Contact
            {
                Name = "Test Contact",
                Email = "test@example.com",
                PhoneNumber = "123-456-7890",
                Address = "Test Address",
                City = "Test City",
                Country = "Test Country"
            });
            context.SaveChanges();

            // Perform the GET request to /api/contacts
            var response = await _client.GetAsync("/api/contacts");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Deserialize the response content and verify the results
            var content = await response.Content.ReadAsStringAsync();
            var contacts = JsonConvert.DeserializeObject<List<Contact>>(content);
            contacts.Should().NotBeNull();
            contacts.Count.Should().Be(1);

            var testContact = contacts.FirstOrDefault();
            testContact.Should().NotBeNull();
            testContact.Name.Should().Be("Test Contact");
            testContact.Email.Should().Be("test@example.com");
            testContact.PhoneNumber.Should().Be("123-456-7890");
        }
        
    
        [Fact]
        public async Task TestDeleteContact()
        {
            // Add test data to the database
            var context = _server.Host.Services.GetRequiredService<ContactContext>();
            context.Contacts.Add(new Contact
            {
                Name = "Test Contact",
                Email = "test@example.com",
                PhoneNumber = "123-456-7890",
                Address = "Test Address",
                City = "Test City",
                Country = "Test Country"
            });
            context.SaveChanges();

            // Perform the GET request to /api/contacts to get the contact ID
            var response = await _client.GetAsync("/api/contacts");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var contacts = JsonConvert.DeserializeObject<List<Contact>>(content);
            contacts.Should().NotBeNull();
            contacts.Count.Should().Be(1);
            var contactId = contacts[0].Id;

            // Delete the contact
            var deleteResponse = await _client.DeleteAsync($"/api/contacts/{contactId}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the contact was deleted
            var deletedResponse = await _client.GetAsync($"/api/contacts/{contactId}");
            deletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
