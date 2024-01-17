using DNS.Protocol;
using DNS.Server;
using Microsoft.AspNetCore.Hosting.Server;
using System.Net;

namespace NameGate.Services
{
    public class QueryLogger : IDisposable
    {
        public event Action<LoggedQuery>? NewQueryLogged;

        private readonly DnsServer _server;

        private readonly List<LoggedQuery> _queries = new();

        public QueryLogger(DnsServer server)
        {
            _server = server;
            _server.Responded += Server_Responded;
        }


        public IEnumerable<LoggedQuery> GetQueries()
        {
            lock (_queries)
            {
                return _queries.ToList();
            }
        }

        private void Server_Responded(object? sender, DnsServer.RespondedEventArgs e)
        {
            var wasBlocked = (e.Response as WrappedResponse)?.WasBlocked ?? false;
            LogQuery(new()
            {
                Request = e.Request,
                Response = e.Response,
                WasBlocked = wasBlocked,
                Endpoint = e.Remote
            });
        }

        private void LogQuery(LoggedQuery query)
        {
            lock (_queries)
            {
                _queries.Add(query);
                while (_queries.Count > 1000)
                    _queries.RemoveAt(0);
            }
            NewQueryLogged?.Invoke(query);
        }

        public void Dispose()
        {
            _server.Responded -= Server_Responded;
        }
    }
    
    public record LoggedQuery
    {
        public required IRequest Request { get; init; }
        public required IResponse Response { get; init; }
        public required bool WasBlocked { get; init; }
        public required IPEndPoint Endpoint { get;init; } 
        public DateTime Timestamp { get; init; } = DateTime.Now;
    }
}
