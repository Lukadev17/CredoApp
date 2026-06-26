using CredoApp.DTOs;
using CredoApp.Interfaces;
using CredoApp.Models;

namespace CredoApp.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<LoanService> _logger;

        public LoanService(ILoanRepository loanRepository, IRabbitMqService rabbitMqService, ILogger<LoanService> logger)
        {
            _loanRepository = loanRepository;
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        public async Task<IEnumerable<LoanApplication>> GetAllLoansAsync()
        {
            return await _loanRepository.GetAllAsync();
        }

        public async Task<LoanApplication> CreateLoanAsync(CreateLoanDto dto, int userId)
        {
            try
            {
                var loan = new LoanApplication
                {
                    UserId = userId,
                    LoanType = dto.LoanType,
                    Amount = dto.Amount,
                    Currency = dto.Currency,
                    PeriodMonths = dto.PeriodMonths,
                    Status = "Draft"
                };

                await _loanRepository.AddAsync(loan);
                await _loanRepository.SaveChangesAsync();

                try
                {
                    _rabbitMqService.SendLoanToQueue(loan.Id);

                    loan.Status = "Submitted";
                    _loanRepository.Update(loan);
                    await _loanRepository.SaveChangesAsync();
                }
                catch (Exception queueEx)
                {
                    _logger.LogWarning(queueEx, "Loan {LoanId} saved but RabbitMQ message failed.", loan.Id);
                }

                return loan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create loan for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateLoanAsync(int id, CreateLoanDto dto)
        {
            try
            {
                var loan = await _loanRepository.GetByIdAsync(id);
                if (loan == null) return false;

                if (loan.Status != "Draft")
                {
                    throw new InvalidOperationException("Edit is impossible, loan already submitted or processed.");
                }

                loan.LoanType = dto.LoanType;
                loan.Amount = dto.Amount;
                loan.Currency = dto.Currency;
                loan.PeriodMonths = dto.PeriodMonths;

                _loanRepository.Update(loan);
                await _loanRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan {LoanId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteLoanAsync(int id)
        {
            try
            {
                return await _loanRepository.Delete(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan {LoanId}", id);
                throw;
            }
        }

        public async Task<string> ReviewLoanAsync(int id, string action)
        {
            try
            {
                var loan = await _loanRepository.GetByIdAsync(id);
                if (loan == null)
                {
                    return null;
                }

                if (loan.Status != "Submitted")
                {
                    throw new InvalidOperationException("Only Submitted Loans can be reviewed.");
                }

                string normalAction = action.ToLower();
                if (normalAction == "approve")
                {
                    loan.Status = "Approved";
                }
                else if (normalAction == "reject")
                {
                    loan.Status = "Rejected";
                }
                else
                {
                    throw new Exception("Use 'approve' OR 'reject' Actions only");
                }

                _loanRepository.Update(loan);
                await _loanRepository.SaveChangesAsync();
                return loan.Status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing loan {LoanId} with action {Action}", id, action);
                throw;
            }
        }
    }
}
