using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;

    public ContactRepository(IDbContextFactory<TempusDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Contact?> GetByIdAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Contacts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task<Contact?> GetByEmailAsync(string email, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Contacts
            .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower() && c.UserId == userId);
    }

    public async Task<List<Contact>> GetAllAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Contacts
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Contact> CreateAsync(Contact contact)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Contacts.Add(contact);
        await context.SaveChangesAsync();
        return contact;
    }

    public async Task<Contact> UpdateAsync(Contact contact)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Contacts.Update(contact);
        await context.SaveChangesAsync();
        return contact;
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var contact = await context.Contacts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (contact != null)
        {
            context.Contacts.Remove(contact);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Contact>> SearchAsync(string searchTerm, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var lowerSearchTerm = searchTerm.ToLower();
        return await context.Contacts
            .Where(c => c.UserId == userId &&
                       (c.Name.ToLower().Contains(lowerSearchTerm) ||
                        c.Email.ToLower().Contains(lowerSearchTerm) ||
                        (c.Company != null && c.Company.ToLower().Contains(lowerSearchTerm))))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string email, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Contacts
            .AnyAsync(c => c.Email.ToLower() == email.ToLower() && c.UserId == userId);
    }
}
