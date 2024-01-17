using Microsoft.Extensions.DependencyInjection;
using NameGate.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameGate.Tests
{
    internal class Services
    {
        public static IServiceProvider GetServiceProvider()
        {
            var sc = new ServiceCollection();
            sc.AddLogging();
            sc.AddScoped<GlobalConfig>(sp => new()
            {
                CacheDirectory = null,
                DataDirectory = null,
                DnsServers = ["1.1.1.1"],
                OisdConfig = new(),
                StevenBlackConfig = new(),
                FireBogConfig = new()
            });
            sc.AddHttpClient();
            sc.AddScoped<DirectoryManager>();

            return sc.BuildServiceProvider();
        }
    }
}
