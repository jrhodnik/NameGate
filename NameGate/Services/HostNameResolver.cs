using DNS.Client;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using MudBlazor.Extensions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using ZiggyCreatures.Caching.Fusion;

namespace NameGate.Services
{
    public class HostNameResolver : BackgroundService
    {
        private IFusionCache _cache;
        private readonly DnsServer _dnsServer;
        private readonly ILogger<HostNameResolver> _log;

        private readonly Channel<IPAddress> _queuedLookups;
        private readonly List<IPAddress> _currentlyResolving = new();

        private readonly IPEndPoint _reverseLookupServer;

        public HostNameResolver(IFusionCacheProvider cacheProvider, DnsServer dnsServer, GlobalConfig config, ILogger<HostNameResolver> log)
        {
            _cache = cacheProvider.GetCache("HostNameResolver");
            _dnsServer = dnsServer;
            _log = log;

            _queuedLookups = Channel.CreateBounded<IPAddress>(100);
            _dnsServer.Requested += LookupRequested;

            if (!IpUtilities.TryParseAddress(config.ReverseLookupServer, 53, out var rls) && !IpUtilities.TryParseAddress(config.DnsServers.First(), 53, out rls))
                throw new InvalidOperationException("No valid reverse lookup server address found.");

            _reverseLookupServer = rls ?? throw new InvalidOperationException("Null reverse lookup server.");
        }

        public IPHostEntry? GetHostEntry(IPAddress address)
        {
            var ret = _cache.TryGet<IPHostEntry?>(address.ToString());

            if(!ret.HasValue)
            {
                _queuedLookups.Writer.TryWrite(address);
                return null;
            }

            return ret.Value;
        }

        public async Task<IPHostEntry?> GetHostEntryAsync(IPAddress ip)
        {
            try
            {
                return await _cache.GetOrSetAsync<IPHostEntry?>(ip.ToString(), async ct => await Get(ip),
                    new FusionCacheEntryOptions()
                    {
                        FactoryHardTimeout = TimeSpan.FromSeconds(1)
                    });
            }
            catch(TimeoutException)
            {
                return null;
            }
            catch(Exception ex)
            {
                _log.LogWarning(ex, "Error resolving hoset info: {ip}", ip);
                return null;
            }
        }

        private async Task<IPHostEntry?> Get(IPAddress address)
        {
            try
            {
                ClientRequest request = new ClientRequest(_reverseLookupServer);

                request.Questions.Add(new Question(Domain.PointerName(address), RecordType.PTR));
                request.RecursionDesired = true;

                using var cts = new CancellationTokenSource(3000);
                IResponse response = await request.Resolve(cts.Token);

                var hostName = response.AnswerRecords.OfType<PointerResourceRecord>().FirstOrDefault()?.PointerDomainName.ToString();

                _log.LogDebug("Got host info for {ip}: {hostName}", address, hostName);

                if (hostName == null)
                    return null;

                return new IPHostEntry() { HostName = hostName, AddressList = [address] };
            }
            catch (OperationCanceledException ocx)
            {
                _log.LogDebug(ocx, "Timeout waiting for reverse IP lookup.");
                return null;
            }
            catch (ResponseException rex) when (rex.Response.ResponseCode == ResponseCode.NameError)
            {
                _log.LogDebug(rex, "Lookup yielded no results?");
                return null;
            }
        }

        private async Task Worker(int id, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var ip = await _queuedLookups.Reader.ReadAsync(ct);

                    try
                    {
                        lock (_currentlyResolving)
                        {
                            if (_currentlyResolving.Contains(ip))
                                continue;

                            _currentlyResolving.Add(ip);
                        }

                        using var scope = _log.BeginScope("IP: {ip} Worker: {id}", ip, id);

                        _ = _cache.GetOrSetAsync<IPHostEntry?>(ip.ToString(), async ct => await Get(ip), token: ct);
                    }
                    finally
                    {
                        lock (_currentlyResolving)
                        {
                            _currentlyResolving.Remove(ip);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error in hostname worker.");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var tasks = Enumerable.Range(0, 10).Select(x => Task.Run(() => Worker(x, stoppingToken))).ToList();
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // Normal
            }
        }

        private void LookupRequested(object? sender, DnsServer.RequestedEventArgs e)
        {
            try
            {
                GetHostEntry(e.Remote.Address);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error proactively looking up host.");
            }
        }
    }
}
