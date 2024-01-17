using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Net;
using ZiggyCreatures.Caching.Fusion;

namespace NameGate.Services
{
    public class CustomRequestResolver : IRequestResolver
    {
        private readonly BlockListManager _blockListManager;
        private readonly ILogger<CustomRequestResolver> _log;

        private IRequestResolver _defaultResolver = new NullRequestResolver();

        public CustomRequestResolver(BlockListManager blockListManager, GlobalConfig config, ILogger<CustomRequestResolver> log)
        {
            _blockListManager = blockListManager;
            _log = log;

            foreach (var address in config.DnsServers)
            {
                if (!IpUtilities.TryParseAddress(address, 53, out var ep))
                {
                    _log.LogWarning("Error parsing DNS server address: {addr}", address);
                    continue;
                }

                _defaultResolver = new UdpRequestResolver(ep, _defaultResolver);
            }

            if (!config.DnsServers.Any())
                _log.LogWarning("No dns servers defined in config.");
        }

        public async Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
        {
            using var logCtx = _log.BeginScope("Domain: {domain}", request.Questions.FirstOrDefault()?.Name?.ToString());

            var getDefault = () => _defaultResolver.Resolve(request, cancellationToken);

            var endpoint = (request as Request)?.RequestorEndPoint;

            try
            {
                bool blocked = false;
                foreach (var rec in request.Questions)
                {
                    var blockInfo = await _blockListManager.DomainIsBlocked(rec.Name.ToString(), endpoint);

                    if (blockInfo.IsBlocked)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                {
                    var blockedRes = WrappedResponse.FromRequest(request);
                    blockedRes.ResponseCode = ResponseCode.Refused;
                    return blockedRes;
                }

                return await getDefault();
            }
            catch (Exception ex)
            {
                _log.LogDebug(ex, "Error trying to look up domain with blocklists.");
                return await getDefault();
            }
        }
    }

    public class WrappedResponse : Response
    {
        public bool WasBlocked => true;

        public static new WrappedResponse FromRequest(IRequest request)
        {
            WrappedResponse response = new WrappedResponse();
            response.Id = request.Id;
            foreach (Question question in request.Questions)
            {
                response.Questions.Add(question);
            }

            return response;
        }
    }
}
