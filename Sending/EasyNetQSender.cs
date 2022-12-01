using EasyNetQ;

namespace Sending
{
    public class EasyNetQSender<T> : ISender<T>
    {
        private IConnection<IBus> connection;

        public EasyNetQSender(IConnection<IBus> connection)
        {
            this.connection = connection;
        }

        public Task SendAsync(Message<T> message, CancellationToken cancellationToken)
        {
            return connection.Connection.PubSub.PublishAsync<T>(message.Value, message.TransportationId, cancellationToken);
        }
    }
}
