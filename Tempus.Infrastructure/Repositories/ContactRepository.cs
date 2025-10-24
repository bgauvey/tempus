using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly TempusDbContext _context;

    public ContactRepository(TempusDbContext context)
    {
        _context = context;
    }

    public async Task<Contact?> GetByIdAsync(Guid id, string userId)
    {
        return await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task<Contact?> GetByEmailAsync(string email, string userId)
    {
        return await _context.Contacts
            .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower() && c.UserId == userId);
    }

    public async Task<List<Contact>> GetAllAsync(string userId)
    {
        return await _context.Contacts
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Contact> CreateAsync(Contact contact)
    {
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();
        return contact;
    }

    public async Task<Contact> UpdateAsync(Contact contact)
    {
        _context.Contacts.Update(contact);
        await _context.SaveChangesAsync();
        return contact;
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var contact = await GetByIdAsync(id, userId);
        if (contact != null)
        {
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Contact>> SearchAsync(string searchTerm, string userId)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _context.Contacts
            .Where(c => c.UserId == userId &&
                       (c.Name.ToLower().Contains(lowerSearchTerm) ||
                        c.Email.ToLower().Contains(lowerSearchTerm) ||
                        (c.Company != null && c.Company.ToLower().Contains(lowerSearchTerm))))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string email, string userId)
    {
        return await _context.Contacts
            .AnyAsync(c => c.Email.ToLower() == email.ToLower() && c.UserId == userId);
    }
}
