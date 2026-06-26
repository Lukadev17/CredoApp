using CredoApp.Data;
using CredoApp.Interfaces;
using CredoApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CredoApp.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly AppDbContext _context;

        public LoanRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LoanApplication>> GetAllAsync()
        {
            return await _context.LoanApplications.Include(l => l.User).ToListAsync();
        }

        public async Task<LoanApplication?> GetByIdAsync(int id)
        {
            return await _context.LoanApplications.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AddAsync(LoanApplication loan)
        {
            await _context.LoanApplications.AddAsync(loan);
        }

        public void Update(LoanApplication loan)
        {
            _context.LoanApplications.Update(loan);
        }

        public async Task<bool> Delete(int id)
        {
            
            int affectedRows = await _context.LoanApplications
                .Where(l => l.Id == id)
                .ExecuteDeleteAsync();

            return affectedRows > 0;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
