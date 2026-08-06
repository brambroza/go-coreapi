using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

public sealed class RabbitMQService : IDisposable
{
    private readonly object _sync = new();
    private readonly ConnectionFactory _connectionFactory;
    private readonly string _queueName;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQService(IConfiguration configuration)
    {
        _connectionFactory = RabbitMQConnectionFactory.Create(configuration);
        _queueName = configuration["RabbitMQ:QueueName"]
            ?? throw new InvalidOperationException("RabbitMQ:QueueName is not configured");
    }

    public void SendLog(LogRequest log)
    {
        var message = JsonConvert.SerializeObject(log);
        var body = Encoding.UTF8.GetBytes(message);

        lock (_sync)
        {
            EnsureChannel();
            _channel!.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: null,
                body: body
            );
        }
    }

    private void EnsureChannel()
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        CloseSafely();
        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    public void Dispose()
    {
        lock (_sync)
        {
            CloseSafely();
        }
    }

    private void CloseSafely()
    {
        try { _channel?.Close(); } catch { }
        try { _connection?.Close(); } catch { }
        try { _channel?.Dispose(); } catch { }
        try { _connection?.Dispose(); } catch { }
        _channel = null;
        _connection = null;
    }
}
