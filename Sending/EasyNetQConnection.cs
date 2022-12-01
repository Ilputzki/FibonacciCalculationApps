using EasyNetQ; 

namespace Sending
{
    public class EasyNetQConnection : IConnection<IBus>
    {
        public string ConnectionString { get; }

        public IBus Connection { get; }

        public EasyNetQConnection(string connectionString)
        {
            ConnectionString = connectionString;
            Connection = RabbitHutch.CreateBus(connectionString);
        }
    }
}
