using System.Text;
using Newtonsoft.Json;

namespace Sending
{
    public class HttpSender<T> : ISender<T>
    {
        private IConnection<HttpClient> connection;

        public HttpSender(IConnection<HttpClient> connection)
        {
            this.connection = connection;
        }

        public Task SendAsync(Message<T> message, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(message);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            return connection.Connection.PostAsync(connection.ConnectionString, data, cancellationToken);
        }
    }
}
