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
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

    private readonly ConnectionFactory _connectionFactory;
    private readonly string _queueName;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LogProcessorService> _logger;

    public LogProcessorService(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<LogProcessorService> logger)
    {
        _connectionFactory = RabbitMQConnectionFactory.Create(configuration);
        _queueName = configuration["RabbitMQ:QueueName"] ?? string.Empty;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_queueName))
        {
            _logger.LogError("RabbitMQ consumer is disabled because RabbitMQ:QueueName is not configured");
            return;
        }

        // Do not let the first network connection attempt block Host.StartAsync.
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            IConnection? connection = null;
            IModel? channel = null;

            try
            {
                connection = _connectionFactory.CreateConnection();
                channel = connection.CreateModel();
                var activeChannel = channel;
                activeChannel.QueueDeclare(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                activeChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(activeChannel);
                consumer.Received += async (_, ea) =>
                {
                    try
                    {
                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var log = JsonConvert.DeserializeObject<LogRequest>(message);

                        if (log == null)
                        {
                            _logger.LogWarning("Ignoring an empty RabbitMQ log message");
                            TryNack(activeChannel, ea.DeliveryTag, requeue: false);
                            return;
                        }

                        using var scope = _scopeFactory.CreateScope();
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

                        await dbContext.SaveChangesAsync(stoppingToken);
                        if (activeChannel.IsOpen)
                            activeChannel.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Ignoring an invalid RabbitMQ log message");
                        TryNack(activeChannel, ea.DeliveryTag, requeue: false);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        // Application shutdown; an unacknowledged message will return to the queue.
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to persist a RabbitMQ log message");
                        TryNack(activeChannel, ea.DeliveryTag, requeue: true);
                    }
                };

                activeChannel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
                _logger.LogInformation(
                    "RabbitMQ log consumer connected to {Host}:{Port}, queue {QueueName}",
                    _connectionFactory.HostName,
                    _connectionFactory.Port,
                    _queueName
                );

                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "RabbitMQ is unavailable at {Host}:{Port}; the API will keep running and retry in {RetrySeconds} seconds ({ErrorMessage})",
                    _connectionFactory.HostName,
                    _connectionFactory.Port,
                    RetryDelay.TotalSeconds,
                    ex.Message
                );
            }
            finally
            {
                CloseSafely(channel, connection);
            }

            try
            {
                await Task.Delay(RetryDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private static void TryNack(IModel channel, ulong deliveryTag, bool requeue)
    {
        if (channel.IsOpen)
            channel.BasicNack(deliveryTag, multiple: false, requeue: requeue);
    }

    private static void CloseSafely(IModel? channel, IConnection? connection)
    {
        try { channel?.Close(); } catch { }
        try { connection?.Close(); } catch { }
        try { channel?.Dispose(); } catch { }
        try { connection?.Dispose(); } catch { }
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

public class LogTrans
{

    public string Username { get; set; }
    public string TicketId { get; set; }
    public string DocNo { get; set; }
    public string Descriptions { get; set; }
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

public class IVersionInfo
{
    public string? CreateAt { get; set; }
    public int? Seq { get; set; }
    public string Version { get; set; }
    public string Descriptions { get; set; }
    public string CmpId { get; set; }
}
