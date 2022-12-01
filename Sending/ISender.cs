namespace Sending
{
    public interface ISender<T>
    {
        public Task SendAsync(Message<T> message, CancellationToken cancellationToken);
    }
}
