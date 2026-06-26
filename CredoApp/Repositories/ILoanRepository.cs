using CredoApp.Models;

namespace CredoApp.Repositories
{
    public interface ILoanRepository
    {
        Task<IEnumerable<LoanApplication>> GetAllAsync();
        Task<LoanApplication?> GetByIdAsync(int id);
        Task AddAsync(LoanApplication loan);
        void Update(LoanApplication loan);
        void Delete(LoanApplication loan);
        Task<bool> SaveChangesAsync();
    }
}
