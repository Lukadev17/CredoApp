using CredoApp.DTOs;
using CredoApp.Models;
using CredoApp.Repositories;
using CredoApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CredoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class LoansController : ControllerBase
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IRabbitMqService _rabbitMqService;

        public LoansController(ILoanRepository loanRepository, IRabbitMqService rabbitMqService)
        {
            _loanRepository = loanRepository;
            _rabbitMqService = rabbitMqService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var loans = await _loanRepository.GetAllAsync();
            return Ok(new { success = true, data = loans }); 
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLoanDto dto)
        {
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

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
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ Error: {ex.Message}");
            }

            return Ok(new { success = true, data = loan, message = "Loan sent sucessfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateLoanDto dto)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            if (loan == null) return NotFound(new { message = "Loan not found" });

            
            if (loan.Status != "Draft")
            {
                return BadRequest(new { message = " Edit is impossible, loan already edited " });
            }

            loan.LoanType = dto.LoanType;
            loan.Amount = dto.Amount;
            loan.Currency = dto.Currency;
            loan.PeriodMonths = dto.PeriodMonths;

            _loanRepository.Update(loan);
            await _loanRepository.SaveChangesAsync();

            return Ok(new { success = true, message = "Loan Updated" });
        }

      
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            if (loan == null) return NotFound(new { message = "Loan Not Found" });

            _loanRepository.Delete(loan);
            await _loanRepository.SaveChangesAsync();

            return Ok(new { success = true, message = "Loan Deleted Successfuly" });
        }

        
        [HttpPost("{id}/review")]
        [Authorize(Roles = "Verifier")] 
        public async Task<IActionResult> ReviewLoan(int id, [FromQuery] string action)
        {
            // action  "approve" / "reject"
            var loan = await _loanRepository.GetByIdAsync(id);
            if (loan == null) return NotFound(new { message = "Loan not found" });

            
            if (loan.Status != "Submitted")
            {
                return BadRequest(new { message = "Only Submitted Loans approved" });
            }

            if (action.ToLower() == "approve")
            {
                loan.Status = "Approved";
            }
            else if (action.ToLower() == "reject")
            {
                loan.Status = "Rejected";
            }
            else
            {
                return BadRequest(new { message = "Use 'approve' OR 'reject' Action" });
            }

            _loanRepository.Update(loan);
            await _loanRepository.SaveChangesAsync();

            return Ok(new { success = true, message = $"Loan status has been changed: {loan.Status}" });
        }
    }
}
