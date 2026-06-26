namespace CredoApp.Interfaces
{
    public interface IRabbitMqService
    {
        Task SendLoanToQueue(int loanId);
        Task RemoveLoanFromQueue();
    }
}
