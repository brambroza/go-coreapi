using System.ComponentModel.DataAnnotations;
using System.Text;
using goalongapi.Data;
using goalongapi.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class LogProcessorService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly IServiceProvider _serviceProvider;

    public LogProcessorService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        var factory = new ConnectionFactory() { HostName = configuration["RabbitMQ:Host"] };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RabbitMQ Connection Error: {ex.Message}");
            throw;
        }

        _queueName = configuration["RabbitMQ:QueueName"];
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var log = JsonConvert.DeserializeObject<LogRequest>(message);

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                dbContext.LogSystemClick.Add(
                    new LogSystemClick
                    {
                        MenuName = log.MenuName,
                        ObjectName = log.ObjectName,
                        TimeStamp = log.TimeStamp,
                        UserName = log.Username,
                        CmpId = log.CmpId,
                    }
                );

                dbContext.SaveChangesAsync();
            }

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}

public class LogRequest
{
    [Key]
    public long Seq { get; set; }
    public string Username { get; set; }
    public string MenuName { get; set; }
    public string ObjectName { get; set; }
    public string CmpId { get; set; }
    public DateTime TimeStamp { get; set; }
}

public class GetLogRequest
{
    public string Username { get; set; }
    public string MenuName { get; set; }
    public string ObjectName { get; set; }
    public string CmpId { get; set; }
}
