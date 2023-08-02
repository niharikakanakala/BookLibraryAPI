using System.Collections.Generic;
using System.Threading.Tasks;
using ContactManagementSystem.WebAPI.Models;

namespace ContactManagementSystem.WebAPI.Services
{
    public interface IContactService
    {
        Task<List<Contact>> GetAllContacts();
        Task<Contact> GetContactById(int id);
        Task<List<Contact>> GetContactsByName(string name); // Keep this method for searching contacts by name
        Task AddContact(Contact contact);
        Task UpdateContact(Contact contact);
        Task DeleteContact(int id);
        Task DeleteAllContacts();
        Task<Contact> GetContactByName(string name); // Update the return type to Task<Contact>
    }
}
