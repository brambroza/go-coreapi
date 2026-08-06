using RabbitMQ.Client;

internal static class RabbitMQConnectionFactory
{
    public static ConnectionFactory Create(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedConnectionTimeout = TimeSpan.FromSeconds(
                configuration.GetValue<double?>("RabbitMQ:ConnectionTimeoutSeconds") ?? 5
            ),
        };

        var port = configuration.GetValue<int?>("RabbitMQ:Port");
        if (port is > 0)
            factory.Port = port.Value;

        var userName = configuration["RabbitMQ:UserName"];
        if (!string.IsNullOrWhiteSpace(userName))
            factory.UserName = userName;

        var password = configuration["RabbitMQ:Password"];
        if (!string.IsNullOrWhiteSpace(password))
            factory.Password = password;

        return factory;
    }
}
