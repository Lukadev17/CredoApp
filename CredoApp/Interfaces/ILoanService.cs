using CredoApp.DTOs;
using CredoApp.Models;

namespace CredoApp.Interfaces
{
    public interface ILoanService
    {
        Task<IEnumerable<LoanApplication>> GetAllLoansAsync();
        Task<LoanApplication> CreateLoanAsync(CreateLoanDto dto, int userId);
        Task<bool> SendLoanToQueueAsync(int loanId);
        Task<bool> UpdateLoanAsync(int id, CreateLoanDto dto);
        Task<bool> DeleteLoanAsync(int id);
        Task<string> ReviewLoanAsync(int id, string action);

    }
}
