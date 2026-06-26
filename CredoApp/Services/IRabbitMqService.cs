namespace CredoApp.Services
{
    public interface IRabbitMqService
    {
        void SendLoanToQueue(int loanId);
    }
}
