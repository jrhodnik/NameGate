using DNS.Server;

namespace NameGate.Services
{
    public class DnsServerBootstrapper(DnsServer _dnsServer, ILogger<DnsServerBootstrapper> _log) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(_dnsServer.Listen);
            _log.LogInformation("Dns server started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _dnsServer.Dispose();
            return Task.CompletedTask;
        }
    }
}
