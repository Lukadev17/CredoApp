namespace CredoApp.Interfaces
{
    public interface IRabbitMqService
    {
        void SendLoanToQueue(int loanId);
    }
}
