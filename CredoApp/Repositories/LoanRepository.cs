using CredoApp.Data;
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

        public void Delete(LoanApplication loan)
        {
            _context.LoanApplications.Remove(loan);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
