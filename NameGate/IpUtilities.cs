using System.Net;

namespace NameGate
{
    public class IpUtilities
    {
        public static bool TryParseAddress(string? address, int defaultPort, out IPEndPoint? endpoint)
        {
            endpoint = null;

            if (address == null)
                return false;

            if (!IPEndPoint.TryParse(address, out endpoint))
            {
                if (IPAddress.TryParse(address, out var ip))
                    endpoint = new(ip, defaultPort);
            }

            if(endpoint == null) return false;

            if (endpoint.Port == 0)
                endpoint.Port = defaultPort;

            return true;
        }
    }
}
