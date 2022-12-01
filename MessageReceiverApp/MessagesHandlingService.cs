using Sending;
using Processing;
using Microsoft.Extensions.Options;

namespace MessageReceiverApp
{
    public class MessagesHandlingService<T> : IHostedService
    {
        private readonly CancellationTokenSource cts = new();

        private readonly int threadsNumber;

        public readonly T baseValue;

        public readonly string transportationIdPattern;

        private readonly IConnection<EasyNetQ.IBus> connection;

        private readonly ISender<T> sender;

        private readonly IProcessor<T> processor;

        private readonly IHostApplicationLifetime lifetime;

        private readonly ILogger logger;

        public MessagesHandlingService(IConnection<EasyNetQ.IBus> connection, ISender<T> sender, IProcessor<T> processor,
            IOptions<MessagesHandlingServiceSettings<T>> options, IHostApplicationLifetime lifetime,
            ILogger<MessagesHandlingService<T>> logger)
        {
            if (options.Value.ThreadsNumber <= 0)
                throw new ArgumentException($"Property value less than or equal or not specified - {nameof(options.Value.ThreadsNumber)}");

            if (options.Value.BaseValue == null)
                throw new ArgumentException($"Property value not specified - {nameof(options.Value.BaseValue)}");

            if (options.Value.TransportationIdPattern == null)
                throw new ArgumentException($"Property value not specified - {nameof(options.Value.TransportationIdPattern)}");

            this.threadsNumber = options.Value.ThreadsNumber;
            this.baseValue = options.Value.BaseValue;
            this.transportationIdPattern = options.Value.TransportationIdPattern;
            this.connection = connection;
            this.sender = sender;
            this.processor = processor;
            this.lifetime = lifetime;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var messages = CreateMessages();

            Parallel.ForEach(messages,
                message =>
                {
                    connection.Connection.PubSub.SubscribeAsync<T>($"subscription.{message.TransportationId}",
                        (number, token) => { return ProcessReceivedMessageAsync(new Message<T>(message.TransportationId, number), cts.Token); },
                        x => x.WithTopic(message.TransportationId), cts.Token)
                    .AsTask().ContinueWith(task =>
                        {
                            if (!task.IsCompletedSuccessfully)
                            {
                                logger.LogError($"RabbitMQ is not responding. Application will be finished." +
                                    $" TransportationID = {message.TransportationId}, Number = {message.Value}");
                                StopApplication();
                            }
                            else
                                SendAsync(message, cts.Token);
                        });
                });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private List<Message<T>> CreateMessages()
        {
            var exclusiveTransportationIdPattern = $"{DateTime.Now.Ticks.ToString("x")}_{transportationIdPattern}";
            var messages = new List<Message<T>>();
            for (var i = 0; i < threadsNumber; i++)
                messages.Add(new Message<T>(string.Format(exclusiveTransportationIdPattern, i.ToString()), baseValue));
            return messages;
        }

        private Task ProcessReceivedMessageAsync(Message<T> message, CancellationToken cancellationToken)
        {
            var logPostfix = $"TransportationID = {message.TransportationId}, Number = {message.Value}";
            logger.LogInformation($"Received message. {logPostfix}");
            try
            {
                var nextValue = processor.Process(message.Value);
                var newMessage = new Message<T>(message.TransportationId, nextValue);
                return SendAsync(newMessage, cancellationToken);
            }
            catch (OverflowException)
            {
                logger.LogInformation($"Received last fibonacci number in range of the type {nameof(T)}. Application will be finished. {logPostfix}");
                StopApplication();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Application will be finished. {logPostfix}");
                StopApplication();
                return Task.CompletedTask;
            }
        }

        private Task SendAsync(Message<T> message, CancellationToken cancellationToken)
        {
            logger.LogInformation($"Send http request. TransportationID = {message.TransportationId}, Number = {message.Value}");
            return sender.SendAsync(message, cancellationToken).ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    logger.LogError(task.Exception, $"Application will be finished. TransportationID = {message.TransportationId}, Number = {message.Value}");
                    StopApplication();
                }
            });
        }

        private void StopApplication()
        {
            cts.Cancel();
            lifetime.StopApplication();
        }
    }
}
