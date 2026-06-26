using CredoApp.Interfaces;
using RabbitMQ.Client;
using System.Text;

namespace CredoApp.Services
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "loan_applications";

        public async Task SendLoanToQueue(int loanId)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = _hostname };

               
                using var connection = await factory.CreateConnectionAsync();

                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queue: _queueName,
                                                durable: true,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);

                var message = loanId.ToString();
                var body = Encoding.UTF8.GetBytes(message);

                
                await channel.BasicPublishAsync(exchange: "",
                                                routingKey: _queueName,
                                                mandatory: false,
                                                basicProperties: new BasicProperties(),
                                                body: body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ Exception: {ex.Message}");
            }
        }

        public async Task RemoveLoanFromQueue()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = _hostname };
                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

      
                var result = await channel.BasicGetAsync(queue: _queueName, autoAck: true);

                if (result != null)
                {
                    var message = Encoding.UTF8.GetString(result.Body.ToArray());
                    Console.WriteLine($"[RabbitMQ] Loan {message} successfully processed and removed from queue.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ Delete Exception: {ex.Message}");
            }
        }
    }
}
