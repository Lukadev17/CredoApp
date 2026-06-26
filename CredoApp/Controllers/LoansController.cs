using CredoApp.DTOs;
using CredoApp.Interfaces;
using CredoApp.Models;
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
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var loans = await _loanService.GetAllLoansAsync();
            return Ok(new { success = true, data = loans });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLoanDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            var loan = await _loanService.CreateLoanAsync(dto, userId);
            return Ok(new { success = true, data = loan, message = "Loan application submitted successfully." });
        }

        [HttpPost("{id}/send")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> SendToQueue(int id)
        {
            var result = await _loanService.SendLoanToQueueAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Loan not found" });
            }

            return Ok(new { success = true, message = "Loan Submitted Successfully" });

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateLoanDto dto)
        {

            var updated = await _loanService.UpdateLoanAsync(id, dto);
            if (!updated)
            {
                return NotFound(new { message = "Loan not found" });
            }

            return Ok(new { success = true, message = "Loan Updated" });

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _loanService.DeleteLoanAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Loan Not Found" });
            }

            return Ok(new { success = true, message = "Loan Deleted Successfully" });
        }

        [HttpPost("{id}/review")]
        [Authorize(Roles = "Verifier")]
        public async Task<IActionResult> ReviewLoan(int id, [FromQuery] string action)
        {

            var newStatus = await _loanService.ReviewLoanAsync(id, action);
            if (newStatus == null)
            {
                return NotFound(new { message = "Loan not found" });
            }

            return Ok(new { success = true, message = $"Loan status has been changed: {newStatus}" });



        }
    }
}
