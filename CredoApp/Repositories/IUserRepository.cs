using CredoApp.Models;

namespace CredoApp.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);
        Task<bool> SaveChangesAsync();
    }
}
