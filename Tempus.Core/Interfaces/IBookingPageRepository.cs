using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IBookingPageRepository
{
    Task<BookingPage?> GetByIdAsync(Guid id, string userId);
    Task<BookingPage?> GetBySlugAsync(string slug);
    Task<List<BookingPage>> GetAllAsync(string userId);
    Task<List<BookingPage>> GetActiveBookingPagesAsync(string userId);
    Task<BookingPage> CreateAsync(BookingPage bookingPage);
    Task<BookingPage> UpdateAsync(BookingPage bookingPage);
    Task DeleteAsync(Guid id, string userId);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
    Task IncrementBookingCountAsync(Guid bookingPageId);
}
