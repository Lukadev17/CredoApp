using CredoApp.Models;

namespace CredoApp.Interfaces
{
    public interface ILoanRepository
    {
        Task<IEnumerable<LoanApplication>> GetAllAsync();
        Task<LoanApplication?> GetByIdAsync(int id);
        Task AddAsync(LoanApplication loan);
        void Update(LoanApplication loan);
        Task<bool> Delete(int id);
        Task<bool> SaveChangesAsync();
    }
}
