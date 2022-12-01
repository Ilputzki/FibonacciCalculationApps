using Processing;
using Sending;

namespace HttpReceiverApp
{
    public class RequestsHandlingService<T>
    {
        private readonly CancellationTokenSource cts = new();

        private readonly IConnection<EasyNetQ.IBus> connection;

        private readonly ISender<T> sender;

        private readonly IProcessor<T> processor;

        private readonly IHostApplicationLifetime lifetime;

        private readonly ILogger logger;

        public RequestsHandlingService(IConnection<EasyNetQ.IBus> connection, ISender<T> sender, IProcessor<T> processor, 
            IHostApplicationLifetime lifetime, ILogger<RequestsHandlingService<T>> logger)
        {
            this.connection = connection;
            this.sender = sender;
            this.processor = processor;
            this.lifetime = lifetime;
            this.logger = logger;
        }

        public Task ProcessReceivedMessageAsync(Message<T> message)
        {
            var logPostfix = $"TransportationID = {message.TransportationId}, Number = {message.Value}";
            logger.LogInformation($"Received http request. {logPostfix}");
            try
            {
                var nextValue = processor.Process(message.Value);
                var newMessage = new Message<T>(message.TransportationId, nextValue);
                return SendAsync(newMessage, cts.Token);
            }
            catch (OverflowException ex)
            {
                logger.LogInformation($"Received last fibonacci number in range of the type {nameof(T)}. Application will be finished. {logPostfix}");
                StopApplication();
                return Task.FromException(new OverflowException($"The last fibonacci number in range of the type {nameof(T)} was sent", ex));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Application will be finished. {logPostfix}");
                StopApplication();
                return Task.FromException(ex);
            }
        }
        private Task SendAsync(Message<T> message, CancellationToken cancellationToken)
        {
            logger.LogInformation($"Publish message. TransportationID = {message.TransportationId}, Number = {message.Value}");
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
