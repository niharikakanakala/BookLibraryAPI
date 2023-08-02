using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ContactManagementSystem.WebAPI.Data;
using ContactManagementSystem.WebAPI.Models;
using ContactManagementSystem.WebAPI.Services;

namespace ContactManagementSystem.WebAPI.Services
{
    public class ContactService : IContactService
    {
        private readonly ContactContext _contactContext;

        public ContactService(ContactContext contactContext)
        {
            _contactContext = contactContext;
        }

        public async Task<List<Contact>> GetAllContacts()
        {
            return await _contactContext.Contacts.ToListAsync();
        }

        public async Task<Contact> GetContactById(int id)
        {
            return await _contactContext.Contacts.FindAsync(id);
        }

        public async Task<List<Contact>> GetContactsByName(string name)
        {
            return await _contactContext.Contacts
                .Where(c => c.Name.Contains(name))
                .ToListAsync();
        }

        public async Task AddContact(Contact contact)
        {
            _contactContext.Contacts.Add(contact);
            await _contactContext.SaveChangesAsync();
        }

        public async Task UpdateContact(Contact contact)
        {
            _contactContext.Contacts.Update(contact);
            await _contactContext.SaveChangesAsync();
        }

        public async Task DeleteContact(int id)
        {
            var contact = await _contactContext.Contacts.FindAsync(id);
            if (contact != null)
            {
                _contactContext.Contacts.Remove(contact);
                await _contactContext.SaveChangesAsync();
            }
        }

        public async Task DeleteAllContacts()
        {
            // Delete all contacts from the database
            _contactContext.Contacts.RemoveRange(_contactContext.Contacts);
            await _contactContext.SaveChangesAsync();
        }

        public async Task<Contact> GetContactByName(string name)
        {
            return await _contactContext.Contacts
                .FirstOrDefaultAsync(c => c.Name == name);
        }
    }
}
