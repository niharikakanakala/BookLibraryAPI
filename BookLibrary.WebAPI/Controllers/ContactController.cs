using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ContactManagementSystem.WebAPI.Data;
using ContactManagementSystem.WebAPI.Models;
using ContactManagementSystem.WebAPI.Services;

namespace ContactManagementSystem.WebAPI.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Contact>>> GetAllContacts()
        {
            var contacts = await _contactService.GetAllContacts();
            return Ok(contacts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> GetContactById(int id)
        {
            var contact = await _contactService.GetContactById(id);
            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<Contact>>> SearchContactsByName([FromQuery] string name)
        {
            var contacts = await _contactService.GetContactsByName(name);
            return Ok(contacts);
        }

        [HttpPost]
        public async Task<IActionResult> AddContact(Contact contact)
        {
            await _contactService.AddContact(contact);
            return CreatedAtAction(nameof(GetContactById), new { id = contact.Id }, contact);
        }

       
      [HttpPut("{id}")]
public async Task<IActionResult> UpdateContact(int id, Contact contact)
{
    if (id != contact.Id)
    {
        return BadRequest();
    }

    var existingContact = await _contactService.GetContactById(id);
    if (existingContact == null)
    {
        return NotFound();
    }

    existingContact.Name = contact.Name;
    existingContact.Email = contact.Email;
    existingContact.PhoneNumber = contact.PhoneNumber;
    existingContact.Address = contact.Address; // Make sure to update the address here
    existingContact.City = contact.City;
    existingContact.Country = contact.Country;

    await _contactService.UpdateContact(existingContact);

    return NoContent();
}


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contact = await _contactService.GetContactById(id);
            if (contact == null)
            {
                return NotFound();
            }

            await _contactService.DeleteContact(id);

            return NoContent();
        }

        [HttpDelete]
public async Task<IActionResult> DeleteAllContacts()
{
    await _contactService.DeleteAllContacts();
    return NoContent();
}

    }
}
