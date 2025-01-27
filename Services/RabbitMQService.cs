using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

public class RabbitMQService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;

    public RabbitMQService(IConfiguration configuration)
    {
        var factory = new ConnectionFactory() { HostName = configuration["RabbitMQ:Host"] };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _queueName = configuration["RabbitMQ:QueueName"];

        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    public void SendLog(LogRequest log)
    {
        var message = JsonConvert.SerializeObject(log);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: "",
            routingKey: _queueName,
            basicProperties: null,
            body: body
        );
    }
}
