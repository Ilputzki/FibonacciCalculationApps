namespace Sending
{
    public interface IConnection<T>
    {
        string ConnectionString { get; }

        T Connection { get; }
    }
}
