namespace Sending
{
    public class HttpConnection : IConnection<HttpClient>
    {
        public string ConnectionString { get; }

        public HttpClient Connection { get; }

        public HttpConnection(string connectionString)
        {
            ConnectionString = connectionString;
            Connection = new HttpClient();  
        }
    }
}
