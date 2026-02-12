using StudentEvent.Domain.Entities;
using System.Threading.Tasks;

namespace StudentEvent.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task UpdateAsync(User user);
        Task SaveChangesAsync();
    }
}