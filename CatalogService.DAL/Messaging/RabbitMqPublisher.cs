using CatalogService.Transversal.Interfaces.Events;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;

namespace CatalogService.DAL.Messaging
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqPublisher> _logger;
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

        public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
        {
            _options = options.Value;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.User,
                Password = _options.Password
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            _channel
                .ExchangeDeclareAsync(exchange: _options.Exchange, type: ExchangeType.Topic, durable: true, autoDelete: false)
                .GetAwaiter()
                .GetResult();
        }

        public async Task PublishAsync(IIntegrationEvent @event, string routingKey, CancellationToken ct = default)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(@event, JsonOpts);

            await _channel.BasicPublishAsync(exchange: _options.Exchange, routingKey: routingKey, body: body);

            _logger.LogInformation("Published event {EventType} with id {EventId} routingKey {RoutingKey}", @event.EventType, @event.EventId, routingKey);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
