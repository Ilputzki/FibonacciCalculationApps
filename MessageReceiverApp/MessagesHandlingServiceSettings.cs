namespace MessageReceiverApp
{
    public class MessagesHandlingServiceSettings<T>
    {
        public int ThreadsNumber { get; set; }

        public T? BaseValue { get; set; }

        public string? TransportationIdPattern { get; set; }
    }
}
