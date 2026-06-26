using CredoApp.DTOs;
using CredoApp.Enums;
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
                    Status = LoanStatus.Draft
                };

                await _loanRepository.AddAsync(loan);
                await _loanRepository.SaveChangesAsync();

                return loan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create loan for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> SendLoanToQueueAsync(int loanId)
        {
            try
            {
                var loan = await _loanRepository.GetByIdAsync(loanId);
                if (loan == null)
                {
                    throw new KeyNotFoundException("Loan application not found.");
                }

                if (loan.Status != LoanStatus.Draft)
                {
                    throw new InvalidOperationException("Only draft loans can be submitted.");
                }

                loan.Status = LoanStatus.Submitted;
                _loanRepository.Update(loan);
                await _loanRepository.SaveChangesAsync();

                try
                {
                    await _rabbitMqService.SendLoanToQueue(loanId);
                }
                catch (Exception Ex)
                {
                    
                    _logger.LogWarning("RabbitMQ is not reachable, but loan status  updated in DB. Message: {Msg}", Ex.Message);
                }

                return true; ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send loan {LoanId} to queue", loanId);
                throw;
            }
        }

        public async Task<bool> UpdateLoanAsync(int id, CreateLoanDto dto)
        {
            try
            {
                var loan = await _loanRepository.GetByIdAsync(id);
                if (loan == null) return false;

                if (loan.Status != LoanStatus.Draft)
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

                if (loan.Status != LoanStatus.Submitted)
                {
                    throw new InvalidOperationException("Only Submitted Loans can be reviewed.");
                }

                string normalAction = action.ToLower();
                if (normalAction == "approve")
                {
                    loan.Status = LoanStatus.Approved;
                }
                else if (normalAction == "reject")
                {
                    loan.Status = LoanStatus.Rejected;
                }
                else
                {
                    throw new Exception("Use 'approve' OR 'reject' Actions only");
                }

                _loanRepository.Update(loan);
                await _loanRepository.SaveChangesAsync();

                await _rabbitMqService.RemoveLoanFromQueue();

                return loan.Status.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing loan {LoanId} with action {Action}", id, action);
                throw;
            }
        }
    }
}
