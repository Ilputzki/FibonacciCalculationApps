namespace Sending
{
    public class Message<T>
    {
        public string TransportationId { get; }

        public T Value { get; set; }

        public Message(string transportationId, T value)
        {
            TransportationId = transportationId;
            Value = value;
        }
    }
}
