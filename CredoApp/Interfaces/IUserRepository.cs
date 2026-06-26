using CredoApp.Models;

namespace CredoApp.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);
        Task<bool> SaveChangesAsync();
    }
}
