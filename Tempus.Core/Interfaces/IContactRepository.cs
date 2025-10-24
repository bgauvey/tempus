using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id, string userId);
    Task<Contact?> GetByEmailAsync(string email, string userId);
    Task<List<Contact>> GetAllAsync(string userId);
    Task<Contact> CreateAsync(Contact contact);
    Task<Contact> UpdateAsync(Contact contact);
    Task DeleteAsync(Guid id, string userId);
    Task<List<Contact>> SearchAsync(string searchTerm, string userId);
    Task<bool> ExistsAsync(string email, string userId);
}
